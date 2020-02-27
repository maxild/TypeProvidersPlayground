#r "../../src/TypeProviders/bin/Debug/netstandard2.0/TypeProviders.dll"

// TODO: Problem that we have to close the instance of vscode (or visual studio)
//       where FSI instance is running.

open TypeProviders

// TODO: The generated type does not work in the repl
// In the REPL type:
// > MyType.MyProperty;;

// But we can use the generated type from within the assembly!!!
// This way we can build the type provider without any problems
printfn "MyType.StaticState: %s" MyType.StaticState

let thing = MyType()
printfn "thing.InnerState = %s" thing.InnerState

let thing2 = MyType("Some other text")
printfn "thing2.InnerState = %s" thing2.InnerState
