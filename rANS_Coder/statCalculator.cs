using System;




namespace rANS_Coder
{
    class StatCalculator
    {
        UInt32[] statF_Element = new UInt32[256];
        UInt32[] statF_Cumulative = new UInt32[257];


        byte pow;
        bool[] flagNo0 = new bool[256];

        public UInt32[] StatF_Element { get => statF_Element; }
        public UInt32[] StatF_Cumulative { get => statF_Cumulative; }
        public byte Pow { get => pow; }

        public UInt32[] Calculate_F(byte[] b)
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


            return statF_Element;
        }


        public UInt32 AllF()
        {
            UInt32 res = 0;

            foreach (var item in statF_Element)
            {
                res += item;
            }
            return res;
        }

        public int Size2Cache(int L)
        {

            UInt32 af = AllF();

            do
            {

                if (af > L >> 1)
                {
                    return L;
                }
                else
                {
                    L = L >> 1;
                }

            } while (true);

        }

        public UInt32 CalcPOW(byte pw)
        {
            UInt32 k = 1;
            UInt32 n = 2;
            pow = pw;
            while (Pow > 0)
            {
                if ((Pow & 1) == 0)
                {
                    n = n * n;
                    pow >>= 1;
                }
                else
                {
                    k = k * n;
                    --pow;
                }

            }
            return k;
        }

        public void Normalize_F_Cumulative(byte[] b_in, byte pow)
        {
            UInt32 k = CalcPOW(pow);
            UInt64 kB = Convert.ToUInt64(k); ;


            if (k < 256) throw new ArgumentException("must be greater than 7");
            Calculate_F(b_in);
            UInt32 total_F = statF_Cumulative[256];

            for (int i = 1; i <= 256; i++)
            {
                statF_Cumulative[i] = Convert.ToUInt32((UInt64)kB * statF_Cumulative[i] / total_F);
            }


            for (int i = 0; i < 256; i++)
            {


                if (statF_Element[i] != 0 && statF_Cumulative[i + 1] == statF_Cumulative[i])
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


                if (statF_Element[i] == 0)
                {
                    if (statF_Cumulative[i + 1] != statF_Cumulative[i]) throw new ArgumentException("error cumulative for F = 0 " + i);
                }
                else
                {
                    if (statF_Cumulative[i + 1] < statF_Cumulative[i]) throw new ArgumentException("error growth cumulative");
                }
                statF_Element[i] = statF_Cumulative[i + 1] - statF_Cumulative[i];

                if (statF_Element[i] == 0 && flagNo0[i]) //////
                {
                    statF_Element[i] = 1;
                    throw new ArgumentException("size table small !!! " + i);
                }

            }
            Console.WriteLine("normalize OK");
        }

    }
}
