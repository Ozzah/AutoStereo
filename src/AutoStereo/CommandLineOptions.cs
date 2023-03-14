using CommandLine;

namespace Ozzah.AutoStereo;

public class CommandLineOptions
{
	[Option(
		longName: "Image",
		shortName: 'i',
		Required = true,
		HelpText = "Path to the source image")]
	public string? SourceImagePath { get; set; }

	[Option(
		longName: "DepthMap",
		shortName: 'd',
		Required = true,
		HelpText = "Path to the depth map")]
	public string? DepthMapPath { get; set; }

	[Option(
		longName: "OutputPath",
		shortName: 'o',
		Required = true,
		HelpText = "Path to the output the stereo image")]
	public string? OutputPath { get; set; }

	[Option(
		longName: "FlipEyes",
		Required = false,
		Default = false,
		HelpText = "Whether to flip the left and right images")]
	public bool FlipEyes { get; set; }

	[Option(
		longName: "FocusPoint",
		Required = false,
		Default = 128,
		HelpText = "The focus point of the stereo image")]
	public int FocusPoint { get; set; }

	[Option(
		longName: "StereoSeparation",
		Required = false,
		Default = 10,
		HelpText = "The stereo separation of the stereo image")]
	public int StereoSeparation { get; set; }

	[Option(
		longName: "UnderDraw",
		Required = false,
		Default = true,
		HelpText = "Whether to draw the original image underneath the stereo image")]
	public bool UnderDraw { get; set; }

	[Option(
		longName: "JpegQuality",
		Required = false,
		Default = 100,
		HelpText = "The JPEG quality if the output format is JPEG (0-100)")]
	public int JpegQuality { get; set; }
}
