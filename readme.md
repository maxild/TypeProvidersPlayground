
## Technical Notes

The SDK can be found [here](https://github.com/fsprojects/FSharp.TypeProviders.SDK)

* `taskkill /IM "dotnet.exe" /F` is your friend when building.
* Facades (`netstandard.dll`, `System.Reflection.dll` and `System.Runtime.dll` from the deprecated package `NETStandard.Library.NETFramework`) are not needed any more.
* `netfx.props` is only needed for building Full Framework on Mono (travis). This should not be neede because `netstandard2.0`
should be the only TFM is most cases, and can be build using .NET Core SDK on windows, mac and linux.
* Use the latest `ProvidedTypes.fs/fsi` from the [SDK](https://github.com/fsprojects/FSharp.TypeProviders.SDK) project. Paket can
help here, because it allow you to reference files on Github.
* Terminology
    * TPRTC - Type Provider Referenced Component, e.g. MyProvider.dll
        * This is the component referenced by #r or -r: on the command line or other configration of a host tool.
        * May be the same physical file as the TPDTC.
        * Contains either a `TypeProviderAssembly()` attribute indicating that this component is also a TPDTC, or `TypeProviderAssembly("MyProvider.DesignTime.dll")` attribute indicating that the name of the design time component.
        * A type provider package may have multiple such DLLs for different target platforms (in the `lib` folder, see below)
        * TPRTCs are normally .NET Standard 2.0
    * TPDTC - Type Provider Design Time Component, e.g. MyProvider.DesignTime.dll.
        * The DLL that gets loaded into host tools (devenv, FSAC, FSI, FSC etc).
        * May be the same physical file as the TPRTC.
        * This component includes the `ProvidedTypes.fs/fsi` files from the type provider SDK.
        * TPDTCs are (today) normally .NET Standard 2.0
* Check out the examples in the SDK project (`TPDTC` is the compile/design time assembly, and `TPRTC` is the runtime assembly):
    * Combo provider (TPDTC and TPRTC in separate projects/assemblies)
    * Basic provider (TPDTC and TPRTC in single project/assembly)
* Target the TPRTC to netstandard2.0
* Target the TPDTC to netstandard2.0
* The following compile-time constants exist
    * IS_DESIGNTIME
* The typical nuget package layout for a provider that has combined design-time and runtime components is:

```
lib/netstandard2.0
    MyProvider.dll // TPRTC and TPDTC
    netstandard.dll // Extra facade (not needed anymore)
    System.Runtime.dll // Extra facade (not needed anymore)
    System.Reflection.dll // Extra facade (not needed anymore)
```

* The typical nuget package layout for a provider that has separate design-time and runtime components is:

```
lib/netstandard2.0/
    MyProvider.dll // TPRTC are placed in the usual location (lib/tfm)
    MyProvider.DesignTime.dll // TPDTC in the legacy location alongside TPRTC (see below)

lib/typeproviders/fsharp41/
    netstandard2.0/
        MyProvider.DesignTime.dll // TPDTC in the future-proof location

    netcoreapp2.0/
        MyProvider.DesignTime.dll // .NET Core App 2.0 TPDTC
```

* It is important that the design-time assemblies you use (if any) are not loaded at runtime. To ensure this does not happen, when you distribute a Nuget package for your Type Provider you must provide an explicit list of project references for consumers to include. If you do not, every assembly you publish in the package will be included, which can lead to design-type only references being loaded at runtime. That is, an explicit .nuspec file will be needed with an explicit `<references>` node (so that only the TPRTC gets added as a reference)

## How the TPDTC is found and loaded

See [this](https://github.com/fsharp/fslang-design/blob/master/tooling/FST-1003-loading-type-provider-design-time-components.md) FS-RFC.

1. When executing using .NET Core the compiler looks in this order
```
   ...\typeproviders\fsharpNN\netcoreapp2.0\MyDesignTime.dll
   ...\tools\fsharpNN\netcoreapp2.0\MyDesignTime.dll
   ...\typeproviders\fsharpNN\netstandard2.0\MyDesignTime.dll
   ...\tools\fsharpNN\netstandard2.0\MyDesignTime.dll
   MyDesignTime.dll
```

2. When executing using .NET Framework the compiler looks in this order
```
    ...\typeproviders\fsharpNN\net461\MyDesignTime.dll
    ...\tools\fsharpNN\net461\MyDesignTime.dll
    ...\typeproviders\fsharpNN\net46\MyDesignTime.dll
    ...\tools\fsharpNN\net46\MyDesignTime.dll
    ...
    ...\typeproviders\fsharpNN\netstandard2.0\MyDesignTime.dll
    ...\tools\fsharpNN\netstandard2.0\MyDesignTime.dll
    MyDesignTime.dll
```

When we use `...` we mean a recursive upwards directory search looking for a directory names typeproviders or tools respectively, stopping when we find a directory name packages or a directory root.

When we use `fsharpNN` we mean a successive search backwards for fsharp42, fsharp41 etc. Putting a TPDTC in fsharp41 means the TPDTC is suitable to load into F# 4.1 tooling and later, and has the right to minimally assume FSharp.Core 4.4.1.0.

NOTE: Today FSharp.Core and F# versions are aligned at 4.7.

## Code Quotations

This feature is similar to `Expression<T>` trees in C#3 (LINQ).

This feature is used heavily when defining provided methods, properties, constructors etc.

Docs can be found [here](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/code-quotations)

The (FSharp.Quotations.Evaluator)[https://github.com/fsprojects/FSharp.Quotations.Evaluator] project can be used to evaluate F# quotations. It provides support for evaluating and executing F# expression objects.

Splicing enables you to combine literal code quotations with expressions that you have created programmatically or from another code quotation. The `%` and `%%` operators enable you to add an F# expression object into a code quotation.  You use the `%` operator to insert a typed expression object into a typed quotation; you use the `%%` operator to insert an untyped expression object into an untyped quotation.

Splicing is "string interpolation" for F# quotation objects (a.k.a expressions)

Travesal of F# expression expressions are done using pattern matching. The following modules (defined in ???) contains active patterns

* `Microsoft.FSharp.Quotations`: ExprShape
* `Microsoft.FSharp.Quotations.Patterns`: Patterns
* `Microsoft.FSharp.Quotations.DerivedPatterns`: DerivedPatterns
