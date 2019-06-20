using System;
using System.Collections.Generic;

namespace rANS_Coder
{
    class Decoder
    {

        Stack<UInt32> Xarr = new Stack<uint>();
        UInt32 NormalizeNum;
        byte POW;
        UInt32[] Bs;
        UInt32[] Fs;
        UInt32 F_All;
        UInt32 Range;

        UInt32 [] X;

        byte[] BtoS;
        byte[] PackTable;

        Queue <byte> OutB;

        public Decoder(byte[] btos, byte[] packtable, byte pow)
        {
            NormalizeNum = 1u << 16;
            POW = pow;
            F_All = 1u << pow;
            Range = 1u << 16;
            BtoS = btos;
            PackTable = packtable;

            Fs = new UInt32[256];
            Bs = new UInt32[257];
            OutB = new Queue <byte>();

            X = new UInt32[PackTable.Length/4]; // восстановление состояний X кодировки из byte по 4 байта в UInt32 - для дальнейшего декодирования
            CalcX();
            CalcFsBs();

        }

        public void CalcX() {
            int CountX = 0;
            for (int i = 0; i < PackTable.Length; i = i + 4)
            {

                //Console.WriteLine("i " + i + " CountX " + CountX);
                byte[] Temp4byte = new byte[4];
                Array.Copy(PackTable, i, Temp4byte, 0, 4);
                X[CountX] = BitConverter.ToUInt32(Temp4byte, 0);
                CountX++;
            }

            //for (int i = 0; i < X.Length; i++)
            //{
            //    Console.WriteLine(i + " xxx " + X[i]);
            //}
        }

        public byte [] GetS() // восстановление байт на основе состояния кодировщика X
        {

            UInt32 XSize= (UInt32) X.Length;

            UInt32 XN = X [XSize - 1];
            //Console.WriteLine("X [0]" + XN);


            int i = (int) XSize - 2;

            while (XN != 77777 || i > 0)
            {
                //Console.WriteLine("XN..." + XN);
                OutB.Enqueue (DecodeS(XN));  // восстановленный очередной байт
                XN = NextX(XN); // следующее состояние X

                if (XN < Range)
                {
                    //Console.WriteLine("renorm...");
                    
                    XN = X[i];
                    i--;

                    //if ((XSize - i) % 1000 == 0) {
                    //    Console.WriteLine(" decode " + (XSize - i) + " from " + XSize);
                    //}


                }
            }

            //if (XN == 77777)
            //{
            //    Console.WriteLine("OOOPS..." + i);
            //}

            Console.WriteLine("decode OK");
            return OutB.ToArray();


        }



        public byte DecodeS(UInt32 x)
        { // определение байта по состоянию декодировщика
            UInt32 xMOD = x % (F_All);
            //Console.WriteLine("-s " + BtoS[xMOD]);
            return BtoS[xMOD];
        }

        public void CalcFsBs() // рассчет таблиц частот символов
        {
            Fs[BtoS[0]] = 0;
            UInt32 prev = 0;
            for (UInt32 i = 1; i < BtoS.Length; i++)
            {
                if (BtoS[i] == BtoS[i - 1])
                {
                    continue;
                }
                else
                {
                    Fs[BtoS[prev]] = i - prev;
                    prev = i;
                }

            }
            Fs[BtoS[prev]] = F_All - prev;

            for (int i = 0; i < 256; i++)
            {

                Bs[i + 1] = Bs[i] + Fs[i];
            }

        }

        public UInt32 NextX(UInt32 xx) // рассчет следующего состояния декодировщика
        {

            if (xx < NormalizeNum) throw new ArgumentException("error normalize X");

            UInt32 xMOD = xx % (F_All);
            byte s = BtoS[xMOD];

            UInt32 XN = (xx >> POW) * Fs[s] + xMOD - Bs[s];


            //Console.WriteLine("xn " + XN);
            return XN;
        }

        public byte[] UintToByte(UInt32 number)
        {
            byte[] bytesUint = new byte[4];
            bytesUint = BitConverter.GetBytes(number);
            //for (int i = 0; i < bytesUint.Length; i++)
            //{
            //    Console.WriteLine(i + " byte " + bytesUint[i]);
            //}
            return bytesUint;
        }

        public UInt32 ByteToUint(byte[] bytesUint)
        {
            UInt32 number;
            number = BitConverter.ToUInt32(bytesUint, 0);
            //Console.WriteLine("number " + number);
            return number;
        }
    }
}
