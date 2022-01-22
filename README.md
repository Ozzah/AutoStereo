# AutoStereo

## Description

AutoStereo is a tool to convert a 2D image into an autostereogram (left and right eye's perspective) using a depth map.

## Building

You need the [.NET 6 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) to build this project, and [.NET 6 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) to run it.

Run `dotnet build src` from the repository root to build the project.

## Running

You can run the project after building it, using the executable that is generated in the `bin` directory, or using `dotnet run --project src/AutoStereo` from the repository root.

You will need to specify some additional arguments when running the program:

---

`-i`, `--Image`      **Required.** Path to the source image

`-d`, `--DepthMap`   **Required.** Path to the depth map

`-o`, `--OutputPath` **Required.** Path to the output the stereo image

`--FlipEyes`         (Default: false) Whether to flip the left and right images

`--FocusPoint`       (Default: 128) The focus point of the stereo image

`--StereoSeparation` (Default: 10) The stereo separation of the stereo image

`--UnderDraw`        (Default: true) Whether to draw the original image underneath the stereo image

`--JpegQuality`      (Default: 100) The JPEG quality if the output format is JPEG (0-100)

---

The supported output formats are JPEG (`.jpg` and `.jpeg` extension), Bitmap (`.bmp` extension), PNG (`.png` extension), GIF (`.gif` extension), or TARGA (`.tga` extension).
If you are writing a JPEG file, you can set the `--JpegQuality` argument.

The focus point determines the point where there is no effective parallax.
Anything closer than the focus point will have parallax in one direction, and anything further than the focus point will have parallax in the opposite direction.
In my own experience, it's better to have the focus point closer to the front, and have parallax behind, rather than in front.

The stereo separation determines how much parallax the depth map produces.
Assuming your focus point is halfway (128), a black pixel on your depth map will be shifted as many pixels in one direction as the stereo separation, and a white pixel will be shift that many pixels in the opposite direction.
The stereo separation is probably specific to your image and the resolution.

Under drawing will draw the original image underneath the stereo image.
This will avoid black spots where there is missing information due to the stereo separation.

### Result

The stereo image is written to the output file.

### Viewing

The principle to view the stereo images is the same as for viewing "Magic Eye" autostereograms.
My approach is to relax my eyes so I am focussing behind the image, however some people prefer to cross their eyes instead (in which case you may need to set the `--FlipEyes` switch to get the correct result).
If you need more help, see [this link](https://www.wikihow.com/View-Stereograms).

## To Do

1. Improve the performance by using pixel row operations rather than pixel-by-pixel.
2. Allow user to specify different depth mapping functions, beyond linear.
3. Add function to try to fill missing data with some content-aware pixels.
