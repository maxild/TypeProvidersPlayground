#r "../../src/LemonadeProvider/bin/Debug/netstandard2.0/LemonadeProvider.dll"

open LemonadeProvider

[<Literal>]
let Path = __SOURCE_DIRECTORY__ + @"/../../data/Episode_56_-_You_Must_Take_Care_Of_Yourself_-_Dave_Fancher.mp3"

type AudioSample = ID3Provider<Path>

let sample = AudioSample()

// title
sample.GetFrame "TIT2" |> printfn "%A"
// val it : Option<ID3Frame> =
//   Some (TIT2 "56: You Must Take Care of Yourself with Dave Fancher")

// image
sample.GetFrame("APIC") |> printfn "%A"
// val it : Option<ID3Frame> =
//   Some
//     (APIC
//        { TextEncoding = 0uy
//          MimeType = "image/jpeg"
//          PictureType = 0uy
//          Description = ""
//          Image = ...

System.Console.ReadLine() |> ignore
