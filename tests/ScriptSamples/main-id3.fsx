#r "../../src/TypeProviders/bin/Debug/netstandard2.0/TypeProviders.dll"

open TypeProviders

[<Literal>]
let Path = __SOURCE_DIRECTORY__ + @"/../../data/Episode_56_-_You_Must_Take_Care_Of_Yourself_-_Dave_Fancher.mp3"

type AudioSample = ID3Provider<Path>

let sample = AudioSample()

// title
sample.Copyright |> printfn "Copyright = %s"
sample.Performer |> printfn "Performer = %s"
sample.TrackTitle |> printfn "TrackTitle = %s"
sample.AlbumTitle |> printfn "AlbumTitle = %s"
sample.Year |> printfn "Year = %s"
sample.ContentType |> printfn "ContentType = %s"
sample.Composer |> printfn "Composer = %s"

printfn "Press enter to exit..."
System.Console.ReadLine() |> ignore
