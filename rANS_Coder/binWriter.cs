using System;
using System.IO;

namespace rANS_Coder
{
    class BinWriter
    {

        string NewFileName; // новое имя на основе параметра FileName


        public void WriteFile(string FileName, byte b_out) // запись нового закодированного файла FileName.rns с 1 байтом (размер расширения исходного файла)
        {

            NewFileName = Path.ChangeExtension(FileName, "rns"); // замена расширения

            try
            {
                using (BinaryWriter bw = new BinaryWriter(File.Open(NewFileName, FileMode.Create)))
                {
                    //for (int i = 0; i < b_out.Length; i++)
                    //{
                    bw.Write(b_out);
                    //}
                }

            }
            catch (Exception e)
            {

                Console.WriteLine("ERROR WRITE " + e.Message);
            }

        }

        public void WriteFile(string FileName, string Extension, byte [] b_out) { // запись нового раскодированного файла
            NewFileName = Path.ChangeExtension(FileName, Extension); // замена расширения на исходное, до кодировки

            try
            {
                using (BinaryWriter bw = new BinaryWriter(File.Open(NewFileName, FileMode.Create)))
                {
                    //for (int i = 0; i < b_out.Length; i++)
                    //{
                    bw.Write(b_out);
                    //}
                }

            }
            catch (Exception e)
            {

                Console.WriteLine("ERROR WRITE " + e.Message);
            }
        }

        public void AppToFile(byte[] b_out) // дозапись закодированного файла
        { // дозапись массива байт

            try
            {
                using (BinaryWriter bw = new BinaryWriter(File.Open(NewFileName, FileMode.Append)))
                {
                    for (int i = 0; i < b_out.Length; i++)
                    {
                        bw.Write(b_out[i]);
                    }
                }
            }
            catch (Exception e)
            {

                Console.WriteLine("ERROR APPEND " + e.Message);
            }

        }

        public void AppToFile(byte b_out) // дозапись байта в закодированный файл
        {

            try
            {
                using (BinaryWriter bw = new BinaryWriter(File.Open(NewFileName, FileMode.Append)))
                {

                    bw.Write(b_out);
                }
            }
            catch (Exception e)
            {

                Console.WriteLine("ERROR APPEND " + e.Message);
            }

        }

        public void AppToFile(UInt32[] UInt32_out) // дозапись массива чисел в закодированный файл
        {

            try
            {
                using (BinaryWriter bw = new BinaryWriter(File.Open(NewFileName, FileMode.Append)))
                {
                    for (int i = 0; i < UInt32_out.Length; i++)
                    {
                        bw.Write(UInt32_out[i]);
                    }
                }
            }
            catch (Exception e)
            {

                Console.WriteLine("ERROR APPEND " + e.Message);
            }

        }

    }
}
