using System;




namespace rANS_Coder
{
    class StatCalculator
    {
        UInt32[] statF_Element = new UInt32[256]; // частоты байт искодного файла
        UInt32[] statF_Cumulative = new UInt32[257]; // кумулятивные частоты байт искодного файла


        byte pow;
        bool[] flagNo0 = new bool[256]; // частоты байт не равные 0 (для далтнейшей перенормализации если частоты уменьшатся до 0)

        public UInt32[] StatF_Element { get => statF_Element; }
        public UInt32[] StatF_Cumulative { get => statF_Cumulative; }
        public byte Pow { get => pow; }

        public void Calculate_F(byte[] b) // считаем частоты и кумулятивные частоты байт искодного файла
        {

            foreach (var item in b)
            {
                statF_Element[item]++;
            }
            for (int i = 0; i < 256; i++)
            {
                statF_Cumulative[i + 1] = statF_Cumulative[i] + statF_Element[i];
                if (statF_Element[i] != 0)
                {
                    flagNo0[i] = true;
                }
            }
        }


        public void RecalcAfterNormalize(UInt32 fm, UInt32 f0, byte ind) // пересчитываем частоты и кумулятивные частоты байт если идет перенормализуем после первичной нормализации
        {
            statF_Element[ind] = fm - f0;

            for (int i = 0; i < 256; i++)
            {
                if (statF_Element[i] == 0 && flagNo0[i]) //////
                {
                    //statF_Element[i] = 1;
                    statF_Element[i] = 1;
                }
            }
            for (int i = 0; i < 256; i++)
            {
                statF_Cumulative[i + 1] = statF_Cumulative[i] + statF_Element[i];
            }


        }

        public void Normalize_F_Cumulative(byte[] b_in, byte pow) // первичная нормализация
        {
            UInt32 k = 1u << pow;
            UInt64 kB = Convert.ToUInt64(k);

            UInt32 F0 = 0; // число обнуленных частот
            UInt32 FMax = 0; // максимальная частота из всех
            byte FMaxInd = 0; // позиция максимальной частоты из всех

            if (k < 256) throw new ArgumentException("must be greater than 7");
            Calculate_F(b_in); // считаем частоты байт

            //for (int i = 0; i < 256; i++)
            //{
            //    Console.WriteLine(i + " statF_Element old " + statF_Element[i]);
            //}

            UInt32 total_F = statF_Cumulative[256]; // считаем кумулятивную частоту байт в конце массива statF_Cumulative

            for (int i = 1; i <= 256; i++) // нормализуем  кумулятивную частоту
            {
                statF_Cumulative[i] = Convert.ToUInt32((UInt64)kB * statF_Cumulative[i] / total_F);
            }


            for (int i = 0; i < 256; i++)  // нормализуем  частоту
            {


                if (statF_Element[i] != 0 && statF_Cumulative[i + 1] == statF_Cumulative[i]) // оптимизируем частоты с 0 (где не должно быть 0) на основе соседних частот
                {

                    UInt32 best_F = ~0u;
                    int steal = -1;
                    for (int j = 0; j < 256; j++)
                    {
                        UInt32 F = statF_Cumulative[j + 1] - statF_Cumulative[j];
                        if (F > 1 && F < best_F)
                        {
                            best_F = F;
                            steal = j;
                        }
                    }

                    if (steal == -1) throw new ArgumentException("steal must not be -1");

                    if (best_F < i)
                    {
                        for (int j = steal + 1; j <= i; j++)
                        {
                            statF_Cumulative[j]--;
                        }
                    }
                    else
                    {
                        if (steal <= i) throw new ArgumentException("error steal swap");
                        for (int j = i + 1; j <= steal; j++)
                        {
                            statF_Cumulative[j]++;
                        }
                    }
                }


            }

            if (statF_Cumulative[0] != 0 || statF_Cumulative[256] != k) throw new ArgumentException("error normalize cumulative F");

            for (int i = 0; i < 256; i++)
            {

                statF_Element[i] = statF_Cumulative[i + 1] - statF_Cumulative[i];


                if (FMax < statF_Element[i]) // байт с максимальной частотой
                {
                    FMax = statF_Element[i];
                    FMaxInd = (byte)i;
                }


                if (statF_Element[i] == 0 && flagNo0[i]) // число байтов с частотой 0 (где не должно быть 0)
                {
                    F0++;
                }

            }



            if (F0 > 0) // число байтов с частотой 0
            {
                if (FMax > F0) // меньше чем максимальная частота
                {

                    RecalcAfterNormalize(FMax, F0, FMaxInd); // запускаем ренормализацию частот с 0 за счет байта с максимальной частотой

                    //for (int i = 0; i < 256; i++)
                    //{
                    //    Console.WriteLine(i + " statF_Element norm " + statF_Element[i]);
                    //}

                    Console.WriteLine("normalize + recalc freq OK " + " F0 " + F0 + " FMax " + FMax);

                }
                else // надо задать больше POW
                {
                    throw new ArgumentException("size table small" + " F0 " + F0 + " FMax " + FMax + " POW IS SMALL !!!");
                }


            }
            else
            {
                Console.WriteLine("normalize OK ");
            }

        }

    }
}
