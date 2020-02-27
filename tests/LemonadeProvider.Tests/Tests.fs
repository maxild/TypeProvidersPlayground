// #if INTERACTIVE
// #r @"../test/ComboProvider.dll"
// #endif

module LemonadeProvider.Tests

open LemonadeProvider
open Xunit

[<Fact>]
let ``Default constructor should create instance`` () =
    Assert.Equal("My internal state", MyType().InnerState)
