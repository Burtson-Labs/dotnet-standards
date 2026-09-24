<div align="center">
  <picture>
    <source media="(prefers-color-scheme: dark)" srcset="https://cdn.burtson.ai/logos/burtson-labs-logo-alt.png" />
    <source media="(prefers-color-scheme: light)" srcset="https://cdn.burtson.ai/logos/burtson-labs-logo.png" />
    <img src="https://cdn.burtson.ai/logos/burtson-labs-logo-alt.png" alt="Burtson Labs" width="220" />
  </picture>

  # .NET Standards

  **Architecture conventions that the compiler can actually enforce.**

  Roslyn analyzers, naming rules, and an opt-in strict build profile for modern .NET repositories.

  [![CI](https://github.com/Burtson-Labs/dotnet-standards/actions/workflows/ci.yml/badge.svg)](https://github.com/Burtson-Labs/dotnet-standards/actions/workflows/ci.yml)
  [![License](https://img.shields.io/badge/license-MIT-a60ee5)](LICENSE)
  [![Roslyn](https://img.shields.io/badge/Roslyn-analyzer-512BD4?logo=dotnet)](https://github.com/dotnet/roslyn)
  [![.NET](https://img.shields.io/badge/.NET-8%2B-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)

  **<img src="https://icons.burtson.ai/svg-accent/hand-heart.svg" align="absmiddle" alt=""> Open standards, courtesy of [Burtson Labs](https://burtson.ai).**
</div>

---

## <picture><source media="(prefers-color-scheme: dark)" srcset="https://icons.burtson.ai/svg-white/blocks.svg"/><img src="https://icons.burtson.ai/svg-black/blocks.svg" align="center" alt=""/></picture> Interfaces belong with the code they describe

When an interface has exactly one implementation in the compilation, Burtson Labs keeps both in one file—with the implementation first:

```csharp
public sealed class Clock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
```

The implementation is the primary thing a reader is looking for. The interface remains easy to discover without forcing every small abstraction into a second file. Once an interface has multiple implementations, it can stand alone as a shared contract.

---

## <picture><source media="(prefers-color-scheme: dark)" srcset="https://icons.burtson.ai/svg-white/list-checks.svg"/><img src="https://icons.burtson.ai/svg-black/list-checks.svg" align="center" alt=""/></picture> Analyzer rules

| Rule | Default | Meaning |
|---|---:|---|
| `BL0001` | Warning | A singly implemented interface should be in its implementation's file. |
| `BL0002` | Warning | A co-located interface should appear after its implementation. |

The package also supplies shared warnings for:

- File-scoped namespaces
- Interfaces prefixed with `I`
- Async methods suffixed with `Async`
- Readonly private fields written as `_camelCase`

The analyzer reasons about implementations visible in the current compilation. Public contracts may have implementations in other assemblies, so consumers can adjust rule severity at that boundary.

---

## <picture><source media="(prefers-color-scheme: dark)" srcset="https://icons.burtson.ai/svg-white/package-build.svg"/><img src="https://icons.burtson.ai/svg-black/package-build.svg" align="center" alt=""/></picture> Install

After the first NuGet release:

```xml
<PackageReference Include="BurtsonLabs.CodeAnalysis" Version="0.1.0">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
</PackageReference>
```

Keep the analyzer private to the consuming project: it should shape builds, not become a runtime dependency or flow into downstream package graphs.

Rule severities can be overridden normally:

```ini
[*.cs]
dotnet_diagnostic.BL0001.severity = error
dotnet_diagnostic.BL0002.severity = error
```

---

## <picture><source media="(prefers-color-scheme: dark)" srcset="https://icons.burtson.ai/svg-white/gauge.svg"/><img src="https://icons.burtson.ai/svg-black/gauge.svg" align="center" alt=""/></picture> Strict mode is explicit

[`config/BurtsonLabs.Strict.props`](config/BurtsonLabs.Strict.props) enables these solution-wide decisions together:

```xml
<AnalysisLevel>latest-recommended</AnalysisLevel>
<EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
<ImplicitUsings>enable</ImplicitUsings>
<Nullable>enable</Nullable>
<TreatWarningsAsErrors>true</TreatWarningsAsErrors>
```

Strict mode is deliberately not activated merely by installing the analyzer. Import or copy the props file only when the entire solution is ready; reusable tooling should not surprise a consumer by changing unrelated compiler behavior.

---

## <picture><source media="(prefers-color-scheme: dark)" srcset="https://icons.burtson.ai/svg-white/beaker.svg"/><img src="https://icons.burtson.ai/svg-black/beaker.svg" align="center" alt=""/></picture> Build and test

```bash
dotnet test tests/BurtsonLabs.CodeAnalysis.Tests/BurtsonLabs.CodeAnalysis.Tests.csproj
dotnet pack src/BurtsonLabs.CodeAnalysis/BurtsonLabs.CodeAnalysis.csproj -c Release
```

The test suite compiles in-memory C# samples and verifies all important placements:

- Single implementation in another file produces `BL0001`.
- Interface above its implementation produces `BL0002`.
- Interface at the bottom of the implementation file passes.
- A shared interface with multiple implementations may stand alone.

---

## <picture><source media="(prefers-color-scheme: dark)" srcset="https://icons.burtson.ai/svg-white/workflow-branch.svg"/><img src="https://icons.burtson.ai/svg-black/workflow-branch.svg" align="center" alt=""/></picture> Contributing

Issues and pull requests are welcome. New analyzer rules should explain the architectural cost they prevent, include positive and negative test cases, and remain useful outside a single Burtson Labs repository.

---

<div align="center">
  <sub>Built in the open by <a href="https://burtson.ai">Burtson Labs</a> · MIT licensed</sub>
</div>
