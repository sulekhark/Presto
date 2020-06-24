using System;
using System.Collections.Generic;
using System.IO;

namespace FilePkgUtil
{
    class FilePkgUtil
    {
        static void Main(string[] args)
        {
            string packageFilePath = ".\\PackageTest\\test.pkg";
            string f1 = ".\\PackageSource\\file1.txt";
            string f2 = ".\\PackageSource\\file2.txt";

            var filePackage = new FilePackage();
            filePackage.FilePath = packageFilePath;
            List<string> flst = new List<string>();
            filePackage.ContentFilePathList = flst;
            flst.Add(f1);
            flst.Add(f2);
            
            var filePackageWriter = new FilePackageWriter(filePackage);
            filePackageWriter.GeneratePackage(false);

            var filePackageReader = new FilePackageReader(packageFilePath);
            var filenameFileContentDictionary = filePackageReader.GetFilenameFileContentDictionary();

            foreach (var keyValuePair in filenameFileContentDictionary)
            {
                Console.WriteLine("Filename: " + keyValuePair.Key);
                Console.WriteLine("Content: " + keyValuePair.Value);
            }
        }
    }
}
