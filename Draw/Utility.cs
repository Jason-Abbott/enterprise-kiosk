using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Idaho.Draw {
	/// <summary>
	/// Methods for adjusting images and graphics
	/// </summary>
	public class Utility {
		
		/// <summary>
		/// Set alpha value on given color
		/// </summary>
		/// <param name="alpha">Value from 0 to 100 representing opacity</param>
		public static Color AdjustOpacity(Color c, float alpha) {
			int r = c.R; int g = c.G; int b = c.B;
			return Color.FromArgb((int)(2.55 * alpha), r, g, b);
		}

		/// <summary>
		/// Get format for format name
		/// </summary>
		/// <remarks>
		/// ImageFormat can't be converted as an enum and has no parse().
		/// </remarks>
		public static ImageFormat GetFormat(string formatName) {
			switch (formatName.ToLower()) {
				case "png":
				case "image/png": return ImageFormat.Png;
				case "jpg":
				case "jpeg":
				case "image/jpeg": return ImageFormat.Jpeg;
				case "gif": 
				case "image/gif": return ImageFormat.Gif;
			}
			return null;
		}

		#region Convolution

		/// <summary>
		/// Sharpen the image
		/// </summary>
		/// <param name="radius">Radius of the convolution matrix</param>
		/// <param name="intensity">Factor by which calculations are multiplied</param>
		/// <remarks>
		/// This is an unsharp-mask style of sharpening where first a copy of
		/// the image is blurred than that copy is subtracted from the original.
		/// </remarks>
		private static Image Sharpen(Bitmap source, int radius, float intensity) {
			ConvolutionMatrix matrix = ConvolutionMatrix.Gaussian(radius * 2);
			return matrix.ApplyTo(source, true);
		}
		private static Image Sharpen(Image source, int radius, float intensity) {
			return Sharpen(new Bitmap(source), radius, intensity);
		}

		private static Image GaussianBlur(Bitmap source, int matrixSize) {
			ConvolutionMatrix matrix = ConvolutionMatrix.Gaussian(matrixSize);
			return matrix.ApplyTo(source);
		}
		private static Image GaussianBlur(Image source, int matrixSize) {
			return GaussianBlur(new Bitmap(source), matrixSize);
		}

		#endregion

		#region Resize

		/// <summary>
		/// Resize image to fit given dimensions, maintaining aspect ratio
		/// </summary>
		public static Image Resize(Image image, double newWidth, double newHeight,
			int sharpenRadius, float sharpenIntensity, string rotate) {

			Image smallerImage = null;
			double width;
			double height;

			if (string.IsNullOrEmpty(rotate)) {
				width = image.Width;
				height = image.Height;
			} else {
				width = image.Height;
				height = image.Width;
			}
			double widthHeightRatio = width / height;

			// set missing parameter
			if (newWidth == 0) { newWidth = newHeight * widthHeightRatio; }
			else if (newHeight == 0) { newHeight = newWidth / widthHeightRatio; }
		   
			if (width > newWidth | height > newHeight) {
				// image needs to be downsized
				if (width / newWidth > height / newHeight) {
					// to maintain aspect ratio, height must be smaller than passed
					newHeight = (int)newWidth / widthHeightRatio;
				} else {
					// to maintain aspect ratio, width must be smaller than passed
					newWidth = (int)newHeight * widthHeightRatio;
				}
				switch (rotate) {
					case "cw":
						image.RotateFlip(RotateFlipType.Rotate90FlipNone); break;
					case "ccw":
						image.RotateFlip(RotateFlipType.Rotate270FlipNone); break;
					default:
						// forced rotation causes GDI+ to re-read image for cleaner
						// pixels
						image.RotateFlip(RotateFlipType.Rotate180FlipNone);
						image.RotateFlip(RotateFlipType.Rotate180FlipNone);
						break;
				}
				smallerImage = image.GetThumbnailImage(
					(int)newWidth, (int)newHeight, null, IntPtr.Zero);
			} else {
				smallerImage = image;
			}
			return (sharpenRadius > 0) ?
				Sharpen(smallerImage, sharpenRadius, sharpenIntensity) : smallerImage;
		}

		#region From byte array (creates stream)

		public static Image Resize(byte[] imageBytes, int newWidth, int newHeight) {
			return Resize(new MemoryStream(imageBytes), newWidth, newHeight);
		}

		#endregion

		#region From stream (creates image object)

		public static Image Resize(System.IO.Stream imageStream, int newWidth, int newHeight) {
			Image image = Image.FromStream(imageStream);
			return Resize(image, newWidth, newHeight);
		}

		#endregion

		#region From image path (creates FileInfo)

		public static Image Resize(string imagePath, int newWidth, int newHeight, int sharpenRadius, float sharpenIntensity, string rotate) {
			return Resize(new FileInfo(imagePath), newWidth, newHeight,
				sharpenRadius, sharpenIntensity, rotate);
		}
		public static Image Resize(string imagePath, int newWidth, int newHeight, int sharpenRadius, float sharpenIntensity) {
			return Resize(imagePath, newWidth, newHeight, sharpenRadius, sharpenIntensity, string.Empty);
		}
		public static Image Resize(string imagePath, int newWidth, int newHeight) {
			return Resize(imagePath, newWidth, newHeight, 0, 1);
		}

		#endregion

		#region From FileInfo (creates image object)

		/// <summary>
		/// Resize image referenced by FileInfo
		/// </summary>
		/// <param name="file">
		/// FileInfo that points to an image (should either be newly constructed
		/// or updated with the Refresh() method)
		/// </param>
		public static Image Resize(FileInfo file, int newWidth, int newHeight, int sharpenRadius, float sharpenIntensity, string rotate) {
			if (file.Exists) {
				return Resize(Image.FromFile(file.FullName), newWidth, newHeight, sharpenRadius, sharpenIntensity, rotate);
			} else {
				return null;
			}
		}
		public static Image Resize(FileInfo file, int newWidth, int newHeight, int sharpenRadius, float sharpenIntensity) {
			return Resize(file, newWidth, newHeight, sharpenRadius, sharpenIntensity, string.Empty);
		}
		public static Image Resize(FileInfo file, int newWidth, int newHeight) {
			return Resize(file, newWidth, newHeight, 0, 1);
		}

		#endregion

		#region From image object

		public static Image Resize(Image image, double newWidth, double newHeight) {
			return Resize(image, newWidth, newHeight, 0, 1, string.Empty);
		}

		#endregion
		
		#endregion

		#region Compressed JPEG

		/// <summary>
		/// Get encoder information for given MIME type
		/// </summary>
		/// <remarks>Copied from Q324788</remarks>
		private static ImageCodecInfo GetEncoderInfo(string mimeType) {
			ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();
			for (int x = 0; x <= encoders.Length; x++) {
				if (encoders[x].MimeType == mimeType) { return encoders[x]; }
			}
			return null;
		}

		/// <summary>
		/// Save JPEG with 1-100 quality setting
		/// </summary>
		/// <remarks>Copied from Q324788</remarks>
		public static void SaveJpegWithCompression(Bitmap image, string fileName, int quality) {
			EncoderParameters eps = new EncoderParameters(1);
			eps.Param[0] = new EncoderParameter(Encoder.Quality, quality);
			ImageCodecInfo ici = GetEncoderInfo("image/jpeg");
			image.Save(fileName, ici, eps);
		}
		public static void SaveJpegWithCompression(Image image, string fileName, int quality) {
			SaveJpegWithCompression(new Bitmap(image), fileName, quality);
		}

		#endregion

		/// <summary>
		/// Aspect ratio for image
		/// </summary>
		public static float GetAspectRatio(FileInfo file) {
			Image i = Image.FromFile(file.FullName);
			return (i == null) ? 1f : ((float)i.Width / (float)i.Height);
		}
	}
}