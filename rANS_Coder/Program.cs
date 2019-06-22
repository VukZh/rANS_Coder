using System;
using System.Text;

namespace rANS_Coder
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            string fileName; // имя файла
            byte pow;  // степень размера для таблицы кодирования


            if (args.Length == 0) // нет параметров командной строки
            {
                throw new ArgumentNullException("argument out");
            }



            else if (args.Length == 1) // 1 параметр командной строки
            {
                fileName = args[0];
                pow = 12; // default pow
            }
            else
            { // 2 параметра командной строки


                if (byte.TryParse(args[0], out pow)) // 1 параметр - pow; 2 параметр - имя файла
                {
                    fileName = args[1];
                }
                else
                {
                    byte.TryParse(args[1], out pow); // 2 параметр - pow; 1 параметр - имя файла
                    fileName = args[0];
                }
            }

            if (pow < 8 || pow > 30) throw new ArgumentException("pow should be > 8 and pow < 30"); // проверка выхода pow за пределы


            BinReader breader = new BinReader(); // читаем файл


            byte[] bcache = breader.ReadFile(fileName); // весь файл сохраняем в массиве байт
                                                        //Console.WriteLine("--- " + breader.FlagRNS);


            if (!breader.FlagRNS)
            { // кодирование
                BinWriter bw = new BinWriter(); // подготовка записи закодированного файла
                byte[] ext = breader.GetExtension(); // берем расширение для сохранения внутри файла
                                                     //Console.WriteLine(ext.Length);
                Array.Reverse(bcache); // для кодирование разворачиваем исходный массив байт
                bw.WriteFile(fileName, (byte)ext.Length); // 1 - записываем размер расширения
                bw.AppToFile(ext); // 2 - записываем расширение
                bw.AppToFile(pow); // 3 - записываем степень

                Encoder enc = new Encoder(bcache, pow, 77777); // подготовка кодирования массива bcache со степенью pow с начальной инициализацией кодировщика (77777) - также проверяется на это число в раскодировке

                byte[] TableFtoS = enc.CalcTabFtoS(); // рассчет таблицы для вычисления байт при дальнейшей раскодировке
                                                      //Console.WriteLine("TableFtoS " + TableFtoS.Length);



                bw.AppToFile(TableFtoS); // 4 - записываем рассчитанную на предыдущем шаге таблицу



                UInt16[] xArray = enc.GetX(); // получаем массив чисел - состояние кодировщика


                bw.AppToFile(xArray); // 5 - записываем состояние кодировщика



            }
            else // декодирование
            {

                byte[] ext = new byte[bcache[0]]; // читаем исходное расширение
                for (int i = 0; i < ext.Length; i++)
                {
                    ext[i] = bcache[i + 1];
                }
                string ExtensionOrigFile = Encoding.ASCII.GetString(ext);
                UInt32 powInd = (UInt32)ext.Length + 1;  // место pow в запакованном файле
                pow = bcache[powInd]; // читаем POW

                UInt32 SizeTable = 1u << pow;


                byte[] TableFtoS = new byte[SizeTable]; // читаем таблицу для декодирования размером 2 в степени pow
                Array.Copy(bcache, powInd + 1, TableFtoS, 0, SizeTable);


                UInt32 PackTableInd = powInd + 1 + SizeTable; // место начала закодированных данных в запакованном файле
                UInt32 PackTableSize = (UInt32)bcache.Length - PackTableInd;
                byte[] PackTable = new byte[PackTableSize]; // читаем упакованные данные для распаковки и записи в дальнейшем в распакованный файл

                Array.Copy(bcache, PackTableInd, PackTable, 0, PackTableSize);

                Decoder dec = new Decoder(TableFtoS, PackTable, pow); // готовим декодирование PackTable

                byte[] OUT = dec.GetS(); // раскодированнй массив

                BinWriter bw = new BinWriter(); // подготовка записи раскодированного файла
                bw.WriteFile(fileName, ExtensionOrigFile, OUT);

            }


        }



    }
}

