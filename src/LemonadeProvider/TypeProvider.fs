module LemonadeProvider

// See also http://blog.mavnn.co.uk/type-providers-from-the-ground-up/

#nowarn "25" // Incomplete Pattern Matching

open System
open System.Reflection
open Newtonsoft.Json
open ProviderImplementation.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices // defined in FSharp.Core

// we'll need some (OO) classes to deserialize the Json into
// Limitation of earlier version of Newtonsoft.Json)

type Id() =
    member val UniqueId = Guid() with get, set
    member val Name = "" with get, set

type Port() =
    member val Id = Id() with get, set
    member val Type = "" with get, set

type Node() =
    member val Id = Id() with get, set
    member val Ports = Collections.Generic.List<Port>() with get, set

// These are the F# Graph types (the interesting part)

type InputPort = InputPort of Port

type OutputPort = OutputPort of Port

type nodeInstance =
    { Node: Node
      InstanceId: Id
      Config: string }

module NodeInstance =
    let create node name guid config =
        { Node = node
          InstanceId = Id(Name = name, UniqueId = guid)
          Config = config }

// Turning our Json into the our new CLR types is straight forward:
let private nodes =
    JsonConvert.DeserializeObject<seq<Node>>(IO.File.ReadAllText(@"c:\Temp\Graph.json"))
    |> Seq.map (fun node -> node.Id.UniqueId.ToString(), node)
    |> Map.ofSeq

let GetNode id =
    nodes.[id]

let private ports =
    nodes
    |> Map.toSeq
    |> Seq.map (fun (_, node) -> node.Ports)
    |> Seq.concat
    |> Seq.map (fun p -> p.Id.UniqueId.ToString(), p)
    |> Map.ofSeq

let GetPort id =
    ports.[id]


// BasicErasingProvider
[<TypeProvider>]
type MavnnProvider (config : TypeProviderConfig) as this =
    inherit TypeProviderForNamespaces (config, addDefaultProbingLocation=true)

    let ns = "Mavnn.Blog.TypeProvider.Provided"
    let asm = Assembly.GetExecutingAssembly()

    let createTypes' () =
        let myType = ProvidedTypeDefinition(asm, ns, "MyType", Some typeof<obj>)

        let myProp = ProvidedProperty("MyProperty", typeof<string>, isStatic = true,
                                        getterCode = fun _ -> <@@ "Hello world" @@>)
        myType.AddMember(myProp)

        // The invokeCode parameter of the constructors needs to return a quotation
        // that will return the internal representation of the object when it's evaluated.
        let ctor = ProvidedConstructor([], invokeCode = fun _ -> <@@ "My internal state" :> obj @@>)
        myType.AddMember(ctor)

        let ctor2 = ProvidedConstructor(
                        [ProvidedParameter("InnerState", typeof<string>)],
                        invokeCode = fun args -> <@@ (%%(args.[0]):string) :> obj @@>)
        myType.AddMember(ctor2)

        let innerState = ProvidedProperty("InnerState", typeof<string>,
                            getterCode = fun args -> <@@ (%%(args.[0]) :> obj) :?> string @@>)
        myType.AddMember(innerState)

        [myType]

    let addInputPort (inputs : ProvidedTypeDefinition) (port : Port) =
        let port = ProvidedProperty(
                        port.Id.Name,
                        typeof<InputPort>,
                        getterCode = fun args ->
                            let id = port.Id.UniqueId.ToString()
                            <@@ GetPort id |> InputPort @@>)
        inputs.AddMember(port)

    let addOutputPort (outputs : ProvidedTypeDefinition) (port : Port) =
        let port = ProvidedProperty(
                        port.Id.Name,
                        typeof<OutputPort>,
                        getterCode = fun args ->
                            let id = port.Id.UniqueId.ToString()
                            <@@ GetPort id |> OutputPort @@>)
        outputs.AddMember(port)

    let addPorts inputs outputs (portList : seq<Port>) =
        portList
        |> Seq.iter (fun port ->
                        match port.Type with
                        | "input" -> addInputPort inputs port
                        | "output" -> addOutputPort outputs port
                        | _ -> failwithf "Unknown port type for port %s/%s" port.Id.Name (port.Id.UniqueId.ToString()))

    let createNodeType id (node : Node) =
        let nodeType = ProvidedTypeDefinition(asm, ns, node.Id.Name, Some typeof<nodeInstance>)
        let ctor = ProvidedConstructor(
                    [
                        ProvidedParameter("Name", typeof<string>)
                        ProvidedParameter("UniqueId", typeof<Guid>)
                        ProvidedParameter("Config", typeof<string>)
                    ],
                    invokeCode = fun [name;unique;config] -> <@@ NodeInstance.create (GetNode id) (%%name:string) (%%unique:Guid) (%%config:string) @@>)
        nodeType.AddMember(ctor)

        let outputs = ProvidedTypeDefinition("Outputs", Some typeof<obj>, hideObjectMethods = true)
        let outputCtor = ProvidedConstructor([], invokeCode = fun args -> <@@ obj() @@>)
        outputs.AddMember(outputCtor)

        let inputs = ProvidedTypeDefinition("Inputs", Some typeof<obj>, hideObjectMethods = true)
        let inputCtor = ProvidedConstructor([], invokeCode = fun args -> <@@ obj() @@>)
        inputs.AddMember(inputCtor)
        addPorts inputs outputs node.Ports

        // Add the inputs and outputs types of nested types under the Node type
        nodeType.AddMembers([inputs;outputs])

        // Now add some instance properties to expose them on a node instance.
        // TODO: Not working....
//        let outputPorts = ProvidedProperty("OutputPorts", typeof<obj>,
//                            getterCode = fun args -> <@@ obj() @@>)
//
//        let inputPorts = ProvidedProperty("InputPorts", typeof<obj>,
//                            getterCode = fun args -> <@@ obj() @@>)
//
//        nodeType.AddMembers([inputPorts;outputPorts])

        nodeType

    let createTypes () =
        nodes |> Map.map createNodeType |> Map.toList |> List.map (fun (k, v) -> v)

    do
        this.AddNamespace(ns, createTypes())

// TODO: How come it is okay to write this twice in the project???
// By convention attach the assembly level attribute to a do-binding at the end of the file
[<assembly:TypeProviderAssembly>]
do ()
