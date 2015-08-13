using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace LuxBotSharp
{
    //This is currently not used.
    //This will be used in future versions to speed things up :P
    public static class ImageConverter
    {
        public static void ConvertOne(string filename)
        {
            var path = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "data\\" + filename));
            Bitmap bitmap;
            using (Stream stream = File.Open(path, FileMode.Open))
            {
                bitmap = (Bitmap)Image.FromStream(stream);
                stream.Close();
            }
            var result = Cv.CreateImage(Cv.Size(bitmap.Width, bitmap.Height), BitDepth.U8, 1);
            bitmap.ToIplImage().CvtColor(result, ColorConversion.BgrToGray);
            var bytes = result.ToBytes(Path.GetExtension(path));
            File.WriteAllBytes(path, bytes);
        }

        public static void ConvertAll()
        {
            string [] files = Directory.GetFiles(Path.Combine(Environment.CurrentDirectory, "data"));
            Utils.Print("FOUND " + files.Count() + " FILES TO CONVERT.", ConsoleColor.Green);
            foreach (string file in files)
            {
                var fileName = Path.GetFileName(file);
                ConvertOne(fileName);
                Utils.Print("CONVERTED " + fileName, ConsoleColor.Green);
            }
        }

        public static void Try(string[] args)
        {
            Utils.Print("STARTING CONVERSION PROCESS BGR > GRAY", ConsoleColor.Cyan);
            ConvertAll();
        }
    }
}
