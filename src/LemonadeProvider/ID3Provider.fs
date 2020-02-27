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

    let inline buildExpr tag =
        fun [tags] -> <@@ (((%%tags:obj) :?> FrameDictionary).[tag]).GetContent() |> unbox @@>

    let inline mkTagPropertyWithComment< ^a> tag comment propName =
        propName
        |> mkReadOnlyProvidedProperty< ^a> (buildExpr tag)
        |>! addXmlComment comment
        |> Some

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

                    // NOTE: The GetFrame method does not really help the client, because when do it return Some/None?
                    //       We need the compiler to gives us a 'dependent type' with properties of the exact frames of the mp3
                    // GetFrame :: (frame: string) -> ID3Frame option
                    //"GetFrame"
                    //|> mkProvidedMethod<ID3Frame option>
                    //    ([ mkProvidedParameter<string> "frame" ])
                    //    // NOTE: We have 2 params here, because the first one is self/this (the FrameDictionary that we wrap)
                    //    // NOTE: frames are treated by the F# compiler as obj, because our toplevel ProvidedTypeDefinition used None for base type
                    //    //       We therefore have to down cast. This is the price for not exposing the FrameDictionary to clients through OO derivation.
                    //    (fun [frames; frame] -> <@@ let tagDict = ((%%frames : obj) :?> FrameDictionary)
                    //                                if tagDict.ContainsKey(%%frame)
                    //                                then Some tagDict.[%%frame]
                    //                                else None @@>)
                    //|>! addXmlComment "Returns a value indicating whether the specified frame was located within the source file."
                    //|> id3ReaderType.AddMember

                    // By using Seq.choose only known/supported frame names are provided as read-only properties
                    fileName
                    |> ID3Reader.readID3Frames
                    |> Seq.choose (fun kvp ->
                                    match kvp.Key.ToUpperInvariant() with
                                    | "APIC" as tag ->
                                        "AttachedPicture"
                                        |> mkReadOnlyProvidedProperty<AttachedPicture> (buildExpr tag)
                                        |>! addXmlComment "Gets the album art attached to the file. Corresponds to the APIC tag."
                                        |> Some
                                    | "MCDI" as tag ->
                                        "CdIdentifier"
                                        |> mkTagPropertyWithComment<string> tag "Gets the CD Identifier. Corresponds to the MCDI tag."
                                    | "POPM" as tag ->
                                        "Popularimeter"
                                        |> mkReadOnlyProvidedProperty<Popularimeter> (buildExpr tag)
                                        |>! addDelayedXmlComment "Gets the Popularimeter data including play count and rating. Corresponds to the POPM tag."
                                        |> Some
                                    | "TALB" as tag ->
                                        "AlbumTitle"
                                        |> mkTagPropertyWithComment<string> tag "Gets the album title. Corresponds to the TALB tag."
                                    | "TIT1" as tag ->
                                        "ContentGroup"
                                        |> mkTagPropertyWithComment<string> tag "Gets the content group. Corresponds to the TIT1 tag."
                                    | "TIT2" as tag ->
                                        "TrackTitle"
                                        |> mkReadOnlyProvidedProperty<string> (buildExpr tag)
                                        |>! addDelayedXmlComment "Gets the track title. Corresponds to the TIT2 tag."
                                        |> Some
                                    | "TIT3" as tag ->
                                        "TrackSubtitle"
                                        |> mkReadOnlyProvidedProperty<string> (buildExpr tag)
                                        |>! addDelayedXmlComment "Gets the track subtitle. Corresponds to the TIT3 tag."
                                        |> Some
                                    | "TRCK" as tag ->
                                        "TrackNumber"
                                        |> mkReadOnlyProvidedProperty<string> (buildExpr tag)
                                        |>! addDelayedXmlComment "Gets the track number. Corresponds to the TRCK tag."
                                        |> Some
                                    | "TYER" as tag ->
                                        "Year"
                                        |> mkReadOnlyProvidedProperty<string> (buildExpr tag)
                                        |>! addDelayedXmlComment "Gets the year the track was released. Corresponds to the TYER tag."
                                        |> Some
                                    | "TPE1" as tag ->
                                        "Performer"
                                        |> mkReadOnlyProvidedProperty<string> (buildExpr tag)
                                        |>! addDelayedXmlComment "Gets the track performer's name. Corresponds to the TPE1 tag."
                                        |> Some
                                    | "TPE2" as tag ->
                                        "Band"
                                        |> mkReadOnlyProvidedProperty<string> (buildExpr tag)
                                        |>! addDelayedXmlComment "Gets the band name. Corresponds to the TPE2 tag."
                                        |> Some
                                    | "TPOS" as tag ->
                                        "SetIdentifier"
                                        |> mkReadOnlyProvidedProperty<string> (buildExpr tag)
                                        |>! addDelayedXmlComment "Gets the track's position within the set. Corresponds to the TPOS tag."
                                        |> Some
                                    | "TPUB" as tag ->
                                        "Publisher"
                                        |> mkReadOnlyProvidedProperty<string> (buildExpr tag)
                                        |>! addDelayedXmlComment "Gets the track publisher's name. Corresponds to the TPUB tag."
                                        |> Some
                                    | "TCOM" as tag ->
                                        "Composer"
                                        |> mkReadOnlyProvidedProperty<string> (buildExpr tag)
                                        |>! addDelayedXmlComment "Gets the track composer's name. Corresponds to the TCOM tag."
                                        |> Some
                                    | "TCON" as tag ->
                                        "ContentType"
                                        |> mkReadOnlyProvidedProperty<string> (buildExpr tag)
                                        |>! addDelayedXmlComment "Gets the track's content type. Corresponds to the TCON tag."
                                        |> Some
                                    | "TCOP" as tag ->
                                        "Copyright"
                                        |> mkReadOnlyProvidedProperty<string> (buildExpr tag)
                                        |>! addDelayedXmlComment "Gets the copyright information for the track. Corresponds to the TCOP tag."
                                        |> Some
                                    | "TLEN" as tag ->
                                        "TrackLength"
                                        |> mkReadOnlyProvidedProperty<string> (buildExpr tag)
                                        |>! addDelayedXmlComment "Gets the length of the track. Corresponds to the TLEN tag."
                                        |> Some
                                    // Unreqognized frame names are ignored
                                    | _ -> None)
                    |> Seq.toList
                    |> id3ReaderType.AddMembers

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
