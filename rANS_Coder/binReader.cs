using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rANS_Coder
{
    class BinReader
    {

        byte[] bArray;
        bool flagRNS = false; // флаг архива *.rns

        string Extension; // расширение



        public bool FlagRNS { get => flagRNS; set => flagRNS = value; }

        public byte[] GetExtension()
        { // получение расширения как массива байт

            Extension = Extension.Substring(1, Extension.Length - 1);
            byte[] res = Encoding.ASCII.GetBytes(Extension);

            return res;
        }


        public byte[] ReadFile(string FileName) // чтение файла
        {


            var fileInfo = new FileInfo(FileName); // чтение параметров файла
;
            Extension = fileInfo.Extension; // определение расширения файла

            try
            {
                using (BinaryReader br = new BinaryReader(File.Open(FileName, FileMode.Open)))
                {

                    bArray = new byte[fileInfo.Length]; // определение размера файла
                    bArray = br.ReadBytes((int)br.BaseStream.Length); // чтение в битовый массив всего файла

                    Console.WriteLine("> Extension " + fileInfo.Extension + " " + fileInfo.Extension.Length);

                    if (fileInfo.Extension == ".rns") // выставление флага архива
                    {
                        FlagRNS = true;
                    }

                }

            }
            catch (Exception e)
            {

                Console.WriteLine("ERROR READ " + e.Message);
            }
            return bArray;
        }
    }

}
