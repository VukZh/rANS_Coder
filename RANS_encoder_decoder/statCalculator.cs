using System;




namespace RANS_encoder_decoder
{
    class StatCalculator
    {
        readonly UInt32[] statFreq_Element = new UInt32[256]; // частоты байт искодного файла
        readonly UInt32[] statFreq_Cumulative = new UInt32[257]; // кумулятивные частоты байт искодного файла

        readonly bool[] flagNo0 = new bool[256]; // частоты байт не равные 0 (для далтнейшей перенормализации если частоты уменьшатся до 0)

        public UInt32[] StatFreq_Element { get => statFreq_Element; }
        public UInt32[] StatFreq_Cumulative { get => statFreq_Cumulative; }
        private void Calculate_Frequency(byte[] b) // считаем частоты и кумулятивные частоты байт искодного файла
        {

            foreach (var item in b)
            {
                statFreq_Element[item]++;
            }
            for (int i = 0; i < 256; i++)
            {
                statFreq_Cumulative[i + 1] = statFreq_Cumulative[i] + statFreq_Element[i];
                if (statFreq_Element[i] != 0)
                {
                    flagNo0[i] = true;
                }
            }
        }


        private void RecalcAfterNormalize(UInt32 freqmax, UInt32 freq0, byte ind) // пересчитываем частоты и кумулятивные частоты байт если идет перенормализуем после первичной нормализации
        {
            statFreq_Element[ind] = freqmax - freq0;

            for (int i = 0; i < 256; i++)
            {
                if (statFreq_Element[i] == 0 && flagNo0[i]) //////
                {
                    //statF_Element[i] = 1;
                    statFreq_Element[i] = 1;
                }
            }
            for (int i = 0; i < 256; i++)
            {
                statFreq_Cumulative[i + 1] = statFreq_Cumulative[i] + statFreq_Element[i];
            }


        }

        public void Normalize_Frequency_Cumulative(byte[] byte_in, byte pow) // первичная нормализация
        {
            UInt32 FrequencySum = 1u << pow;
            UInt64 FrequencySum_64 = Convert.ToUInt64(FrequencySum);

            UInt32 Freq0 = 0; // число обнуленных частот
            UInt32 FreqMax = 0; // максимальная частота из всех
            byte FMaxInd = 0; // позиция максимальной частоты из всех

            Calculate_Frequency(byte_in); // считаем частоты байт            

            UInt32 total_Frequency = statFreq_Cumulative[256]; // считаем кумулятивную частоту байт в конце массива statF_Cumulative

            for (int i = 1; i <= 256; i++) // нормализуем  кумулятивную частоту
            {
                statFreq_Cumulative[i] = Convert.ToUInt32((UInt64)FrequencySum_64 * statFreq_Cumulative[i] / total_Frequency);
            }


            for (int i = 0; i < 256; i++)  // нормализуем  частоту
            {


                if (statFreq_Element[i] != 0 && statFreq_Cumulative[i + 1] == statFreq_Cumulative[i]) // оптимизируем частоты с 0 (где не должно быть 0) на основе соседних частот
                {

                    UInt32 best_Frequency = ~0u;
                    int steal = -1;
                    for (int j = 0; j < 256; j++)
                    {
                        UInt32 F = statFreq_Cumulative[j + 1] - statFreq_Cumulative[j];
                        if (F > 1 && F < best_Frequency)
                        {
                            best_Frequency = F;
                            steal = j;
                        }
                    }

                    if (steal == -1) throw new ArgumentException("steal must not be -1");

                    if (best_Frequency < i)
                    {
                        for (int j = steal + 1; j <= i; j++)
                        {
                            statFreq_Cumulative[j]--;
                        }
                    }
                    else
                    {
                        if (steal <= i) throw new ArgumentException("error steal swap");
                        for (int j = i + 1; j <= steal; j++)
                        {
                            statFreq_Cumulative[j]++;
                        }
                    }
                }


            }

            if (statFreq_Cumulative[0] != 0 || statFreq_Cumulative[256] != FrequencySum)
            {
                Console.WriteLine("error normalize cumulative F");
                Environment.Exit(0);
            } 

            for (int i = 0; i < 256; i++)
            {

                statFreq_Element[i] = statFreq_Cumulative[i + 1] - statFreq_Cumulative[i];


                if (FreqMax < statFreq_Element[i]) // байт с максимальной частотой
                {
                    FreqMax = statFreq_Element[i];
                    FMaxInd = (byte)i;
                }


                if (statFreq_Element[i] == 0 && flagNo0[i]) // число байтов с частотой 0 (где не должно быть 0)
                {
                    Freq0++;
                }

            }



            if (Freq0 > 0) // число байтов с частотой 0
            {
                if (FreqMax > Freq0) // меньше чем максимальная частота
                {

                    RecalcAfterNormalize(FreqMax, Freq0, FMaxInd); // запускаем ренормализацию частот с 0 за счет байта с максимальной частотой                    

                    Console.WriteLine("recalc freq + normalize OK " + " (frequency=0 " + Freq0 + " ,frequency Max " + FreqMax + ")");

                }
                else // надо задать больше POW
                {
                    Console.WriteLine("POW IS SMALL !!!");
                    Environment.Exit(0);
                }


            }
            else
            {
                Console.WriteLine("normalize OK ");
            }

        }

    }
}
