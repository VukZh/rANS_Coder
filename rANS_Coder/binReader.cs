using System;
using System.IO;
using System.Text;

namespace rANS_Coder
{
    class BinReader : IReader
    {

        bool flagRNS = false; // флаг архива *.rns
        string Extension; // расширение
        byte[] ExtensionToByte; //расширение как массив байт для записи в бинарный файл

        public bool FlagRNS { get => flagRNS; set => flagRNS = value; }
        public byte[] GetExtension { get => ExtensionToByte; }
        public byte[] ReadFile(string FileName) // чтение файла
        {

            try
            {

                var fileInfo = new FileInfo(FileName); // чтение параметров файла
                byte[] byteArray = new byte[fileInfo.Length]; // определение размера файла
                Extension = fileInfo.Extension; // определение расширения файла
                Extension = Extension.Substring(1, Extension.Length - 1); // убираем точку в расширении
                ExtensionToByte = Encoding.ASCII.GetBytes(Extension);

                using (BinaryReader br = new BinaryReader(File.Open(FileName, FileMode.Open)))
                {

                    byteArray = br.ReadBytes((int)br.BaseStream.Length); // чтение в битовый массив всего файла

                    if (fileInfo.Extension == ".rns") // выставление флага архива
                    {
                        FlagRNS = true;
                    }                    
                }
                return byteArray;
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("FILE NOT FOUND");
                Environment.Exit(0);
            }
            catch (IOException)
            {
                Console.WriteLine("ERROR READ FILE");
                Environment.Exit(0);
            }

            return null;
        }
    }

}
