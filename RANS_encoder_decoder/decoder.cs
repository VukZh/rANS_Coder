using System;
using System.Collections.Generic;

namespace RANS_encoder_decoder
{
    public class Decoder
    {
        readonly UInt32 NormalizeNum;
        readonly byte POW;
        readonly UInt32[] IndexByteInTable; // таблица адреса опреденного байта в таблице байт-частота
        readonly UInt32[] LengthByteInTable; // таблица длины (вероятности) байта в таблице байт-частота
        readonly UInt32 FrequencySum; // размер таблицы байт-частота
        readonly UInt16[] StateArray; // массив состояний кодировки - для прохода по нему при декодировке

        readonly byte[] TableFrequencyToByte; // считанная из архива таблица байт-частота
        readonly byte[] PackTable;// считанная из архива таблица байт-частота

        readonly Queue<byte> OutByteArray; // очередь для хранения распакованных байт

        public Decoder(byte[] btos, byte[] packtable, byte pow)
        {
            NormalizeNum = 1u << 16; // число для определения что требуется нормализация State
            POW = pow;
            FrequencySum = 1u << pow;
            //Range = 1u << 16;
            TableFrequencyToByte = btos;
            PackTable = packtable;

            LengthByteInTable = new UInt32[256];
            IndexByteInTable = new UInt32[257];
            OutByteArray = new Queue<byte>();

            StateArray = new UInt16[PackTable.Length / 2]; // восстановление состояний State кодировки из byte по 2 байта в UInt16 - для дальнейшего декодирования
            CalcState(); // рассчет таблицы состояний X
            CalculateIndexAndLengthByteTable(); // рассчет таблиц частот символов (IndexByteInTable/LengthByteInTable)

        }

        private void CalcState() // рассчет состояний раскодировщика на основе массива байт (byte -> UIint16)
        {
            int CountState = 0;
            for (int i = 0; i < PackTable.Length; i += 2)
            {
                byte[] Temp4byte = new byte[2];
                Array.Copy(PackTable, i, Temp4byte, 0, 2);
                StateArray[CountState] = BitConverter.ToUInt16(Temp4byte, 0);
                CountState++;
            }
        }


        public byte[] GetByte() // восстановление байт на основе состояния кодировщика State
        {

            UInt32 StateSize = (UInt32)StateArray.Length;

            UInt32 StateNext = StateArray[StateSize - 1]; // читаем последнее состояние State, далее в процессе раскодировки идем к началу

            int i = (int)StateSize - 2;

            while (StateNext != 77777 || i > 0)
            {

                if (StateNext < NormalizeNum) // renorm
                {
                    StateNext <<= 16;
                    StateNext += StateArray[i];
                    i--;
                }

                OutByteArray.Enqueue(DecodeByte(StateNext));
                StateNext = NextState(StateNext);

            }

            Console.WriteLine("decode OK");
            return OutByteArray.ToArray();

        }



        private byte DecodeByte(UInt32 state)
        { // определение байта по состоянию декодировщика
            UInt32 StateMOD = state % (FrequencySum);
            return TableFrequencyToByte[StateMOD];
        }

        private void CalculateIndexAndLengthByteTable() // рассчет таблиц частот символов
        {
            LengthByteInTable[TableFrequencyToByte[0]] = 0;
            UInt32 prev = 0;
            for (UInt32 i = 1; i < TableFrequencyToByte.Length; i++)
            {
                if (TableFrequencyToByte[i] == TableFrequencyToByte[i - 1])
                {
                    continue;
                }
                else
                {
                    LengthByteInTable[TableFrequencyToByte[prev]] = i - prev;
                    prev = i;
                }

            }
            LengthByteInTable[TableFrequencyToByte[prev]] = FrequencySum - prev;

            for (int i = 0; i < 256; i++)
            {

                IndexByteInTable[i + 1] = IndexByteInTable[i] + LengthByteInTable[i];
            }

        }

        private UInt32 NextState(UInt32 state) // рассчет следующего состояния декодировщика
        {
            if (state < NormalizeNum)
            {
                Console.WriteLine("error normalize encoder status");
                Environment.Exit(0);
            }
            UInt32 StateMOD = state % (FrequencySum);
            byte b = TableFrequencyToByte[StateMOD];
            UInt32 StateNext = (state >> POW) * LengthByteInTable[b] + StateMOD - IndexByteInTable[b];
            return StateNext;
        }

    }
}
