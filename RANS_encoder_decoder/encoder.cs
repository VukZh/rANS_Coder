using System;
using System.Collections.Generic;

namespace RANS_encoder_decoder
{
    public class Encoder
    {
        readonly byte[] ByteCache; // массив исходных байт для кодирования
        UInt32 CountState; // счетчик - для записи последнего состояния State
        readonly UInt32[] IndexByteInTable; // начало для байта в таблице для декодирования
        readonly UInt32[] LengthByteInTable; // диапазон для байта в таблице для декодирования
        readonly byte POW; // степень для кодирования
        readonly UInt32 NormalizeNum; // число для определения что требуется нормализация State
        readonly UInt32 FrequencySum; // размер таблицы для декодирования
        UInt32 State; // состояние кодировщика
        //private byte[] bcache;
        //private int v;
        readonly Queue<UInt16> StateArray; // очередь для записи состояний кодировщика

        public Encoder(byte[] bcache, byte pow, UInt32 StateInit) // запуск кодирования RANS; bcache - исходный массив байт; pow - степень для кодирования и для генерации таблицы для декодирования; state - инициализация начального состояния кодировщика (> 65536)
        {
            ByteCache = bcache;
            POW = pow;
            StatCalculator sc = new StatCalculator(); // запуск рассчета статистик исходного файла
            sc.Normalize_Frequency_Cumulative(ByteCache, POW); // запуск нормализации
            LengthByteInTable = sc.StatFreq_Element; // нормализованные частоты байт
            IndexByteInTable = sc.StatFreq_Cumulative; // нормализованные аккумулированные частоты байт
            NormalizeNum = 1u << 16; // число для сравнения при нормализации
            FrequencySum = 1u << pow; // размер таблицы байт-частот, сохраняется в закодированном файле (используется для вычисления байт и следующих состояний при раскодировке)
            State = StateInit; // инициализация начального состояния
            StateArray = new Queue<UInt16>();
        }


        public UInt16[] GetState()
        { // кодирование - побайтно рассчитываем состояние State кодировщика и заносим состояние в случае нормализации в очередь, потом после кодировки всех байт выводим в массив для записи в дальнейшем в файл
            int BCacheSize = ByteCache.Length;
            for (int i = 0; i < BCacheSize; i++)
            {
                EncodeByte(ByteCache[i]);

                //if (i % 1000000 == 0 && i != 0) // отображение хода кодировки
                //{
                //    Console.WriteLine(" encode " + i + " from " + BCacheSize + " byte ");
                //}

            }
            Console.WriteLine("encode OK");
            return StateArray.ToArray();
        }

        public byte[] CalculateTableFrequencyToByte() // рассчет таблицы для декодирования (размер 2 в степени POW) - для определения байта раскодировки по состянию State кодировщика
        {
            byte[] TableFrequencyToByte = new byte[FrequencySum];
            Queue<UInt32> indexFrequencyToByte = new Queue<UInt32>(); // таблица места байта в рассчитываемой таблице
            Queue<byte> valueFrequencyToByte = new Queue<byte>(); // таблица длины в рассчитываемой таблице для байта


            for (int i = 0; i < 256; i++)
            {
                if (LengthByteInTable[i] > 0)
                {
                    indexFrequencyToByte.Enqueue(IndexByteInTable[i] - 1);
                    indexFrequencyToByte.Enqueue(IndexByteInTable[i]);
                    valueFrequencyToByte.Enqueue((byte)i);
                }
            }
            indexFrequencyToByte.Dequeue();
            indexFrequencyToByte.Enqueue(FrequencySum - 1);

            int count = valueFrequencyToByte.Count;
            for (int i = 0; i < count; i++) // заполнение таблицы для декодирования данными из очередей indexFtoS/valueFtoS
            {

                UInt32 start = indexFrequencyToByte.Dequeue();
                UInt32 end = indexFrequencyToByte.Dequeue();
                byte value = valueFrequencyToByte.Dequeue();
                for (UInt32 j = start; j <= end; j++)
                {
                    TableFrequencyToByte[j] = value;
                }
            }
            return TableFrequencyToByte;
        }


        private void EncodeByte(byte b) // кодирование 1 байта
        {
            if (State < NormalizeNum || LengthByteInTable[b] <= 0 || LengthByteInTable[b] > FrequencySum)
            {
                Console.WriteLine("error normalize");
                Environment.Exit(0);
            } 

            CountState++;

            if ((State >> 16) >= (NormalizeNum >> POW) * LengthByteInTable[b]) // нормализация состояния кодировщика и его запись в очередь
            {
                StateArray.Enqueue((UInt16)State); // сохраняем 16 мл. разрядов состояния
                State >>= 16; // сдвигаем состояние 16 ст. разрядов на место мл. разрядов
            }
            

            State = ((State / LengthByteInTable[b] << POW) + IndexByteInTable[b] + (State % LengthByteInTable[b])); // рассчет нового состояния кодировщика в зависимости от старого состояния и нового поступившего байта

            if (State < NormalizeNum)
            {
                Console.WriteLine("error normalize");
                Environment.Exit(0);
            }


            if (CountState == ByteCache.Length) // запись последнего состояния кодировщика при необходимости с нормализацией
            {
                StateArray.Enqueue((UInt16)State);
                State >>= 16;
                StateArray.Enqueue((UInt16)State);
            }

        }

    }
}
