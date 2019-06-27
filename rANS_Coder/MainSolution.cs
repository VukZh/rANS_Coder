using System;
using System.Text;
using RNS = RANS_encoder_decoder;

namespace rANS_Coder
{
    class MainSolution
    {
        string fileName; // имя файла
        bool FlagRns; // флаг архива
        byte pow;  // степень размера для таблицы кодирования
        readonly string[] argumentsCommandLine; // аргументы командной строки
        byte[] bcache; // массив считанных байт
        byte[] ext; // расширение

        public MainSolution(string[] arguments)
        {
            argumentsCommandLine = arguments;
        }

        public void InitSolution() // определяем аргументы, читаем файл и определяем что делать
        {

            if (argumentsCommandLine.Length == 0) // нет параметров командной строки
            {
                Console.WriteLine("argument out (must be 1-filename and 2-number for table size (8-16, default 12))");
                return;
            }

            else if (argumentsCommandLine.Length == 1) // 1 параметр командной строки
            {
                fileName = argumentsCommandLine[0];
                pow = 12; // default pow
            }
            else
            { // 2 параметра командной строки


                if (byte.TryParse(argumentsCommandLine[0], out pow)) // 1 параметр - pow; 2 параметр - имя файла
                {
                    fileName = argumentsCommandLine[1];
                }
                else
                {
                    byte.TryParse(argumentsCommandLine[1], out pow); // 2 параметр - pow; 1 параметр - имя файла
                    fileName = argumentsCommandLine[0];
                }
            }

            if (pow < 8 || pow > 16) // проверка выхода pow за пределы
            {
                Console.WriteLine("pow should be > 7 and pow < 17");
                Environment.Exit(0);
            }

            IReader breader = new BinReader(); // читаем файл

            bcache = breader.ReadFile(fileName); // весь файл сохраняем в массиве байт






            FlagRns = breader.FlagRNS; // определяем тип открываемого файла
            if (!FlagRns)
            {
                ext = breader.GetExtension;
                InitEncode();
            }
            else
                InitDecode();

            // берем расширение для сохранения внутри файла
        }





        private void InitEncode() // кодируем файл
        {
            IWriter bw = new BinWriter(); // подготовка записи закодированного файла

            Array.Reverse(bcache); // для кодирования разворачиваем исходный массив байт (LIFO)
            bw.WriteFile(fileName, (byte)ext.Length); // 1 - записываем размер расширения в файл архива
            bw.ContinueWriteToFile(ext); // 2 - записываем расширение
            bw.ContinueWriteToFile(pow); // 3 - записываем степень

            RNS.Encoder enc = new RNS.Encoder(bcache, pow, 77777); // подготовка кодирования массива bcache со степенью pow с начальной инициализацией кодировщика (77777) - также проверяется на это число в раскодировке

            byte[] TableFtoS = enc.CalculateTableFrequencyToByte(); // рассчет таблицы для вычисления байт при дальнейшей раскодировке размером 2^pow

            bw.ContinueWriteToFile(TableFtoS); // 4 - записываем рассчитанную на предыдущем шаге таблицу

            UInt16[] xArray = enc.GetState(); // получаем массив чисел - состояние кодировщика

            bw.ContinueWriteToFile(xArray); // 5 - записываем состояние кодировщика
        }





        private void InitDecode() // декодируем файл
        {
            byte[] ext = new byte[bcache[0]]; // читаем исходное расширение
            for (int i = 0; i < ext.Length; i++)
            {
                ext[i] = bcache[i + 1];
            }

            string ExtensionOrigFile = Encoding.ASCII.GetString(ext);
            UInt32 powInd = (UInt32)ext.Length + 1;  // место pow в запакованном файле
            pow = bcache[powInd]; // читаем POW

            UInt32 SizeTable = 1u << pow; // размер таблицы для вычисления байт при раскодировке размером 2^pow

            byte[] TableFtoS = new byte[SizeTable]; // читаем таблицу для декодирования размером 2 в степени pow
            Array.Copy(bcache, powInd + 1, TableFtoS, 0, SizeTable);

            UInt32 PackTableInd = powInd + 1 + SizeTable; // место начала закодированных данных в запакованном файле
            UInt32 PackTableSize = (UInt32)bcache.Length - PackTableInd; // размер закодированных данных в запакованном файле
            byte[] PackTable = new byte[PackTableSize]; // читаем упакованные данные для распаковки и записи в дальнейшем в распакованный файл

            Array.Copy(bcache, PackTableInd, PackTable, 0, PackTableSize);

            RNS.Decoder dec = new RNS.Decoder(TableFtoS, PackTable, pow); // готовим декодирование PackTable

            byte[] OUT = dec.GetByte(); // раскодированнй массив

            IWriter bw = new BinWriter(); // подготовка записи раскодированного файла
            bw.WriteFile(fileName, ExtensionOrigFile, OUT);
        }


    }
}
