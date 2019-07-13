using System;
using System.IO;
using System.Net;
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;

namespace CIFAR_Extractor
{
    class Program
    {
       static string[] classNames = new string[]{"airplane","automobile","bird","cat","deer",
               "dog","frog","horse","ship","truck" };
        static void Main(string[] args)
        {
            var urlFile = "http://www.cs.toronto.edu/~kriz/cifar-10-binary.tar.gz";
            var destPath = "..\\..\\..\\..\\data\\cifar-10-binary.tar.gz";

            downloadExtractFile(urlFile, destPath);


            Console.WriteLine("Press Any Key To Continue...");
            Console.ReadKey();
        }

        private static void downloadExtractFile(string fileUrl, string destPath)
        {
            
            FileInfo fi1 = new FileInfo(destPath);
            var destFolder = Path.GetDirectoryName(fi1.FullName);
            if (!fi1.Exists)
            {
                using (var client = new WebClient())
                {
                    client.DownloadFile(fileUrl, destPath);
                }

            }

            using (Stream inStream = File.OpenRead(fi1.FullName))
            {
                //Stream inStream = File.OpenRead(gzArchiveName);
                Stream gzipStream = new GZipInputStream(inStream);

                TarArchive tarArchive = TarArchive.CreateInputTarArchive(gzipStream);
                tarArchive.ExtractContents(destFolder);
                tarArchive.Close();

                gzipStream.Close();
                inStream.Close();

            }

            //first bytes are reserved for label, and the rest of 3072 bytes are image
            var destImgFolder = $"{destFolder}\\{"cifar-10-batches-bin\\"}";
            //
            int imageCOunter = 1;

            //read batches and save images
            var batch1 = File.ReadAllBytes($"{destFolder}\\{"cifar-10-batches-bin\\data_batch_1.bin"}");
            extractandSave(batch1, destImgFolder, ref imageCOunter);

            var batch2 = File.ReadAllBytes($"{destFolder}\\{"cifar-10-batches-bin\\data_batch_2.bin"}");
            extractandSave(batch1, destImgFolder, ref imageCOunter);

            var batch3 = File.ReadAllBytes($"{destFolder}\\{"cifar-10-batches-bin\\data_batch_3.bin"}");
            extractandSave(batch1, destImgFolder, ref imageCOunter);

            var batch4 = File.ReadAllBytes($"{destFolder}\\{"cifar-10-batches-bin\\data_batch_4.bin"}");
            extractandSave(batch1, destImgFolder, ref imageCOunter);


            return;
        }

        public static void extractandSave(byte[] batch, string destImgFolder, ref int imgCounter)
        {

            var nStep = 3073;//1 for label and 3072 for image

            for (int i = 0; i < batch.Length; i += nStep)
            {
                var l = (int)batch[i];
                var img = batch.Skip(i + 1).Take(nStep - 1).Select(x=>(float)x).ToList();
                // data in CIFAR-10 dataset is in CHW format.
                //Mat img = new Mat(200, 400, DepthType.Cv8U, 3);
                var image = ArrayToImg(32,32,img);

                //prepare for saving
                //check if folder exist
                var currentFolder = destImgFolder + classNames[l];
                if (!Directory.Exists(currentFolder))
                    Directory.CreateDirectory(currentFolder);

                //save image to specified folder
                image.Save(currentFolder + "\\" + imgCounter.ToString() + ".png");

                imgCounter++;
            }

        }
        public static Image byteArrayToImage(byte[] byteArray)
        {
            using (var ms = new MemoryStream(byteArray, 0, byteArray.Length))
            {
                ms.Seek(0, SeekOrigin.Begin);
                Image returnImage = System.Drawing.Image.FromStream(ms);
                return returnImage;
            }
               
        }

        internal static Bitmap ArrayToImg(int width, int height, List<float> img)
        {

            Bitmap bmp = new Bitmap(width, height);
            int index = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int rgb = (int)img[index];
                    var col = Color.FromArgb(rgb);
                    bmp.SetPixel(x, y, col);
                    index++;
                }
            }

            return bmp;
        }
    }
}
