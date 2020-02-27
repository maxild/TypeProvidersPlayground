// #if INTERACTIVE
// #r @"../test/ComboProvider.dll"
// #endif

module TypeProviders.Tests

open TypeProviders
open Xunit

[<Fact>]
let ``Default constructor should create instance`` () =
    Assert.Equal("My internal state", MyType().InnerState)
