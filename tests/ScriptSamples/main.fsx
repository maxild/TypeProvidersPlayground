#r "../../src/LemonadeProvider/bin/Debug/netstandard2.0/LemonadeProvider.dll"

// TODO: Problem that we have to close the instance of vscode (or visual studio)
//       where FSI instance is running.

open Mavnn.Blog.TypeProvider.Provided

// TODO: The generated type does not work in the repl
// In the REPL type:
// > MyType.MyProperty;;

// But we can use the generated type from within the assembly!!!
// This way we can build the type provider without any problems
printfn "MyType.MyProperty: %s" MyType.MyProperty
