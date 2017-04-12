using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BassPlayer
{
    class SimpleFileAbstraction : TagLib.File.IFileAbstraction
    {
        private string fileName;

        public SimpleFileAbstraction(string outputFile)
        {
            this.fileName = outputFile;
        }

        public String Name
        {
            get { return fileName; }
        }

        public System.IO.Stream ReadStream
        {
            get { return new FileStream(Name, System.IO.FileMode.Open); }
        }

        public System.IO.Stream WriteStream
        {
            get { return new FileStream(Name, System.IO.FileMode.Open); }
        }

        public void CloseStream (System.IO.Stream stream)
        {
            stream.Close();
        }
    }
}
