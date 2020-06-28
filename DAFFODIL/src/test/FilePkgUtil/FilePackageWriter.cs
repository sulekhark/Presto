
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace FilePkgUtil
{
    public class FilePackageWriter
    {
        private readonly string _filepath;
        private readonly IEnumerable<string> _contentFilePathList;
        private string _tempDirectoryPath;

        public static bool simulateError1;

        public FilePackageWriter(FilePackage filePackage)
        {
            _filepath = filePackage.FilePath;
            _contentFilePathList = filePackage.ContentFilePathList;
        }

        public void GeneratePackage(bool deleteContents)
        {
            try
            {
                GeneratePackageInt(deleteContents);
            }
            catch (NullReferenceException e)
            {
                var errorMessage = "An error occured while generating the package.";
                throw new Exception(errorMessage, e);
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
                    System.Console.WriteLine("GeneratePackage: Non-fatal exception raised:" + msg);
                }
            }
        }

        public void GeneratePackageInt(bool deleteContents)
        {
            string parentDirectoryPath = null;
            string filename = null;

            var fileInfo = new FileInfo(_filepath);
            int errId = -1;
            string msg = "";
            // Get the parent directory path of the package file and if the package file already exists delete it
            if (fileInfo.Exists)
            {
                filename = fileInfo.Name;

                var parentDirectoryInfo = fileInfo.Directory;
                if (parentDirectoryInfo != null)
                {
                    parentDirectoryPath = parentDirectoryInfo.FullName;
                    if (simulateError1) errId = 1;
                }
                else
                {
                    errId = 1;
                    msg = "Parent directory info was null!";
                }
                File.Delete(_filepath);
            }
            else
            {
                var lastIndexOfFileSeperator = _filepath.LastIndexOf("\\", StringComparison.Ordinal);
                if (lastIndexOfFileSeperator != -1)
                {
                    parentDirectoryPath = _filepath.Substring(0, lastIndexOfFileSeperator);
                    filename = _filepath.Substring(lastIndexOfFileSeperator + 1, _filepath.Length - (lastIndexOfFileSeperator + 1));
                    if (simulateError1) errId = 2;
                }
                else
                {
                    errId = 2;
                    msg = "The input file path does not contain any file seperators.";
                }
            }
            // Create a temp directory for our package
            _tempDirectoryPath = CreateTempDir(parentDirectoryPath);
            
            foreach (var filePath in _contentFilePathList)
            {
                // Copy every content file into the temp directory we created before
                var filePathInfo = new FileInfo(filePath);
                if (filePathInfo.Exists)
                {
                    File.Copy(filePathInfo.FullName, _tempDirectoryPath + "\\" + filePathInfo.Name);
                }
            }
            FilePackageHelper.HandleError(errId, msg);
            // Generate the ZIP from the temp directory
            ZipFile.CreateFromDirectory(_tempDirectoryPath, _filepath);

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

        public string CreateTempDir(string parentPath)
        {
            string tempDirName = parentPath + "\\" + "f_temp";
            if (Directory.Exists(tempDirName))
            {
                Directory.Delete(tempDirName, true);
            }

            Directory.CreateDirectory(tempDirName);
            return tempDirName;
        }
    }
}
