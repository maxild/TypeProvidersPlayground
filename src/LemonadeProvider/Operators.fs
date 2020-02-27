namespace LemonadeProvider

[<AutoOpen>]
module CoreExtensions =
  let inline tee fn x = x |> fn |> ignore; x
  let inline (|>!) x fn = tee fn x
