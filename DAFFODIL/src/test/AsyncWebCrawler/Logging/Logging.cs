using System.IO;

namespace AsyncWebCrawler.Logging
{
    /// <summary>
    /// Logging class for Log functionality
    /// </summary>
    public class Logging
    {
        public static string LogFile;
        /// <summary>
        /// Method to write reports to local machine.
        /// </summary>
        /// <param name="contents">string contents</param>
        public static void WriteReportToDisk(string contents)
        {
            FileStream fStream;
            if (File.Exists(LogFile))
            {
                File.Delete(LogFile);
                fStream = File.Create(LogFile);
            }
            else
            {
                fStream = File.OpenWrite(LogFile);
            }

            using (TextWriter writer = new StreamWriter(fStream))
            {
                writer.WriteLine(contents);
                writer.Flush();
            }
            fStream.Dispose();
        }
    }
}
