using System;
using System.Collections.Generic;
using System.IO;

namespace FilePkgUtil
{
    class FilePkgUtil
    {
        static bool flag = false;
        static void Main(string[] args)
        {
            M1();
            M2(null);
            Console.WriteLine("Done");

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

        static void M1()
        {
            try
            {
                M4();
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
            }
            finally
            {
                M3();
            }
        }

        static void M4()
        {
            CFoo objFoo = new CFoo();
            M2(objFoo);
        }

        static void M2(CFoo paramFoo)
        {
            if (paramFoo != null)
            {
                M5();
                M6();
            }
        }

        static void M3() { if (flag) throw new ArgumentException(); }
        static void M5() { if (flag) throw new NullReferenceException(); }
        static void M6() { if (flag) throw new FieldAccessException(); }
    }

    class CFoo { }
}
