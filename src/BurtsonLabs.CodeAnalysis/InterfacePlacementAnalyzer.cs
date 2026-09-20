using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace BurtsonLabs.CodeAnalysis;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InterfacePlacementAnalyzer : DiagnosticAnalyzer
{
    public const string CoLocateDiagnosticId = "BL0001";
    public const string OrderDiagnosticId = "BL0002";

    private static readonly DiagnosticDescriptor CoLocateRule = new(
        CoLocateDiagnosticId,
        "Co-locate a singly implemented interface",
        "Interface '{0}' has one implementation in this compilation and should live in the same file as '{1}'",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "An interface with one implementation belongs beside that implementation. Interfaces shared by multiple implementations may stand alone.",
        customTags: WellKnownDiagnosticTags.CompilationEnd);

    private static readonly DiagnosticDescriptor OrderRule = new(
        OrderDiagnosticId,
        "Place a co-located interface after its implementation",
        "Interface '{0}' should be declared after '{1}' in the file",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Burtson Labs keeps the primary implementation first and its single-use interface at the bottom of the file.",
        customTags: WellKnownDiagnosticTags.CompilationEnd);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(CoLocateRule, OrderRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(startContext =>
        {
            var interfaces = new ConcurrentBag<INamedTypeSymbol>();
            var implementations = new ConcurrentBag<INamedTypeSymbol>();

            startContext.RegisterSymbolAction(symbolContext =>
            {
                var type = (INamedTypeSymbol)symbolContext.Symbol;
                if (type.ContainingType is not null || !HasSourceLocation(type))
                {
                    return;
                }

                if (type.TypeKind == TypeKind.Interface)
                {
                    interfaces.Add(type);
                }
                else if (type.TypeKind is TypeKind.Class or TypeKind.Struct)
                {
                    implementations.Add(type);
                }
            }, SymbolKind.NamedType);

            startContext.RegisterCompilationEndAction(endContext =>
                AnalyzeCompilation(endContext, interfaces, implementations));
        });
    }

    private static void AnalyzeCompilation(
        CompilationAnalysisContext context,
        IEnumerable<INamedTypeSymbol> interfaces,
        IEnumerable<INamedTypeSymbol> implementations)
    {
        var allImplementations = implementations.ToImmutableArray();
        foreach (var contract in interfaces)
        {
            var implementers = allImplementations
                .Where(candidate => candidate.AllInterfaces.Any(
                    implemented => SymbolEqualityComparer.Default.Equals(implemented, contract)))
                .ToImmutableArray();

            if (implementers.Length != 1)
            {
                continue;
            }

            var implementation = implementers[0];
            var interfaceLocation = FirstSourceLocation(contract);
            var implementationLocations = implementation.Locations.Where(location => location.IsInSource).ToArray();
            if (interfaceLocation is null || implementationLocations.Length == 0)
            {
                continue;
            }

            var coLocatedImplementation = implementationLocations.FirstOrDefault(
                location => location.SourceTree == interfaceLocation.SourceTree);
            if (coLocatedImplementation is null)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    CoLocateRule,
                    interfaceLocation,
                    contract.Name,
                    implementation.Name));
                continue;
            }

            if (interfaceLocation.SourceSpan.Start < coLocatedImplementation.SourceSpan.End)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    OrderRule,
                    interfaceLocation,
                    contract.Name,
                    implementation.Name));
            }
        }
    }

    private static bool HasSourceLocation(INamedTypeSymbol symbol) =>
        FirstSourceLocation(symbol) is not null;

    private static Location? FirstSourceLocation(INamedTypeSymbol symbol) =>
        symbol.Locations.FirstOrDefault(location =>
            location.IsInSource &&
            location.SourceTree?.FilePath.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase) != true);
}
