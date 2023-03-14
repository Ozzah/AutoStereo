using CommandLine;
using ShellProgressBar;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace Ozzah.AutoStereo;

public class Program
{
	public static void Main(string[] args)
	{
		var parserResult = Parser
			.Default
			.ParseArguments<CommandLineOptions>(args);

		parserResult.WithParsed(CreateStereoImage);
	}

	static void CreateStereoImage(CommandLineOptions options)
	{
		using var sourceImage = ReadImage(options.SourceImagePath!).CloneAs<Rgba32>();
		using var depthMap = ReadImage(options.DepthMapPath!).CloneAs<L8>();

		if (sourceImage.Width != depthMap.Width || sourceImage.Height != depthMap.Height)
		{
			throw new ApplicationException("The source image and depth map do not have the same dimensions");
		}

		using var stereoImage = new Image<Rgba32>(Configuration.Default, sourceImage.Width * 2, sourceImage.Height);

		if (options.UnderDraw)
		{
			UnderDraw(sourceImage, stereoImage);
		}

		DrawStereoPixels(depthMap, sourceImage, stereoImage, options);
		SaveStereoImage(options, stereoImage);
	}

	static Image ReadImage(string imagePath)
	{
		if (!File.Exists(imagePath))
		{
			throw new FileNotFoundException($"'{imagePath}' was not found");
		}

		using var imageSource = File.OpenRead(imagePath);
		return Image.Load(imageSource);
	}

	static void UnderDraw(Image<Rgba32> sourceImage, Image<Rgba32> stereoImage)
	{
		using var progressBar = new ProgressBar(
			sourceImage.Width * sourceImage.Height,
			"Underdrawing");

		for (var x = 0; x < sourceImage.Width; x++)
		{
			for (var y = 0; y < sourceImage.Height; y++)
			{
				stereoImage[x, y] = sourceImage[x, y];
				stereoImage[x + sourceImage.Width, y] = sourceImage[x, y];

				progressBar.Tick();
			}
		}
	}

	static void DrawStereoPixels(
		Image<L8> depthMap,
		Image<Rgba32> sourceImage,
		Image<Rgba32> stereoImage,
		CommandLineOptions options)
	{
		using var progressBar = new ProgressBar(256, "Generating stereo image");

		for (var layer = 0; layer < 256; layer++)
		{
			DrawStereoPixelsForLayer(depthMap, sourceImage, stereoImage, options, layer);
			progressBar.Tick();
		}
	}

	static void DrawStereoPixelsForLayer(
		Image<L8> depthMap,
		Image<Rgba32> sourceImage,
		Image<Rgba32> stereoImage,
		CommandLineOptions options,
		int layer)
	{
		var flipFactor = options.FlipEyes ? -1.0 : 1.0;

		for (var y = 0; y < depthMap.Height; y++)
		{
			for (var x = 0; x < sourceImage.Width; x++)
			{
				var depth = (int)depthMap.Frames[0][x, y].PackedValue;
				if (depth != layer)
				{
					continue;
				}

				var offset = flipFactor * options.StereoSeparation * (options.FocusPoint - layer) / 128.0;

				var _xL = x + offset;
				var _xR = x - offset;

				if (_xL >= 0 && _xL < sourceImage.Width)
				{
					stereoImage[x, y] = GetSubPixel(sourceImage, _xL, y);
				}
				if (_xR >= 0 && _xR < sourceImage.Width)
				{
					stereoImage[x + sourceImage.Width, y] = GetSubPixel(sourceImage, _xR, y);
				}
			}
		}
	}

	static Rgba32 GetSubPixel(Image<Rgba32> image, double x, int y)
	{
		var xLeft = Math.Floor(x);
		var xRight = Math.Ceiling(x);

		var leftPixel = image[(int)Math.Max(0, xLeft), y];
		var rightPixel = image[(int)Math.Min(image.Width - 1, xRight), y];

		if (Math.Abs(xLeft - xRight) < 1E-3)
		{
			return leftPixel;
		}

		var red = LinearInterpolate(
			xLeft,
			leftPixel.R,
			xRight,
			rightPixel.R,
			x);

		var green = LinearInterpolate(
			xLeft,
			leftPixel.G,
			xRight,
			rightPixel.G,
			x);

		var blue = LinearInterpolate(
			xLeft,
			leftPixel.B,
			xRight,
			rightPixel.B,
			x);

		var alpha = LinearInterpolate(
			xLeft,
			leftPixel.A,
			xRight,
			rightPixel.A,
			x);

		return new Rgba32(
			(byte)Math.Max(0, Math.Min(255, (int)Math.Round(red))),
			(byte)Math.Max(0, Math.Min(255, (int)Math.Round(green))),
			(byte)Math.Max(0, Math.Min(255, (int)Math.Round(blue))),
			(byte)Math.Max(0, Math.Min(255, (int)Math.Round(alpha))));
	}

	static double LinearInterpolate(double x1, double y1, double x2, double y2, double x)
	{
		var gradient = (y2 - y1) / (x2 - x1);
		var offset = y1 - gradient * x1;
		return gradient * x + offset;
	}

	static void SaveStereoImage(CommandLineOptions options, Image<Rgba32>? stereoImage)
	{
		var extension = new FileInfo(options.OutputPath!).Extension;

		if (extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
		    extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase))
		{
			stereoImage.SaveAsJpeg(
				options.OutputPath,
				new JpegEncoder() { Quality = options.JpegQuality });
			return;
		}
		if (extension.Equals(".png", StringComparison.OrdinalIgnoreCase))
		{
			stereoImage.SaveAsPng(options.OutputPath);
			return;
		}
		if (extension.Equals(".bmp", StringComparison.OrdinalIgnoreCase))
		{
			stereoImage.SaveAsBmp(options.OutputPath);
			return;
		}
		if (extension.Equals(".gif", StringComparison.OrdinalIgnoreCase))
		{
			stereoImage.SaveAsGif(options.OutputPath);
			return;
		}
		if (extension.Equals(".tga", StringComparison.OrdinalIgnoreCase))
		{
			stereoImage.SaveAsTga(options.OutputPath);
			return;
		}

		throw new FormatException($"Unspported output format '{extension}'");
	}
}
