using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace Idaho.Draw {
	/// <summary>
	/// Represents array of factors used in convolutions
	/// </summary>
	/// <remarks>
	/// Article on convolution filters for sharpening, blurring:
	/// http://www.codeproject.com/cs/media/csharpfilters.asp
	/// http://www.codeguru.com/cpp/g-m/gdi/gdi/article.php/c3675/
	/// http://www.codeproject.com/cs/media/Image_Processing_Lab.asp
	/// http://www.codersource.net/published/view/279/guassian_smoothing_in_csharp.aspx
	/// http://www.codeproject.com/bitmap/ImageConvolution.asp
	/// http://www.miszalok.de/Samples/IP/gauss_filter/simple_gauss_filter.htm
	/// http://www.miszalok.de/Samples/IP/gauss_filter/fast_gauss_filter.htm
	/// </remarks>
	internal class ConvolutionMatrix {
		private float[,] _matrix;
		private int _size;
		private int _radius;
		private float _total;
		private bool _includeEdges = true;
		private bool _preserveTransparency = true;

		internal int Size { get { return _size; } }
		internal int Radius { get { return _radius; } }
		internal float Total { get { return _total; } }

		/// <summary>
		/// Indicate whether edge pixels should be convolved
		/// </summary>
		/// <remarks>
		/// Only pixels greater than the matrix radius from the image edge
		/// can be fully convolved. Set this to true to perform partial
		/// convolution for pixels closer to the edge. Otherwise these
		/// pixels are not processed.
		/// </remarks>
		internal bool IncludeEdges { set { _includeEdges = value; } }

		/// <summary>
		/// Indicate whether opacity value (alpha) should be unchanged from source image
		/// </summary>
		internal bool PreserveTransparency { set { _preserveTransparency = value; } }

		internal ConvolutionMatrix(int size) {
			_size = size |= 1;					// force odd
			_radius = _size / 2;
			_matrix = new float[_size, _size];	// always square
		}

		/// <summary>
		/// Apply convolution to given image
		/// </summary>
		/// <param name="invert">Whether to invert the resulting convolution</param>
		/// <param name="intensity">Value by which to multiply inversion strength</param>
		internal Bitmap ApplyTo(Bitmap imageSource, bool invert, float intensity) {
			ColorMatrix source = new ColorMatrix(imageSource);
			ColorMatrix target = ColorMatrix.Blank(source.Width, source.Height);
			float a, r, g, b;
			float factor = 0.0f;
			int offsetX, offsetY;
			int sourceY, sourceX, matrixY, matrixX;
			int width = source.Width, height = source.Height;
			int startX = 0, startY = 0;

			if (!_includeEdges) { height -= _radius; width -= _radius; startX = startY = _radius; }


			for (sourceY = startY; sourceY < height; sourceY++) {
				for (sourceX = startX; sourceX < width; sourceX++) {
					a = r = g = b = 0.0f;

					if (!(_preserveTransparency && source.IsTransparent(sourceX, sourceY))) {
						// do not convolve alpha channel
						a = source.Color[sourceX, sourceY, ColorMatrix.Channel.Alpha];

						for (matrixY = -_radius; matrixY <= _radius; matrixY++) {
							// now analyze pixels under matrix
							offsetY = Idaho.Utility.Constrain(sourceY + matrixY, 0, height - 1);

							for (matrixX = -_radius; matrixX <= _radius; matrixX++) {
								factor = _matrix[matrixY + _radius, matrixX + _radius];
								offsetX = Idaho.Utility.Constrain(sourceX + matrixX, 0, width - 1);
								// track sum of all color values separately in area
								// covered by matrix
								r += factor * source.Color[offsetX, offsetY, ColorMatrix.Channel.Red];
								g += factor * source.Color[offsetX, offsetY, ColorMatrix.Channel.Green];
								b += factor * source.Color[offsetX, offsetY, ColorMatrix.Channel.Blue];
							}
						}
						r /= _total; g /= _total; b /= _total;
					}
					// apply convolved color to single pixel under consideration
					target.SetPixel(sourceX, sourceY, a, r, g, b);
				}
			}

			if (invert) {
				source.CombineWith(target, new ColorMatrix.CombineDelegate(Invert), intensity);
				return source.ToBitmap();
			} else {
				return target.ToBitmap();
			}
		}
		internal Bitmap ApplyTo(Bitmap imageSource, bool invert) { return this.ApplyTo(imageSource, invert, 1); }
		internal Bitmap ApplyTo(Bitmap imageSource) { return this.ApplyTo(imageSource, false, 1); }

		/// <summary>
		/// Used as a delegate to combine color matrices
		/// </summary>
		/// <remarks>
		/// The second color (channel) is typically the result of some operation on the first.
		/// This function will invert the effect so that a difference between the values
		/// of N will become -N. Optionally, a factor X can be applied so that the
		/// change is from n to -XN.
		/// </remarks>
		private float Invert(float c1, float c2, float factor) {
			return (factor + 1) * c1 - factor * c2;
		}

		/// <summary>
		/// Create an instance of a matrix populated with gaussian values
		/// </summary>
		internal static ConvolutionMatrix Gaussian(int size, float centerFactor) {
			ConvolutionMatrix matrix = new ConvolutionMatrix(size);
			matrix.MakeGuassian(centerFactor);
			return matrix;
		}
		internal static ConvolutionMatrix Gaussian(int size) {
			return Gaussian(size, 1.0f);
		}

		/// <summary>
		/// Populate matrix with values for gaussian convolution
		/// </summary>
		/// <remarks>
		/// The formula is from W. Kovalevski and V. Miszalok
		/// http://www.miszalok.de/Samples/IP/gauss_filter/simple_gauss_filter.htm
		/// </remarks>
		/// <param name="centerFactor">Default value for matrix center</param>
		private void MakeGuassian(double centerFactor) {
			if (_size > 3) { centerFactor = -2 * _radius * _radius / Math.Log(0.01); }

			for (int x = 0; x < _size; x++) {
				for (int y = 0; y < _size; y++) {
					// fill matrix with values depending on center value and distance from center
					// to form a 2D Guassian bell curve
					double distance = Math.Sqrt((y - _radius) * (y - _radius) + (x - _radius) * (x - _radius));
					_total += _matrix[x, y] = (float)(Math.Exp(-distance * distance / centerFactor));
				}
			}
		}
	}
}
