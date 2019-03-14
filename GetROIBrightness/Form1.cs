using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GetROIBrightness
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Bitmap orig = new Bitmap(@"c:\temp\24bpp.bmp");
            Bitmap bmp = new Bitmap(@"F:\temp.jpeg");
            //Bitmap croppedBmp = CropBitmap(bmp);
            DateTime start = DateTime.Now;

            byte[] pixels = GetRGBValues(bmp);
            //byte[] pixels = GetPixel(bmp);

            Console.WriteLine("GetPixelTime = {0}ms", DateTime.Now.Subtract(start).TotalMilliseconds);

            float result = GetROIAvgBrightness(pixels);

            Console.WriteLine("Avg Britness of ROI = {0}, ElapsedTime = {1}", result, DateTime.Now.Subtract(start).TotalMilliseconds);
        }

        private int GetROIAvgBrightness(byte[] pixels)
        {
            int cnt = 0;
            float sumValue = 0;
            int result = 0;

            for (int i = 1023; i < pixels.Length - 2; i++)
            {
                //if (i != 0 && i % 1024 == 0)
                if (i%1023==0)
                {
                    sumValue += GetBrightness(pixels[i], pixels[i + 1], pixels[i + 2]);
                    cnt++;
                }
            }

            result = (int)Math.Round((sumValue / cnt) * 255);

            return result;
        }

        private float GetBrightness(float R, float G, float B)
        {
            float r = (float)R / 255.0f;
            float g = (float)G / 255.0f;
            float b = (float)B / 255.0f;

            float max, min;

            max = r; min = r;

            if (g > max) max = g;
            if (b > max) max = b;

            if (g < min) min = g;
            if (b < min) min = b;

            return (max + min) / 2;
        }

        private byte[] GetPixel(Bitmap processedBitmap)
        {
            unsafe
            {

                BitmapData bitmapData = processedBitmap.LockBits(new Rectangle(0, 0, processedBitmap.Width, processedBitmap.Height), ImageLockMode.ReadWrite, processedBitmap.PixelFormat);
                int bytesPerPixel = System.Drawing.Bitmap.GetPixelFormatSize(processedBitmap.PixelFormat) / 8;
                int heightInPixels = bitmapData.Height;
                int widthInBytes = bitmapData.Width * bytesPerPixel;

                byte* ptrFirstPixel = (byte*)bitmapData.Scan0;

                int width = processedBitmap.Width;
                int height = processedBitmap.Height;

                byte[] pixel = new byte[width * height * 3];
                int rgbIndex = 0;
                for (int y = 0; y < heightInPixels; y++)
                {
                    byte* currentLine = ptrFirstPixel + (y * bitmapData.Stride);

                    for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                    {
                        pixel[rgbIndex] = currentLine[x];
                        pixel[rgbIndex + 1] = currentLine[x + 1];
                        pixel[rgbIndex + 2] = currentLine[x + 2];

                        rgbIndex += 3;
                    }
                }

                processedBitmap.UnlockBits(bitmapData);
                processedBitmap.Dispose();

                return pixel;
            }
        }

        private byte[] GetRGBValues(Bitmap bmp)
        {
            // Lock the bitmap's bits. 
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            System.Drawing.Imaging.BitmapData bmpData =
             bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly,
             bmp.PixelFormat);

            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            int bytes = bmpData.Stride * bmp.Height;
            byte[] rgbValues = new byte[bytes];

            // Copy the RGB values into the array.
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

            bmp.UnlockBits(bmpData);

            return rgbValues;
        }

        private Bitmap CropBitmap(Bitmap original)
        {
            if (original == null) return null;

            try
            {
                Bitmap copy = new Bitmap(306, 256);

                using (Graphics g = Graphics.FromImage(copy))
                {
                    Rectangle source_rect = new Rectangle(0, 0, 306, 256);
                    Rectangle dest_rect = new Rectangle(0, 0, 306, 256);

                    g.CompositingQuality = CompositingQuality.HighSpeed;
                    g.DrawImage(original, dest_rect, source_rect, GraphicsUnit.Pixel);
                }

                return copy;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
