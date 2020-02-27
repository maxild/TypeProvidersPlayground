
#r "System.Windows.Forms"
#r "System.Drawing"
#r "../../src/TypeProviders/bin/Debug/netstandard2.0/TypeProviders.dll"

open TypeProviders
open System.Drawing
open System.Windows.Forms

[<Literal>]
let Path = __SOURCE_DIRECTORY__ + @"/../../data/Episode_56_-_You_Must_Take_Care_Of_Yourself_-_Dave_Fancher.mp3"
//let Path = __SOURCE_DIRECTORY__ + @"/../../data/Sting - Shape of My Heart (Leon).mp3"

type AudioSample = ID3Provider<Path>

let sample = AudioSample()

// AttachedPicture, AlbumTitle and TrackTitle is used here
// NOTE: If we load an mp3 file without the underlying tags/frames then the compiler will complain!!!
let showForm (sample: AudioSample) =
    let converter = ImageConverter()

    use image =
        sample.AttachedPicture.Image
        |> converter.ConvertFrom
        :?> Image

    use form =
        new Form(
          BackgroundImage = image,
          BackgroundImageLayout = ImageLayout.Center,
          Height = image.Height,
          Width = image.Width,
          Text = sample.AlbumTitle + ": " + sample.TrackTitle)

    form.ShowDialog() |> ignore

sample |> showForm
