using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;

namespace WindowsSocketForms
{
    static class ImageGenerator
    {
        public static Bitmap Generate(bool[][] data)
        {
            var img = new Bitmap(data.Length, data[0].Length, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            for (int y = 0; y < data.Length; y++)
            {
                for (int x = 0; x < data[y].Length; ++x)
                {
                    if (data[y][x])
                    {
                        img.SetPixel(x, y, Color.Black);
                    }
                    else
                    {
                        img.SetPixel(x, y, Color.White);
                    }
                }
            }
            return img;
        }

        public static Bitmap CloseResize(Bitmap original, Size destinationSize)
        {
            var rect = new Rectangle(0, 0, destinationSize.Width, destinationSize.Height);
            var result = new Bitmap(destinationSize.Width, destinationSize.Height);

            result.SetResolution(original.HorizontalResolution, original.VerticalResolution);

            using (var graphics = Graphics.FromImage(result))
            {
                graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                using (var wrap = new ImageAttributes())
                {
                    wrap.SetWrapMode(System.Drawing.Drawing2D.WrapMode.Clamp);
                    graphics.DrawImage(original, rect, 0, 0, original.Width, original.Height, GraphicsUnit.Pixel, wrap);
                }
            }

            return result;
        }
    }
}
