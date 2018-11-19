namespace T.Extensions.ImageSharp
{
    using System;
    using System.Diagnostics;

    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.Primitives;

    public abstract class AbstractFloodFiller
    {
        internal Stopwatch _watch = new Stopwatch();

        protected int _bitmapWidth;
		protected int _bitmapHeight;
    	protected bool[,] _pixelsChecked;
		protected byte[] _byteFillColor;
		protected Rgba32 _startColor;

		protected AbstractFloodFiller(AbstractFloodFiller configSource)
		{
            FillColor = Rgba32.Magenta;
            Tolerance = new byte[] { 25, 25, 25 };

			if (configSource != null)
			{
				Bitmap = configSource.Bitmap;
				FillColor = configSource.FillColor;
				FillDiagonally = configSource.FillDiagonally;
				Tolerance = configSource.Tolerance;
			}
		}

		public Rgba32 FillColor { get; set; }

		public bool FillDiagonally { get; set; }

		public byte[] Tolerance { get; set; }

		public Image<Rgba32> Bitmap { get; set; }

		public abstract void FloodFill(Point pt);

		protected void PrepareForFloodFill(Point pt)
		{
			//cache data in member variables to decrease overhead of property calls
			//this is especially important with Width and Height, as they call
			//GdipGetImageWidth() and GdipGetImageHeight() respectively in gdiplus.dll - 
			//which means major overhead.
			_byteFillColor = new byte[] { FillColor.B, FillColor.G, FillColor.R };
			_bitmapWidth = Bitmap.Width;
			_bitmapHeight = Bitmap.Height;
			_pixelsChecked = new bool[_bitmapWidth, _bitmapHeight];
		}
	}
}
