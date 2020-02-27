namespace DidacticCode.ID3

module internal Helper =
    // Get the assembly and namespace used to house the provided types
    let thisAssembly = System.Reflection.Assembly.GetExecutingAssembly()
    let rootNamespace = "DidacticCode.ID3"


// Notes
// * 'ProvidedTypes.fs' (from the SDK) contains helper types and helper
//   methods for implementing ITypeProvider interface. Its a little ironic
//   that the helpers have a very Object-Oriented feel, when F# is an FP lang
// * Many types in the SDK extend the built-in Reflection types (MemberInfo,
//   PropertyInfo etc)
//
// https://github.com/davefancher/ID3TagProvider/blob/master/ID3TagProvider/ID3.fs

open Microsoft.FSharp.Core.CompilerServices
open ProviderImplementation.ProvidedTypes

// Provider built in Pluralsight Course "Building F# Type Providers", by Dave Fancher
// Notes:
//   1. We don't need 'cfg: TypeProviderConfig' on the ctor
//   2. root provided types need namespace/assembly
[<TypeProvider>]
type ID3Provider(config : TypeProviderConfig) as this =
    inherit TypeProviderForNamespaces(config, addDefaultProbingLocation=false)

    // Get the assembly and namespace used to house the provided types
    let thisAssembly = System.Reflection.Assembly.GetExecutingAssembly()
    let rootNamespace = "DidacticCode.ID3"

    // Passing None for the base type indicates System.Object as base type, and has something to do with being an erased TP
    let id3ProviderType = ProvidedTypeDefinition(thisAssembly, rootNamespace, "ID3Provider", None, hideObjectMethods = true)

    do
        this.AddNamespace(rootNamespace, [id3ProviderType])

[<assembly:TypeProviderAssembly>]
do ()
