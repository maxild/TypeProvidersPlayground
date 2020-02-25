module Mavnn.Blog.TypeProvider
//namespace LemonadeProvider

//module Mavnn.Blog.TypeProvider

open System.Reflection
open ProviderImplementation.ProvidedTypes
open FSharp.Core.CompilerServices

// BasicErasingProvider
[<TypeProvider>]
type MavnnProvider (config : TypeProviderConfig) as this =
    inherit TypeProviderForNamespaces (config, addDefaultProbingLocation=true)

    let ns = "Mavnn.Blog.TypeProvider.Provided"
    let asm = Assembly.GetExecutingAssembly()

    let createTypes () =
        let myType = ProvidedTypeDefinition(asm, ns, "MyType", Some typeof<obj>)
        let myProp = ProvidedProperty("MyProperty", typeof<string>, isStatic = true, getterCode = fun args -> <@@ "Hello world" @@>)
        myType.AddMember(myProp)
        [myType]

    do
        this.AddNamespace(ns, createTypes())

[<assembly:TypeProviderAssembly>]
do ()
