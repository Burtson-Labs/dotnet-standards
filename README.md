# Burtson Labs .NET Standards

Roslyn analyzers and opt-in build conventions for .NET projects, published by Burtson Labs for anyone to use.

## Interface placement

Burtson Labs keeps an interface beside its implementation when the interface has only one implementation in the compilation:

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

The implementation is the primary thing a reader is looking for, so it comes first. An interface with multiple implementations may live in its own file.

The analyzer provides:

| Rule | Meaning |
|---|---|
| `BL0001` | A singly implemented interface should be in the implementation's file. |
| `BL0002` | A co-located interface should appear after its implementation. |

## Consume

After the first NuGet release:

```xml
<PackageReference Include="BurtsonLabs.CodeAnalysis" Version="0.1.0">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
</PackageReference>
```

The package also supplies warnings for file-scoped namespaces, `I`-prefixed interfaces, `Async`-suffixed async methods, and `_camelCase` readonly private fields.

`config/BurtsonLabs.Strict.props` is deliberately opt-in. Import or copy it only when a solution is ready to enable nullability, recommended analysis, code-style enforcement, and warnings-as-errors together.

## Build

```bash
dotnet test tests/BurtsonLabs.CodeAnalysis.Tests/BurtsonLabs.CodeAnalysis.Tests.csproj
dotnet pack src/BurtsonLabs.CodeAnalysis/BurtsonLabs.CodeAnalysis.csproj -c Release
```

## Scope

The analyzer reasons about implementations visible in the current compilation. Public interfaces may have implementations in other assemblies, so teams can adjust `BL0001` and `BL0002` severity in their own `.editorconfig` when that boundary matters.

## License

MIT. Courtesy of Burtson Labs.
