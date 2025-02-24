using System;
using System.IO;

namespace BuildStamp
{
    public class FileEncodingDetector
    {
        public enum BomType { UTF8, UTF8WithoutBom, UTF16LE, UTF16BE, Unknown }

        public BomType Type;
        public System.Text.Encoding Encoding;

        private void setUTF8_WITH_BOM()
        {
            Type = BomType.UTF8;
            Encoding = System.Text.Encoding.UTF8;
        }

        private void setUTF8_WITHOUT_BOM()
        {
            Type = BomType.UTF8WithoutBom;
            Encoding = new System.Text.UTF8Encoding(false);
        }

        private void setUTF16LittleEndian_WITH_BOM()
        {
            Type = BomType.UTF16LE;
            Encoding = System.Text.Encoding.Unicode;
        }

        private void setUTF16BigEndian_WITH_BOM()
        {
            Type = BomType.UTF16BE;
            Encoding = System.Text.Encoding.BigEndianUnicode;
        }

        public FileEncodingDetector(BomType type)
        {
            switch (type)
            {
                case BomType.UTF8:
                    setUTF8_WITH_BOM();
                    break;

                case BomType.UTF8WithoutBom:
                    setUTF8_WITHOUT_BOM();
                    break;

                case BomType.UTF16LE:
                    setUTF16LittleEndian_WITH_BOM();
                    break;

                case BomType.UTF16BE:
                    setUTF16BigEndian_WITH_BOM();
                    break;

                default:
                    throw new Exception("Unknown type: " + type.ToString());
            }
        }

        public FileEncodingDetector(string filename)
        {
            byte[] bomBytes = new byte[3];
            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                var bytes_read = fs.Read(bomBytes, 0, bomBytes.Length);
                fs.Close();

                Array.Resize(ref bomBytes, bytes_read);
            }

            if (bomBytes.Length >= 3 && bomBytes[0] == 0xEF && bomBytes[1] == 0xBB && bomBytes[2] == 0xBF)
            {
                setUTF8_WITH_BOM();
                return;
            }

            if (bomBytes.Length >= 2 && bomBytes[0] == 0xFF && bomBytes[1] == 0xFE)
            {
                setUTF16LittleEndian_WITH_BOM();
                return;
            }

            if (bomBytes.Length >= 2 && bomBytes[0] == 0xFE && bomBytes[1] == 0xFF)
            {
                setUTF16BigEndian_WITH_BOM();
                return;
            }

            /* If a Byte Order Mark (BOM) is not present, assume UTF-8 (is also ASCII) encoding. 
             * This will be mostly valid for a programming source file. 
             */
            setUTF8_WITHOUT_BOM();
        }
    }
}
