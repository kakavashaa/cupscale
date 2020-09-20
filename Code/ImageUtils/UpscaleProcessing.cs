using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cupscale.IO;
using ImageMagick;
using Paths = Cupscale.IO.Paths;

namespace Cupscale
{
	internal class UpscaleProcessing
	{
		public enum Format
		{
			PngOpti,
			PngFast,
			JpegHigh,
			JpegMed,
			WeppyHigh,
			WeppyLow,
			BMP,
			TGA,
			DDS
		}

		//public static Button upscaleBtn;

		public static void ChangeOutputExtensions(string newExtension)
		{
			string path = Paths.imgOutPath;
			DirectoryInfo d = new DirectoryInfo(path);
			FileInfo[] files = d.GetFiles("*", SearchOption.AllDirectories);
			FileInfo[] array = files;
			foreach (FileInfo file in array)
			{
				file.MoveTo(file.FullName.Substring(0, file.FullName.Length - 4));
			}
			FileInfo[] array2 = files;
			foreach (FileInfo file2 in array2)
			{
				file2.MoveTo(Path.ChangeExtension(file2.FullName, newExtension));
			}
		}

		public static async Task ConvertImagesToOriginalFormat()
		{
			string path = Paths.imgOutPath;
			DirectoryInfo d = new DirectoryInfo(path);
			FileInfo[] files = d.GetFiles("*", SearchOption.AllDirectories);
			FileInfo[] array = files;
			foreach (FileInfo file in array)
			{
				file.MoveTo(file.FullName.Substring(0, file.FullName.Length - 4));
			}
			FileInfo[] array2 = files;
			foreach (FileInfo file2 in array2)
			{
				if (GetTrimmedExtension(file2) == "png")
				{
					break;
				}
				Format format = Format.PngOpti;
				if (GetTrimmedExtension(file2) == "jpg" || GetTrimmedExtension(file2) == "jpeg")
				{
					format = Format.JpegHigh;
				}
				if (GetTrimmedExtension(file2) == "webp")
				{
					format = Format.WeppyHigh;
				}
				if (GetTrimmedExtension(file2) == "bmp")
				{
					format = Format.BMP;
				}
				if (GetTrimmedExtension(file2) == "tga")
				{
					format = Format.TGA;
				}
				if (GetTrimmedExtension(file2) == "dds")
				{
					format = Format.DDS;
				}
				await ConvertImage(file2.FullName, format, false, false, true);
			}
		}

		private static string GetTrimmedExtension(FileInfo file)
		{
			return file.Extension.ToLower().Replace(".", "");
		}

		public static async Task ConvertImages(string path, Format format, bool removeAlpha = false, bool preprocess = false, bool appendExtension = false, bool delSource = true)
		{
			DirectoryInfo d = new DirectoryInfo(path);
			FileInfo[] files = d.GetFiles("*", SearchOption.AllDirectories);
			FileInfo[] array = files;
			foreach (FileInfo file in array)
			{
				Logger.Log("Converting " + file.Name + " to " + format.ToString() + ", appendExtension = " + appendExtension);
				await ConvertImage(file.FullName, format, removeAlpha, appendExtension, delSource);
				Logger.Log("Done converting this image");
			}
			Logger.Log("Done converting images");
		}

		public static async Task ConvertImage(string path, Format format, bool fillAlpha, bool appendExtension, bool deleteSource = true)
		{
			Logger.Log("ConvertImage: Loading MagickImage from " + path);
			MagickImage img = new MagickImage(path);
			Logger.Log("Converting: " + img.ToString() + " - Target Format: " + format.ToString() + " - DeleteSource: " + deleteSource);
			string ext = "png";
			if (format == Format.PngOpti)
			{
				img.Format = MagickFormat.Png;
				img.Quality = 70;
			}
			if (format == Format.PngFast)
			{
				img.Format = MagickFormat.Png;
				img.Quality = 20;
			}
			if (format == Format.JpegHigh)
			{
				img.Format = MagickFormat.Jpeg;
				img.Quality = 95;
				ext = "jpg";
			}
			if (format == Format.JpegMed)
			{
				img.Format = MagickFormat.Jpeg;
				img.Quality = 80;
				ext = "jpg";
			}
			if (format == Format.WeppyHigh)
			{
				img.Format = MagickFormat.WebP;
				img.Quality = 92;
				ext = "webp";
			}
			if (format == Format.WeppyLow)
			{
				img.Format = MagickFormat.WebP;
				img.Quality = 80;
				ext = "webp";
			}
			if (format == Format.BMP)
			{
				img.Format = MagickFormat.Bmp;
				ext = "bmp";
			}
			if (format == Format.TGA)
			{
				img.Format = MagickFormat.Tga;
				ext = "tga";
			}
			if (format == Format.DDS)
			{
				img.Format = MagickFormat.Dds;
				ext = "dds";
			}
            if (fillAlpha)
            {
				MagickImage colorImg = new MagickImage(new MagickColor("#" + Config.Get("alphaBgColor")), img.Width, img.Height);
				colorImg.Composite(img, Gravity.Center, CompositeOperator.Over);
				// img.ColorAlpha(new MagickColor("#" + Config.Get("alphaBgColor")));	// Might not work correctly for DDS n stuff?
			}
			if (appendExtension)
			{
				string extension = Path.GetExtension(path);
				string outPath = Path.ChangeExtension(path, null) + extension + "." + ext;
				Logger.Log("Appending old extension; writing image to " + outPath);
				img.Write(outPath);
				if (deleteSource && outPath != path)
				{
					Logger.Log("Deleting source file: " + path);
					File.Delete(path);
				}
			}
			else
			{
				img.Write(Path.ChangeExtension(path, ext));
				Logger.Log("Writing image to " + Path.ChangeExtension(path, ext));
				if (deleteSource && !(Path.ChangeExtension(path, ext) == path))
				{
					Logger.Log("Deleting source file: " + path);
					File.Delete(path);
				}
			}
			await Task.Delay(1);
		}

		public static async Task ConvertImageTo(string inPath, string outPath, Format format, bool fillAlpha, bool appendExtension, bool deleteSource = true)
		{
			Logger.Log("ConvertImage: Loading MagickImage from " + inPath);
			MagickImage img = new MagickImage(inPath);
			Logger.Log("Converting: " + img.ToString() + " - Target Format: " + format.ToString() + " - DeleteSource: " + deleteSource);
			string ext = "png";
			if (format == Format.PngOpti)
			{
				img.Format = MagickFormat.Png;
				img.Quality = 70;
			}
			if (format == Format.PngFast)
			{
				img.Format = MagickFormat.Png;
				img.Quality = 20;
			}
			if (format == Format.JpegHigh)
			{
				img.Format = MagickFormat.Jpeg;
				img.Quality = 95;
				ext = "jpg";
			}
			if (format == Format.JpegMed)
			{
				img.Format = MagickFormat.Jpeg;
				img.Quality = 80;
				ext = "jpg";
			}
			if (format == Format.WeppyHigh)
			{
				img.Format = MagickFormat.WebP;
				img.Quality = 92;
				ext = "webp";
			}
			if (format == Format.WeppyLow)
			{
				img.Format = MagickFormat.WebP;
				img.Quality = 80;
				ext = "webp";
			}
			if (format == Format.BMP)
			{
				img.Format = MagickFormat.Bmp;
				ext = "bmp";
			}
			if (format == Format.TGA)
			{
				img.Format = MagickFormat.Tga;
				ext = "tga";
			}
			if (format == Format.DDS)
			{
				img.Format = MagickFormat.Dds;
				ext = "dds";
			}
			if (fillAlpha)
			{
				img.ColorAlpha(new MagickColor("#" + Config.Get("alphaBgColor")));
			}
			if (appendExtension)
			{
				string extension = Path.GetExtension(inPath);
				//string outPath = Path.ChangeExtension(inPath, null) + extension + "." + ext;
				Logger.Log("Appending old extension; writing image to " + outPath);
				img.Write(outPath);
				if (deleteSource && outPath != inPath)
				{
					Logger.Log("Deleting source file: " + inPath);
					File.Delete(inPath);
				}
			}
			else
			{
				img.Write(Path.ChangeExtension(outPath, ext));
				Logger.Log("Writing image to " + Path.ChangeExtension(outPath, ext));
				if (deleteSource && !(Path.ChangeExtension(outPath, ext) == inPath))
				{
					Logger.Log("Deleting source file: " + inPath);
					File.Delete(inPath);
				}
			}
			await Task.Delay(1);
		}
	}
}
