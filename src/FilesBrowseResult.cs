using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KidoZen
{
    public class FilesBrowseResult
    {
        public class File
        {
            public string name;
            public DateTime lastModified;
            public long size;
        }

        public string path;
        public File[] files = new File[0];
        public string[] subfolders = new string[0];
    }
}
