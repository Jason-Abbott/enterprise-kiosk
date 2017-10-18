using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Idaho.Draw {
	/// <summary>
	/// Represent an ARGB image as an array of channel values
	/// </summary>
	/// <remarks>
	/// This allows speedier access to color information for convolutions.
	/// </remarks>
	internal class ColorMatrix {
		private byte[, ,] _matrix;
		private int _width;
		private int _height;
		private int _depth = 4;
		private const int _maxColor = 255;
		private const int _minColor = 0;

		internal int Width { get { return _width; } }
		internal int Height { get { return _height; } }
		/// <summary>
		/// Color channels
		/// </summary>
		internal int Depth { get { return _depth; } }
		internal byte[, ,] Color { get { return _matrix; } }

		/// <remarks>An enum would work but always requires conversion to int</remarks>
		internal class Channel {
			public const int Red = 0;
			public const int Green = 1;
			public const int Blue = 2;
			public const int Alpha = 3;
		}

		#region Constructors

		/// <summary>
		/// Create matrix of colors from image
		/// </summary>
		internal ColorMatrix(Bitmap image) {
			_width = image.Width;
			_height = image.Height;
			_matrix = new byte[_width, _height, _depth];
		
			for (int x = 0; x < _width; x++) {
				for (int y = 0; y < _height; y++) {
					this.SetPixel(x, y, image.GetPixel(x, y));
				}
			}
		}

		/// <summary>
		/// Create matrix entirely of one color
		/// </summary>
		private ColorMatrix(int width, int height, Color c) {
			_width = width;
			_height = height;
			_matrix = new byte[_width, _height, _depth];

			for (int x = 0; x < _width; x++) {
				for (int y = 0; y < _height; y++) { this.SetPixel(x, y, c); }
			}
		}

		#endregion

		#region Get and Set pixel color values

		/// <summary>
		/// Set color for given pixel position
		/// </summary>
		internal void SetPixel(int x, int y, Color c) {
			if (x > _width || y > _height || c == null) { return; }

			_matrix[x, y, Channel.Alpha] = (byte)c.A;
			_matrix[x, y, Channel.Red] = (byte)c.R;
			_matrix[x, y, Channel.Green] = (byte)c.G;
			_matrix[x, y, Channel.Blue] = (byte)c.B;
		}
		internal void SetPixel(int x, int y, float r, float g, float b) {
			Color c = System.Drawing.Color.FromArgb((int)r, (int)g, (int)b);
			this.SetPixel(x, y, c);
		}
		internal void SetPixel(int x, int y, float a, float r, float g, float b) {
			Color c = System.Drawing.Color.FromArgb((int)a, (int)r, (int)g, (int)b);
			this.SetPixel(x, y, c);
		}

		/// <summary>
		/// Get color for given pixel position
		/// </summary>
		internal Color GetPixel(int x, int y) {
			return System.Drawing.Color.FromArgb(
				(int)_matrix[x, y, Channel.Alpha],
				(int)_matrix[x, y, Channel.Red],
				(int)_matrix[x, y, Channel.Green],
				(int)_matrix[x, y, Channel.Blue]);
		}

		/// <summary>
		/// Indicate if color at given coordinates is transparent
		/// </summary>
		internal bool IsTransparent(int x, int y) {
			return (_matrix[x, y, Channel.Alpha] == 0.0f);
		}

		#endregion

		/// <summary>
		/// Return a blank matrix of the given size
		/// </summary>
		internal static ColorMatrix Blank(int width, int height) {
			return new ColorMatrix(width, height, System.Drawing.Color.Transparent);
		}

		/// <summary>
		/// Convert array to image object
		/// </summary>
		internal Image ToImage() {
			Bitmap result = this.ToBitmap();
			MemoryStream ms = new MemoryStream();
			result.Save(ms, ImageFormat.Png);
			return Image.FromStream(ms);
		}
		
		/// <summary>
		/// Convert array to bitmap object
		/// </summary>
		internal Bitmap ToBitmap() {
			Bitmap result = new Bitmap(_width, _height);
			for (int x = 0; x < _width; x++) {
				for (int y = 0; y < _height; y++) {
					result.SetPixel(x, y, this.GetPixel(x, y));
				}
			}
			return result;
		}

		#region Combinations

		/// <summary>
		/// Define different ways of combining color matrices
		/// </summary>
		/// <param name="factor">Can be used to amplify effects of combination</param>
		internal delegate float CombineDelegate(float c1, float c2, float factor);

		private float Add(float c1, float c2, float factor) { return c1 + (factor * c2); }

		internal void Add(ColorMatrix other) {
			this.CombineWith(other, new CombineDelegate(Add));
		}

		/// <summary>
		/// Combine values of this matrix with another according to delegate
		/// </summary>
		/// <param name="factor">Use to amplify the result of the combination</param>
		internal void CombineWith(ColorMatrix other, CombineDelegate method, float factor) {
			float color;

			for (int x = 0; x < _width; x++) {
				for (int y = 0; y < _height; y++) {
					for (int c = 0; c < _depth - 1; c++) {
						// don't invert alpha channel--that gets screwy
						color = method(_matrix[x, y, c], other.Color[x, y, c], factor);
						_matrix[x, y, c] = Idaho.Utility.Constrain(color, _minColor, _maxColor);
					}
				}
			}
		}
		internal void CombineWith(ColorMatrix other, CombineDelegate method) {
			this.CombineWith(other, method, 1);
		}

		#endregion

	}
}
