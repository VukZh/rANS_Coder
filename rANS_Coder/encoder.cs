using System;
using System.Collections.Generic;

namespace rANS_Coder
{
    class Encoder
    {
        byte[] BCache;
        UInt32 CountS;
        UInt32[] Bs; // начало для байта в таблице для декодирования
        UInt32[] Fs; // диапазон для байта в таблице для декодирования
        byte POW; // степень для кодирования
        UInt32 NormalizeNum; // число для определения что требуется нормализация X
        UInt32 FAll; // размер таблицы для декодирования
        UInt32 X; // состояние кодировщика
        Queue<UInt16> Xarr; // очередь для записи состояний кодировщика

        public Encoder(byte[] bcache, byte pow, UInt32 x) // запуск кодирования RANS; bcache - исходный массив байт; pow - степень для кодирования и для генерации таблицы для декодирования; x - инициализация начального состояния кодировщика (> 65536)
        {
            BCache = bcache;
            POW = pow;
            StatCalculator sc = new StatCalculator(); // запуск рассчета статистик исходного файла
            sc.Normalize_F_Cumulative(BCache, POW);
            Fs = sc.StatF_Element; // нормализованные частоты байт
            Bs = sc.StatF_Cumulative; // нормализованные аккумулированные частоты байт
            NormalizeNum = 1u << 16;
            FAll = 1u << pow;
            X = x;
            Xarr = new Queue<UInt16>();


        }

        public UInt16[] GetX()
        { // кодирование - побайтно рассчитываем состояние X кодировщика и заносим состояние в случае нормализации в стэк, стэк выводим в массив для записи в дальнейшем в файл
            int BCacheSize = BCache.Length;
            for (int i = 0; i < BCacheSize; i++)
            {
                Encode(BCache[i]);

                if (i % 1000 == 0)
                {
                    Console.WriteLine(" encode " + i + " from " + BCacheSize);
                }

            }
            Console.WriteLine("encode OK");
            return Xarr.ToArray();
        }

        public byte[] CalcTabFtoS() // рассчет таблицы для декодирования (размер 2 в степени POW) - для определения байта раскодировки по состянию Х кодировщика
        {
            byte[] BtoS = new byte[FAll];
            Queue<UInt32> indexFtoS = new Queue<UInt32>();
            Queue<byte> valueFtoS = new Queue<byte>();


            for (int i = 0; i < 256; i++)
            {

                if (Fs[i] > 0)
                {
                    //Console.WriteLine(Bs[i] + " >> " + i);

                    indexFtoS.Enqueue(Bs[i] - 1);
                    indexFtoS.Enqueue(Bs[i]);
                    valueFtoS.Enqueue((byte)i);
                }
            }
            indexFtoS.Dequeue();
            indexFtoS.Enqueue(FAll - 1);

            int count = valueFtoS.Count;
            for (int i = 0; i < count; i++)
            {


                UInt32 start = indexFtoS.Dequeue();
                UInt32 end = indexFtoS.Dequeue();
                byte value = valueFtoS.Dequeue();
                for (UInt32 j = start; j <= end; j++)
                {
                    BtoS[j] = value;
                }
            }

            return BtoS;
        }


        public void Encode(byte s) // кодирование 1 байта
        {
            if (X < NormalizeNum) throw new ArgumentException("error normalize X");

            if (Fs[s] <= 0 || Fs[s] > FAll) throw new ArgumentException("error get F");


            CountS++;


            if ((X >> 16) >= (NormalizeNum >> POW) * Fs[s]) // нормализация состояния кодировщика и его запись в очередь
            {

                //Console.WriteLine("X >> 16 " + X + " " + " NormalizeNum >> POW " + (NormalizeNum >> POW) + " " + " X " + X + " xSmall " + (UInt16)X);
                Xarr.Enqueue((UInt16)X); // сохраняем 16 мл. разрядов состояния
                X >>= 16; // сдвигаем состояние 16 ст. разрядов на место мл. разрядов

            }



            X = ((X / Fs[s] << POW) + Bs[s] + (X % Fs[s])); // рассчет нового состояния кодировщика в зависимости от старого состояния и нового поступившего байта

            if (X < NormalizeNum) throw new ArgumentException("error normalize X");

            //Console.WriteLine("x " + X + " s " + s + " Fs " + Fs[s] + " Bs " + Bs[s]);




            if (CountS == BCache.Length) // запись последнего состояния кодировщика при необходимости с нормализацией
            {
                //Console.WriteLine((X >> 16) + " >= " + (NormalizeNum >> POW) * Fs[s] + " NormalizeNum " + NormalizeNum + " Fs[s] " + Fs[s]);


                //Console.WriteLine(" X " + X + " xSmall " + (UInt16)X);
                Xarr.Enqueue((UInt16)X);

                X >>= 16;
                //Console.WriteLine(" X- " + X + " xSmall- " + (UInt16)X);
                Xarr.Enqueue((UInt16)X);

            }

        }

    }
}
