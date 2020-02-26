module LemonadeProvider.ID3Provider

// Notes
// * 'ProvidedTypes.fs' (from the SDK) contains helper types and helper
//   methods for implementing ITypeProvider interface. Its a little ironic
//   that the helpers have a very Object-Oriented feel, when F# is an FP lang
// * Many types in the SDK extend the built-in Reflection types (MemberInfo,
//   PropertyInfo etc)
//
// https://github.com/davefancher/ID3TagProvider/blob/master/ID3TagProvider/ID3.fs

open Microsoft.FSharp.Core.CompilerServices

// Provider built in Pluralsight Course "Building F# Type Providers", by Dave Fancher
[<TypeProvider>]
type ID3Provider() =
    class end

[<assembly:TypeProviderAssembly>]
do ()
