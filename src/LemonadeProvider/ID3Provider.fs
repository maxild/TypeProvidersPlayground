namespace LemonadeProvider

#nowarn "0025" // Incomplete pattern match, because of ([| :? string as fileName |]: obj array) annotation

open System.Collections.Generic
open Microsoft.FSharp.Core.CompilerServices
open ProviderImplementation.ProvidedTypes

[<AutoOpen>]
module internal Helper =
    // Get the assembly and namespace used to house the provided types
    let thisAssembly = System.Reflection.Assembly.GetExecutingAssembly()
    let rootNamespace = "LemonadeProvider"

// alias
type FrameDictionary = Dictionary<string, ID3Frame>

[<AutoOpen>]
module internal TypedID3 =

    // toplevel type (the provider)
    let erasedType<'T> assemblyName rootNamespace typeName hideObjectMethods =
        ProvidedTypeDefinition(assemblyName, rootNamespace, typeName, Some(typeof<'T>), hideObjectMethods = hideObjectMethods)

    // Nested type
    let runtimeType<'T> typeName hideObjectMethods =
        ProvidedTypeDefinition(typeName, Some typeof<'T>, hideObjectMethods = hideObjectMethods)

    // createTypes method
    let typedID3 () : ProvidedTypeDefinition =

        // Passing None for the base type indicates System.Object as base type, and has something to do with being an erased TP
        let id3ProviderType = erasedType<obj> thisAssembly rootNamespace "ID3Provider" true
        id3ProviderType.DefineStaticParameters(
            parameters =
                [
                    ProvidedStaticParameter("fileName", typeof<string>)
                ],
            // The provided type created here is kind of like a proxy/wrapper into/around the readID3Frames function
            // The second parameter is an inline pattern matching looking for a string in a single element array and
            // binding that element to a filename
            //    (typeName: string) (parameterValues: obj array)
            instantiationFunction = (fun (typeName: string) (parameterValues: obj array) ->
                match parameterValues with
                | [| :? string as fileName |] ->

                    // TODO: Could this be runtimeType???
                    let id3ReaderType = erasedType<obj> thisAssembly rootNamespace typeName true

                    // ctor :: (fileName: string) -> FrameDictionary
                    mkProvidedConstructor
                        List.empty
                        (fun [] -> <@@ fileName |> ID3Reader.readID3Frames @@>)
                    |>! addXmlComment "Creates a reader for the specified file."
                    |> id3ReaderType.AddMember

                    // GetFrame :: (frame: string) -> ID3Frame option
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
                    |> id3ReaderType.AddMember

                    id3ReaderType

                | _ -> failwith "unexpected parameter values")
        )
        id3ProviderType

// Provider built in Pluralsight Course "Building F# Type Providers", by Dave Fancher
// See also https://github.com/davefancher/ID3TagProvider/blob/master/ID3TagProvider/ID3.fs
[<TypeProvider>]
type ID3Provider(cfg : TypeProviderConfig) =
    inherit TypeProviderForNamespaces(cfg, rootNamespace, [TypedID3.typedID3()])

[<assembly:TypeProviderAssembly>]
do ()
