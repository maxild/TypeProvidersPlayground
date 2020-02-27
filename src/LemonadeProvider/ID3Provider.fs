namespace LemonadeProvider

#nowarn "0025" // Incomplete pattern match, because of ([| :? string as fileName |]: obj array) annotation

open System.Collections.Generic
open Microsoft.FSharp.Core.CompilerServices
open ProviderImplementation.ProvidedTypes

// TODO: Move to ProvidedTypesHelpers
module internal Helper =
    // Get the assembly and namespace used to house the provided types
    let thisAssembly = System.Reflection.Assembly.GetExecutingAssembly()
    let rootNamespace = "DidacticCode.ID3"

    // root provided types need namespace/assembly
    let erasedType<'T> assemblyName rootNamespace typeName hideObjectMethods =
        ProvidedTypeDefinition(assemblyName, rootNamespace, typeName, Some(typeof<'T>), hideObjectMethods = hideObjectMethods)

    // ProvidedTypeDefinition has 2 overloads: with or without assm, ns (toplevel vs nested type)
    // See also instantiate below
    let runtimeType<'T> typeName hideObjectMethods =
        ProvidedTypeDefinition(typeName, Some typeof<'T>, hideObjectMethods=hideObjectMethods)

// alias
type FrameDictionary = Dictionary<string, ID3Frame>

// Notes
// Provider built in Pluralsight Course "Building F# Type Providers", by Dave Fancher
// See also https://github.com/davefancher/ID3TagProvider/blob/master/ID3TagProvider/ID3.fs
[<TypeProvider>]
type ID3Provider(config : TypeProviderConfig) as this =
    inherit TypeProviderForNamespaces(config, addDefaultProbingLocation=false)

    // Get the assembly and namespace used to house the provided types
    let thisAssembly = System.Reflection.Assembly.GetExecutingAssembly()
    let rootNamespace = "DidacticCode.ID3"

    // Passing None for the base type indicates System.Object as base type, and has something to do with being an erased TP
    let id3ProviderType = ProvidedTypeDefinition(thisAssembly, rootNamespace, "ID3Provider", None, hideObjectMethods = true)

    // The provided type created here is kind of like a proxy/wrapper into/around the readID3Frames function
    // The second parameter is an inline pattern matching looking for a string in a single element array and binding that element to filename
    //    (typeName: string) (parameterValues: obj array)
    let instantiate typeName ([| :? string as fileName |]: obj array) =
        let id3ReaderType = ProvidedTypeDefinition(thisAssembly, rootNamespace, typeName, None, hideObjectMethods = true)

        // default ctor with no args
        // let ctor =
        //     mkProvidedConstructor
        //         parameters = List.empty,
        //         invokeCode = fun [] -> <@@ fileName |> ID3Reader.readID3Frames @@>)
        // ctor.AddXmlDoc "Creates a reader for the specified file."
        // id3ReaderType.AddMember ctor

        // A more functional (pipeline) way to create and add the ctor to the id3ReaderType

        // ctor :: (fileName: string) -> FrameDictionary
        mkProvidedConstructor
            List.empty
            (fun [] -> <@@ fileName |> ID3Reader.readID3Frames @@>)
        |>! addXmlComment "Creates a reader for the specified file."
        |> id3ProviderType.AddMember

        // GetFrame:: (frame: string) -> ID3Frame option
        "GetFrame"
        |> mkProvidedMethod<ID3Frame option>
            ([ mkProvidedParameter<string> "frame" ])
            // NOTE: We have 2 params here, because the first one is self/this (the FrameDictionary that we wrap)
            // NOTE: frames are treated by the F# compiler as obj, because our toplevel ProvidedTypeDefinition used None for base type
            //       We therefore have to down cast. This is the price for not exposing the FrameDictionary to clients through OO derivation.
            (fun [frames; frame] -> <@@ let tagDict = ((%%frames : obj) :?> FrameDictionary)
                                        if tagDict.ContainsKey(%%frame)
                                        then Some tagDict.[%%frame]
                                        else None @@>)
        |>! addXmlComment "Returns a value indicating whether the specified frame was located within the source file."
        |> id3ProviderType.AddMember

        id3ReaderType

    do
        id3ProviderType.DefineStaticParameters(
            parameters =
                [
                    ProvidedStaticParameter("fileName", typeof<string>)
                ],
            instantiationFunction = instantiate)


    do
        this.AddNamespace(rootNamespace, [id3ProviderType])

[<assembly:TypeProviderAssembly>]
do ()
