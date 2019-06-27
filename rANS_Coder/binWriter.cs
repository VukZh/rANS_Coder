using System;
using System.IO;

namespace rANS_Coder
{
    class BinWriter : IWriter
    {
        string NewFileName; // новое имя на основе параметра FileName
        public void WriteFile(string FileName, byte byte_out) // запись нового закодированного файла FileName.rns с 1 байтом (размер расширения исходного файла)
        {

            NewFileName = Path.ChangeExtension(FileName, "rns"); // замена расширения
            try
            {
                using (BinaryWriter bw = new BinaryWriter(File.Open(NewFileName, FileMode.Create)))
                {
                    bw.Write(byte_out);
                }

            }
            catch (IOException)
            {
                Console.WriteLine("ERROR WRITE RNS FILE");
                Environment.Exit(0);
            }

        }

        public void WriteFile(string FileName, string Extension, byte[] byte_out)
        { // запись нового раскодированного файла
            NewFileName = Path.ChangeExtension(FileName, Extension); // замена расширения на исходное, до кодировки

            try
            {
                using (BinaryWriter bw = new BinaryWriter(File.Open(NewFileName, FileMode.Create)))
                {
                    bw.Write(byte_out);
                }

            }
            catch (IOException)
            {
                Console.WriteLine("ERROR WRITE FILE");
                Environment.Exit(0);
            }
        }

        public void ContinueWriteToFile(byte[] byte_out) // дозапись закодированного файла
        { // дозапись массива байт

            try
            {
                using (BinaryWriter bw = new BinaryWriter(File.Open(NewFileName, FileMode.Append)))
                {
                    for (int i = 0; i < byte_out.Length; i++)
                    {
                        bw.Write(byte_out[i]);
                    }
                }
            }
            catch (IOException)
            {
                Console.WriteLine("ERROR WRITE ARRAY BYTE TO FILE");
                Environment.Exit(0);
            }

        }

        public void ContinueWriteToFile(byte byte_out) // дозапись байта в закодированный файл
        {

            try
            {
                using (BinaryWriter bw = new BinaryWriter(File.Open(NewFileName, FileMode.Append)))
                {
                    bw.Write(byte_out);
                }
            }
            catch (IOException)
            {
                Console.WriteLine("ERROR WRITE BYTE TO FILE");
                Environment.Exit(0);
            }

        }

        public void ContinueWriteToFile(UInt16[] UInt16_out) // дозапись массива чисел (мл. разрядов состояния кодировщика) в закодированный файл
        {

            try
            {
                using (BinaryWriter bw = new BinaryWriter(File.Open(NewFileName, FileMode.Append)))
                {
                    for (int i = 0; i < UInt16_out.Length; i++)
                    {
                        //bw.Write(UInt32_out[i]);
                        bw.Write(UInt16_out[i]);
                        //Console.WriteLine(i + " ---- " + (UInt16)UInt16_out[i]);
                    }
                }
            }
            catch (IOException)
            {
                Console.WriteLine("ERROR WRITE ARCHIVE TO FILE");
                Environment.Exit(0);
            }

        }
    }
}
