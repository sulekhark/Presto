
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

public class FilePackageWriter
{
    private readonly string _filepath;
    private readonly IEnumerable<string> _contentFilePathList;
    private string _tempDirectoryPath;

    public FilePackageWriter(FilePackage filePackage)
    {
        _filepath = filePackage.FilePath;
        _contentFilePathList = filePackage.ContentFilePathList;
    }

    public void GeneratePackage(bool deleteContents)
    {
        try
        {
            string parentDirectoryPath = null;
            string filename = null;

            var fileInfo = new FileInfo(_filepath);

            // Get the parent directory path of the package file and if the package file already exists delete it
            if (fileInfo.Exists)
            {
                filename = fileInfo.Name;

                var parentDirectoryInfo = fileInfo.Directory;
                if (parentDirectoryInfo != null)
                {
                    parentDirectoryPath = parentDirectoryInfo.FullName;
                }
                else
                {
                    throw new NullReferenceException("Parent directory info was null!");
                }

                File.Delete(_filepath);
            }
            else
            {
                var lastIndexOfFileSeperator = _filepath.LastIndexOf("\\", StringComparison.Ordinal);
                if (lastIndexOfFileSeperator != -1)
                {
                    parentDirectoryPath = _filepath.Substring(0, lastIndexOfFileSeperator);
                    filename = _filepath.Substring(lastIndexOfFileSeperator + 1,_filepath.Length - (lastIndexOfFileSeperator + 1));
                }
                else
                {
                    throw new Exception("The input file path '" + _filepath +
                                        "' does not contain any file seperators.");
                }
            }

            // Create a temp directory for our package
            _tempDirectoryPath = parentDirectoryPath + "\\" + filename + "_temp";
            if (Directory.Exists(_tempDirectoryPath))
            {
                Directory.Delete(_tempDirectoryPath, true);
            }

            Directory.CreateDirectory(_tempDirectoryPath);
            foreach (var filePath in _contentFilePathList)
            {
                // Copy every content file into the temp directory we created before
                var filePathInfo = new FileInfo(filePath);
                if (filePathInfo.Exists)
                {
                    File.Copy(filePathInfo.FullName, _tempDirectoryPath + "\\" + filePathInfo.Name);
                }
                else
                {
                    throw new FileNotFoundException("File path " + filePath + " doesn't exist!");
                }
            }
            // Generate the ZIP from the temp directory
            ZipFile.CreateFromDirectory(_tempDirectoryPath, _filepath);
        }
        catch (Exception e)
        {
            var errorMessage = "An error occured while generating the package.";
            throw new Exception(errorMessage, e);
        }
        finally
        {
            // Clear the temp directory and the content files
            if (Directory.Exists(_tempDirectoryPath))
            {
                Directory.Delete(_tempDirectoryPath, true);
            }

            if (deleteContents)
            {
                foreach (var filePath in _contentFilePathList)
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }
            }
        }
    }
}
