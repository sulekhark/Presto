using System.Collections.Generic;

namespace FilePkgUtil
{
    public class FilePackage
    {
        public string FilePath { get; set; }
        public IEnumerable<string> ContentFilePathList { get; set; }
    }
}

