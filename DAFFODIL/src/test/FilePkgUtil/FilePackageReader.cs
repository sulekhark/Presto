using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace FilePkgUtil
{
    public class FilePackageReader
    {
        private Dictionary<string, string> _filenameFileContentDictionary;
        private readonly string _filepath;

        public FilePackageReader(string filepath)
        {
            _filepath = filepath;
        }

        public Dictionary<string, string> GetFilenameFileContentDictionary()
        {
            var fileInfo = new FileInfo(_filepath);
            FilePackageHelper.CheckInput(fileInfo, "The package to read does not exist!");
            Dictionary<string, string> fContentDict = null;
            try
            {
                // Open the package file
                using (var fs = new FileStream(_filepath, FileMode.Open))
                {
                    fContentDict = GetFilenameFileContentDictionaryInt(fs);
                }
            }
            catch (Exception e)
            {
                if (FilePackageHelper.IsFatal(e))
                {
                    throw e;
                }
                else
                {
                    string msg = e.Message;
                    System.Console.WriteLine("GetFilenameFileContentDictionary: Non-fatal exception raised:" + msg);
                }
            }
            return fContentDict;
        }

        public Dictionary<string, string> GetFilenameFileContentDictionaryInt(FileStream fs)
        {
            _filenameFileContentDictionary = new Dictionary<string, string>();

            // Open the package file as a ZIP
            using (var archive = new ZipArchive(fs))
            {
                // Iterate through the content files and add them to a dictionary
                foreach (var zipArchiveEntry in archive.Entries)
                {
                    using (var stream = zipArchiveEntry.Open())
                    {
                        using (var zipSr = new StreamReader(stream))
                        {
                            _filenameFileContentDictionary.Add(zipArchiveEntry.Name, zipSr.ReadToEnd());
                        }
                    }
                }
            }
            return _filenameFileContentDictionary;
        }
    }
}
