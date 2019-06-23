using System;
using System.Collections.Generic;

namespace rANS_Coder
{
    class Decoder
    {
        UInt32 NormalizeNum;
        byte POW;
        UInt32[] Bs; // таблица адреса опреденного байта в таблице байт-частота
        UInt32[] Fs; // таблица длины (вероятности) байта в таблице байт-частота
        UInt32 F_All; // размер таблицы байт-частота
        UInt16[] Xarr; // массив состояний кодировки - для прохода по нему при декодировке

        byte[] BtoS; // считанная из архива таблица байт-частота
        byte[] PackTable;// считанная из архива таблица байт-частота

        Queue<byte> OutB; // очередь для хранения распакованных байт

        public Decoder(byte[] btos, byte[] packtable, byte pow)
        {
            NormalizeNum = 1u << 16; // число для определения что требуется нормализация X
            POW = pow;
            F_All = 1u << pow;
            //Range = 1u << 16;
            BtoS = btos;
            PackTable = packtable;

            Fs = new UInt32[256];
            Bs = new UInt32[257];
            OutB = new Queue<byte>();

            Xarr = new UInt16[PackTable.Length / 2]; // восстановление состояний X кодировки из byte по 2 байта в UInt16 - для дальнейшего декодирования
            CalcX(); // рассчет таблицы состояний X
            CalcFsBs(); // рассчет таблиц частот символов (Fs/Bs)

        }

        public void CalcX() // рассчет состояний раскодировщика на основе массива байт (byte -> UIint16)
        {
            int CountX = 0;
            for (int i = 0; i < PackTable.Length; i = i + 2)
            {
                byte[] Temp4byte = new byte[2];
                Array.Copy(PackTable, i, Temp4byte, 0, 2);
                Xarr[CountX] = BitConverter.ToUInt16(Temp4byte, 0);
                CountX++;
            }
        }


        public byte[] GetS() // восстановление байт на основе состояния кодировщика X
        {

            UInt32 XSize = (UInt32)Xarr.Length;

            UInt32 XN = Xarr[XSize - 1]; // читаем последнее состояние X, далее в процессе раскодировки идем к началу

            int i = (int)XSize - 2;

            while (XN != 77777 || i > 0)
            {

                if (XN < NormalizeNum) // renorm
                {
                    XN = XN << 16;
                    XN = XN + Xarr[i];
                    i--;

                    if ((XSize - i) % 1000000 == 0)
                    {
                        Console.WriteLine(" decode " + (XSize - i) + " from " + XSize + " UInt16 ");
                    }

                }

                OutB.Enqueue(DecodeS(XN));
                XN = NextX(XN);

            }

            Console.WriteLine("decode OK");
            return OutB.ToArray();

        }



        public byte DecodeS(UInt32 x)
        { // определение байта по состоянию декодировщика
            UInt32 xMOD = x % (F_All);
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
            return XN;
        }

    }
}
