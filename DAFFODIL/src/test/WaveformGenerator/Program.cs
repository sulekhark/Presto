using System;
using System.Drawing;
using Un4seen.Bass;

namespace WaveformGenerator {
    public class Program {
        static void Main() {
            string filePath = "file_example_MP3_700KB.mp3";
            Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
            WaveformGenerator wg = SetupWaveformGenerator(filePath);
            try
            {
                wg.DetectWaveformLevelsAsync().Wait();
            }
            catch (Exception e)
            {
                throw e;
            }

            Bitmap wfImage = CreateBitmapImage(wg);
            string outFilePath = "bitmap_out.bmp";
            wg.SaveImage(wfImage, outFilePath);
        }

        static WaveformGenerator SetupWaveformGenerator(string inputPath)
        {
            WaveformGenerator wg = new WaveformGenerator(inputPath);
            try
            {
                // Change settings.
                wg.Direction = WaveformGenerator.WaveformDirection.LeftToRight;
                wg.Orientation = WaveformGenerator.WaveformSideOrientation.LeftSideOnTopOrLeft;
                wg.Detail = 1.5f;
                wg.LeftSideBrush = new SolidBrush(Color.Orange);
                wg.RightSideBrush = new SolidBrush(Color.Gray);
                wg.ProgressChanged += wg.wg_ProgressChanged;
                wg.Completed += wg.wg_Completed;
                wg.CreateStream();
            }
            catch (Exception e)
            {
                if (e is Exception)
                {
                    Console.WriteLine("BASS not correctly initialized.");
                }
                else
                {
                    throw e;
                }
            }
            return wg;
        }

        static Bitmap CreateBitmapImage(WaveformGenerator wg)
        {
            Bitmap wfImage = null;
            try
            {
                wfImage = wg.CreateWaveform(700, 250);
                Console.WriteLine("Bitmap creation completed");
            }
            catch (Exception e)
            {
                if (e is ArgumentException)
                {
                    Console.WriteLine("Invalid arguments to CreateWaveform.");
                }
                else
                {
                    throw e;
                }
            }
            return wfImage;
        }
    }
}
