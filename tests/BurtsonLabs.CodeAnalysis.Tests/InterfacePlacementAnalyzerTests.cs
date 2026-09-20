using System.Collections.Immutable;
using BurtsonLabs.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace BurtsonLabs.CodeAnalysis.Tests;

public sealed class InterfacePlacementAnalyzerTests
{
    [Fact]
    public async Task ReportsWhenSingleImplementationIsInAnotherFile()
    {
        var diagnostics = await AnalyzeAsync(
            ("IClock.cs", "public interface IClock { int Hour { get; } }"),
            ("Clock.cs", "public sealed class Clock : IClock { public int Hour => 12; }"));

        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal(InterfacePlacementAnalyzer.CoLocateDiagnosticId, diagnostic.Id);
    }

    [Fact]
    public async Task ReportsWhenInterfaceAppearsBeforeImplementation()
    {
        var diagnostics = await AnalyzeAsync(("Clock.cs", """
            public interface IClock { int Hour { get; } }
            public sealed class Clock : IClock { public int Hour => 12; }
            """));

        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal(InterfacePlacementAnalyzer.OrderDiagnosticId, diagnostic.Id);
    }

    [Fact]
    public async Task AcceptsInterfaceAtBottomOfImplementationFile()
    {
        var diagnostics = await AnalyzeAsync(("Clock.cs", """
            public sealed class Clock : IClock { public int Hour => 12; }
            public interface IClock { int Hour { get; } }
            """));

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task AllowsSharedInterfaceToStandAlone()
    {
        var diagnostics = await AnalyzeAsync(
            ("IClock.cs", "public interface IClock { int Hour { get; } }"),
            ("SystemClock.cs", "public sealed class SystemClock : IClock { public int Hour => 12; }"),
            ("FakeClock.cs", "public sealed class FakeClock : IClock { public int Hour => 8; }"));

        Assert.Empty(diagnostics);
    }

    private static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(
        params (string Path, string Source)[] sources)
    {
        var trees = sources.Select(source => CSharpSyntaxTree.ParseText(source.Source, path: source.Path));
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location));
        var compilation = CSharpCompilation.Create(
            "AnalyzerTests",
            trees,
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        return await compilation
            .WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new InterfacePlacementAnalyzer()))
            .GetAnalyzerDiagnosticsAsync();
    }
}
