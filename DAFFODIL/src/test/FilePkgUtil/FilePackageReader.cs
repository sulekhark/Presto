using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

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
        _filenameFileContentDictionary = new Dictionary<string, string>();

        var fileInfo = new FileInfo(_filepath);
        if (!fileInfo.Exists) FilePackageHelper.HandleInputError("The package to read does not exist!");
        try
        {
            // Open the package file
            using (var fs = new FileStream(_filepath, FileMode.Open))
            {
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
            }
        }
        catch (Exception e)
        {
            if (FilePackageHelper.IsFatal(e))
            {
                throw e;
            }
        }
        return _filenameFileContentDictionary;
    }
}
