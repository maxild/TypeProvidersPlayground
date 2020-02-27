namespace TypeProviders

open ProviderImplementation.ProvidedTypes
open FSharp.Quotations
open FSharp.Core.CompilerServices
open System.Reflection

type SomeRuntimeHelper() =
    static member Help() = "help"

[<AllowNullLiteral>]
type SomeRuntimeHelper2() =
    static member Help() = "help"

[<TypeProvider>]
type SimpleErasingProvider (config : TypeProviderConfig) as this =
    inherit TypeProviderForNamespaces (config, addDefaultProbingLocation=true)

    let ns = "TypeProviders"
    let asm = Assembly.GetExecutingAssembly()

    // check we contain a copy of runtime files, and are not referencing the runtime DLL
    do assert (typeof<SomeRuntimeHelper>.Assembly.GetName().Name = asm.GetName().Name)

    let createTypes () =
        let myType = ProvidedTypeDefinition(asm, ns, "MyType", Some typeof<obj>)

        let myStaticStateProp =
            ProvidedProperty(
                "StaticState",
                typeof<string>,
                isStatic = true,
                getterCode = fun _ -> <@@ "Hello world" @@>)
        myType.AddMember(myStaticStateProp)

        let ctor =
            ProvidedConstructor(
                [],
                invokeCode = fun _ -> <@@ "My internal state" :> obj @@>)
        myType.AddMember(ctor)

        let ctor2
            = ProvidedConstructor(
                [ProvidedParameter("InnerState", typeof<string>)],
                invokeCode = fun args -> <@@ (%%(args.[0]):string) :> obj @@>)
        myType.AddMember(ctor2)

        let myInnerStateProp =
            ProvidedProperty(
                "InnerState",
                typeof<string>,
                getterCode = fun args -> <@@ (%%(args.[0]) :> obj) :?> string @@>)
        myType.AddMember(myInnerStateProp)

        let meth =
            ProvidedMethod(
                "StaticMethod",
                [],
                typeof<SomeRuntimeHelper>,
                isStatic=true,
                invokeCode = (fun _ -> <@@ SomeRuntimeHelper() @@>))
        myType.AddMember(meth)

        let meth2 =
            ProvidedMethod(
                "StaticMethod2",
                [],
                typeof<SomeRuntimeHelper2>,
                isStatic=true,
                invokeCode = (fun _ -> Expr.Value(null, typeof<SomeRuntimeHelper2>)))
        myType.AddMember(meth2)

        [myType]

    do
        this.AddNamespace(ns, createTypes())

[<TypeProvider>]
type SimpleGenerativeProvider (config : TypeProviderConfig) as this =
    inherit TypeProviderForNamespaces (config)

    let ns = "TypeProviders"
    let asm = Assembly.GetExecutingAssembly()

    // check we contain a copy of runtime files, and are not referencing the runtime DLL
    do assert (typeof<SomeRuntimeHelper>.Assembly.GetName().Name = asm.GetName().Name)

    let createType typeName (count:int) =
        let asm = ProvidedAssembly()
        let myType = ProvidedTypeDefinition(asm, ns, typeName, Some typeof<obj>, isErased=false)

        let ctor =
            ProvidedConstructor(
                [],
                invokeCode = fun args -> <@@ "My internal state" :> obj @@>)
        myType.AddMember(ctor)

        let ctor2 =
            ProvidedConstructor(
                [ProvidedParameter("InnerState", typeof<string>)],
                invokeCode = fun args -> <@@ (%%(args.[1]):string) :> obj @@>)
        myType.AddMember(ctor2)

        for i in 1 .. count do
            let prop =
                ProvidedProperty(
                    "Property" + string i,
                    typeof<int>,
                    getterCode = fun args -> <@@ i @@>)
            myType.AddMember(prop)

        let meth =
            ProvidedMethod(
                "StaticMethod",
                [],
                typeof<SomeRuntimeHelper>,
                isStatic = true,
                invokeCode = (fun args -> Expr.Value(null, typeof<SomeRuntimeHelper>)))
        myType.AddMember(meth)

        asm.AddTypes [ myType ]

        myType

    let myParamType =
        let t = ProvidedTypeDefinition(asm, ns, "GenerativeProvider", Some typeof<obj>, isErased=false)
        t.DefineStaticParameters(
            [ProvidedStaticParameter("Count", typeof<int>)],
            fun typeName args -> createType typeName (unbox<int> args.[0]))
        t

    do
        this.AddNamespace(ns, [myParamType])


[<assembly:TypeProviderAssembly()>]
do ()
