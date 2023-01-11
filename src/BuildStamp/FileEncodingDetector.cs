using System;
using System.IO;

namespace BuildStamp
{
    public class FileEncodingDetector
    {
        public enum BomType { UTF8, UTF8WithoutBom, UTF16LE, UTF16BE, Unknown }

        public BomType Type;
        public System.Text.Encoding Encoding;

        public FileEncodingDetector(string filename)
        {
            byte[] bomBytes = new byte[3];
            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                var bytes_read = fs.Read(bomBytes, 0, bomBytes.Length);
                fs.Close();

                Array.Resize(ref bomBytes, bytes_read);
            }

            if (bomBytes.Length == 3 && bomBytes[0] == 0xEF && bomBytes[1] == 0xBB && bomBytes[2] == 0xBF)
            {
                Type = BomType.UTF8;
                Encoding = System.Text.Encoding.UTF8;
                return;
            }

            if (bomBytes.Length == 2 && bomBytes[0] == 0xFF && bomBytes[1] == 0xFE)
            {
                Type = BomType.UTF16LE;
                Encoding = System.Text.Encoding.Unicode;
                return;
            }

            if (bomBytes.Length == 2 && bomBytes[0] == 0xFE && bomBytes[1] == 0xFF)
            {
                Type = BomType.UTF16BE;
                Encoding = System.Text.Encoding.BigEndianUnicode;
                return;
            }

            /* If a Byte Order Mark (BOM) is not present, assume UTF-8 (is also ASCII) encoding. 
             * This will be mostly valid for a programming source file. 
             */
            Type = BomType.UTF8WithoutBom;
            Encoding = new System.Text.UTF8Encoding(false);
        }
    }
}
