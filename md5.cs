using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace md5
{
    class md5
    {
        uint A = 0x67452301;
        uint B = 0xefcdab89;
        uint C = 0x98badcfe;
        uint D = 0x10325476;
        uint[] T;
        delegate word Md5Func(word x, word y, word z);
        word[] X;
        public md5()
        {
            T = new uint[64];
            for (int i = 0; i < 64; i++)
            {
                T[i] = (uint)(Math.Pow(2, 32) * Math.Abs(Math.Sin(i + 1)));
            }
            X = new word[16];
        }
        public string GetHash(string msg)
        {
            StringBuilder PaddedMessage=new StringBuilder(GetBinary(msg));
            //выравнивание данных к 448 по модулю 512
            PaddedMessage.Append("1");
            uint BeginMessageLength = (uint)PaddedMessage.Length;
            while(PaddedMessage.Length % 512!=448)
            {
                PaddedMessage.Append("0");
            }
            //дописывание 64 бит длинны сообщения
            word MessageLength=new word(BeginMessageLength-1,-1);
            short [] LengthBit = new short[64];
            int iter = 0;
            for (int i = 7; i >= 0; i--)
            {
                for (int j = 0; j < 8; j++)
                {
                    LengthBit[iter] = MessageLength[i * 8 + j];
                    iter++;
                }
            }
            for (int i = 0; i < LengthBit.Length; i++)
            {
                if (LengthBit[i] == 1) PaddedMessage.Append("1");
                else PaddedMessage.Append("0");
            }

            //начало циклов md5
            int BigBloks = PaddedMessage.Length / 512;
            A = 0x67452301;
            B = 0xefcdab89;
            C = 0x98badcfe;
            D = 0x10325476;
            uint AA;
            uint BB;
            uint CC;
            uint DD;
            short[] temp;
            int iterator = 0;
            for (int i = 0; i < BigBloks; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    temp = new short[32];
                    for (int selected_byte = 3; selected_byte >= 0; selected_byte--)
                    {
                        for (int k = 0; k < 8; k++)
                        {
                            if (PaddedMessage[iterator] == '1') temp[selected_byte * 8 + k] = 1;
                            else temp[k] = 0;
                            iterator++;
                        }
                    }
                    X[j] = new word(temp);
                }
                AA = A;
                BB = B;
                CC = C;
                DD = D;
                string[] Registers = new string[]{"ABCD","DABC","CDAB","BCDA",
                                                  "ABCD","DABC","CDAB","BCDA",
                                                  "ABCD","DABC","CDAB","BCDA",
                                                  "ABCD","DABC","CDAB","BCDA"};
                //этап 1
                // [abcd k s i] a = b + ((a + F(b,c,d) + X[k] + T[i]) <<< s).
                //[ABCD  0 7  1][DABC  1 12  2][CDAB  2 17  3][BCDA  3 22  4]
                //[ABCD  4 7  5][DABC  5 12  6][CDAB  6 17  7][BCDA  7 22  8]
                //[ABCD  8 7  9][DABC  9 12 10][CDAB 10 17 11][BCDA 11 22 12]
                //[ABCD 12 7 13][DABC 13 12 14][CDAB 14 17 15][BCDA 15 22 16]
                int[] S = new int[] { 7, 12, 17, 22,
                                      7, 12, 17, 22,
                                      7, 12, 17, 22,
                                      7, 12, 17, 22 };
                
                for(int step=0;step<16;step++)
                {
                    Step(Registers[step][0], Registers[step][1], Registers[step][2], Registers[step][3], F, step, S[step], step);
                }
                //этап 2
                // [abcd k s i] a = b + ((a + G(b,c,d) + X[k] + T[i]) <<< s). 
                // [ABCD  1 5 17][DABC  6 9 18][CDAB 11 14 19][BCDA  0 20 20]
                // [ABCD  5 5 21][DABC 10 9 22][CDAB 15 14 23][BCDA  4 20 24]
                // [ABCD  9 5 25][DABC 14 9 26][CDAB  3 14 27][BCDA  8 20 28]
                // [ABCD 13 5 29][DABC  2 9 30][CDAB  7 14 31][BCDA 12 20 32]
                S = new int[] { 5, 9, 14, 20,
                                5, 9, 14, 20,
                                5, 9, 14, 20,
                                5, 9, 14, 20 };
                int[] K = new int[] { 1, 6, 11, 0, 5, 10, 15, 4, 9, 14, 3, 8, 13, 2, 7, 12 };
                for (int step = 0; step < 16; step++)
                {
                    Step(Registers[step][0], Registers[step][1], Registers[step][2], Registers[step][3], G, K[step], S[step], step + 16);
                }
                //этап 3
                // [abcd k s i] a = b + ((a + H(b,c,d) + X[k] + T[i]) <<< s).
                // [ABCD  5 4 33][DABC  8 11 34][CDAB 11 16 35][BCDA 14 23 36]
                // [ABCD  1 4 37][DABC  4 11 38][CDAB  7 16 39][BCDA 10 23 40]
                // [ABCD 13 4 41][DABC  0 11 42][CDAB  3 16 43][BCDA  6 23 44]
                // [ABCD  9 4 45][DABC 12 11 46][CDAB 15 16 47][BCDA  2 23 48]
                K = new int[] { 5, 8, 11, 14, 1, 4, 7, 10, 13, 0, 3, 6, 9, 12, 15, 2 };
                S = new int[] { 4, 11, 16, 23,
                                4, 11, 16, 23,
                                4, 11, 16, 23,
                                4, 11, 16, 23 };
                for (int step = 0; step < 16; step++)
                {
                    Step(Registers[step][0], Registers[step][1], Registers[step][2], Registers[step][3], H, K[step], S[step], step + 32);
                }
                //[abcd k s i] a = b + ((a + I(b,c,d) + X[k] + T[i]) <<< s).
                //[ABCD  0 6 49][DABC  7 10 50][CDAB 14 15 51][BCDA  5 21 52]
                //[ABCD 12 6 53][DABC  3 10 54][CDAB 10 15 55][BCDA  1 21 56]
                //[ABCD  8 6 57][DABC 15 10 58][CDAB  6 15 59][BCDA 13 21 60]
                //[ABCD  4 6 61][DABC 11 10 62][CDAB  2 15 63][BCDA  9 21 64]
                K = new int[] { 0, 7, 14, 5, 12, 3, 10, 1, 8, 15, 6, 13, 4, 11, 2, 9 };
                S = new int[] { 6, 10, 15, 21,
                                6, 10, 15, 21,
                                6, 10, 15, 21,
                                6, 10, 15, 21 };
                for (int step = 0; step < 16; step++)
                {
                    Step(Registers[step][0], Registers[step][1], Registers[step][2], Registers[step][3], I, K[step], S[step], step + 48);
                }
                A = AA + A;
                B = BB + B;
                C = CC + C;
                D = DD + D;
            }
            word A_Word = new word(A);
            word B_Word = new word(B);
            word C_Word = new word(C);
            word D_Word = new word(D);
            short[] result_mas = new short[128];
            int index = 0;
            for (int selected_bit=3; selected_bit>=0;selected_bit-- )
                for (int i = 0; i < 8; i++)
                {
                    result_mas[index] = A_Word[selected_bit*8+i];
                    index++;
                }
            for (int selected_bit = 3; selected_bit >= 0; selected_bit--)
            {
                for (int i = 0; i < 8; i++)
                {
                    result_mas[index] = B_Word[selected_bit * 8 + i];
                    index++;
                }
            }
            for (int selected_bit = 3; selected_bit >= 0; selected_bit--)
            {
                for (int i = 0; i < 8; i++)
                {
                    result_mas[index] = C_Word[selected_bit * 8 + i];
                    index++;
                }
            }
            for (int selected_bit = 3; selected_bit >= 0; selected_bit--)
            {
                for (int i = 0; i < 8; i++)
                {
                    result_mas[index] = D_Word[selected_bit * 8 + i];
                    index++;
                }
            }
            StringBuilder result = new StringBuilder();
            StringBuilder Letter16 = new StringBuilder();
            StringBuilder result_string = new StringBuilder();
            for (index = 0; index < 128;index++ )
            {
                if (result_mas[index] == 1)
                {
                    Letter16.Append("1");
                    result_string.Append("1");
                }
                else 
                {
                    Letter16.Append("0");
                    result_string.Append("0");
                }
                if(Letter16.Length==4)
                {
                    switch (Letter16.ToString())
                    {
                        case "0000":
                            result.Append("0");
                            break;
                        case "0001":
                            result.Append("1");
                            break;
                        case "0010":
                            result.Append("2");
                            break;
                        case "0011":
                            result.Append("3");
                            break;
                        case "0100":
                            result.Append("4");
                            break;
                        case "0101":
                            result.Append("5");
                            break;
                        case "0110":
                            result.Append("6");
                            break;
                        case "0111":
                            result.Append("7");
                            break;
                        case "1000":
                            result.Append("8");
                            break;
                        case "1001":
                            result.Append("9");
                            break;
                        case "1010":
                            result.Append("A");
                            break;
                        case "1011":
                            result.Append("B");
                            break;
                        case "1100":
                            result.Append("C");
                            break;
                        case "1101":
                            result.Append("D");
                            break;
                        case "1110":
                            result.Append("E");
                            break;
                        case "1111":
                            result.Append("F");
                            break;
                    }
                    Letter16.Clear();
                }
            }
            A = 0x01234567;
            B = 0x89abcdef;
            C = 0xfedcba98;
            D = 0x76543210;
            return result.ToString();
        }
        private void Step(char First, char Second, char Tried, char Four, Md5Func Function, int k, int s, int i)
        {
            uint First_buf=0, Second_buf=0, Tried_buf=0, Four_buf=0;
            switch (First)
            {
                case 'A':
                    First_buf = A;
                    break;
                case 'B':
                    First_buf = B;
                    break;
                case 'C':
                    First_buf = C;
                    break;
                case 'D':
                    First_buf = D;
                    break;
            }
            switch (Second)
            {
                case 'A':
                    Second_buf = A;
                    break;
                case 'B':
                    Second_buf = B;
                    break;
                case 'C':
                    Second_buf = C;
                    break;
                case 'D':
                    Second_buf = D;
                    break;
            }
            switch (Tried)
            {
                case 'A':
                    Tried_buf = A;
                    break;
                case 'B':
                    Tried_buf = B;
                    break;
                case 'C':
                    Tried_buf = C;
                    break;
                case 'D':
                    Tried_buf = D;
                    break;
            }
            switch (Four)
            {
                case 'A':
                    Four_buf = A;
                    break;
                case 'B':
                    Four_buf = B;
                    break;
                case 'C':
                    Four_buf = C;
                    break;
                case 'D':
                    Four_buf = D;
                    break;
            }
            //uint temp = ;
            //temp = temp;
            uint result = Second_buf + new word(First_buf + Function(new word(Second_buf), new word(Tried_buf), new word(Four_buf)).Raw_word + X[k].Raw_word + T[i]).LeftShift(s);
            switch (First)
            {
                case 'A':
                    A = result;
                    break;
                case 'B':
                    B = result;
                    break;
                case 'C':
                    C = result;
                    break;
                case 'D':
                    D = result;
                    break;
            }
        }
        private string GetBinary(string msg)
        {
            int CurrentLetter;
            Queue<short> stroka = new Queue<short>();
            for(int i=0;i<msg.Length;i++)
            {
                CurrentLetter = (int)msg[i];
                for (int j = 0; j < 8; j++)
                {
                    if ((CurrentLetter & (1 << 8 - 1 - j)) != 0) stroka.Enqueue(1);
                    else stroka.Enqueue(0);
                }
            }
            short[] mas = stroka.ToArray();
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < mas.Length; i++)
            {
                if (mas[i]==1) result.Append("1");
                else result.Append("0");
            }
            return result.ToString();
        }
        //F(x, y, z) = (x & y) | (~x & z)
        //G(x, y, z) = (x & z) | (y & ~z)
        //H(x, y, z) = x ^ y ^ z
        //I(x, y, z) = y ^ (x | ~z) 
        //x y z F   x y z G   x y z H     x y z I
        //0 0 0 0   0 0 0 0   0 0 0 0     0 0 0 1
        //0 0 1 1   0 0 1 0   0 0 1 1     0 0 1 0
        //0 1 0 0   0 1 0 1   0 1 0 1     0 1 0 0
        //0 1 1 1   0 1 1 0   0 1 1 0     0 1 1 1
        //1 0 0 0   1 0 0 0   1 0 0 1     1 0 0 1
        //1 0 1 0   1 0 1 1   1 0 1 0     1 0 1 1
        //1 1 0 1   1 1 0 1   1 1 0 0     1 1 0 0
        //1 1 1 1   1 1 1 1   1 1 1 1     1 1 1 0 
        private word F(word x, word y, word z)
        {
            short[] result = new short[32];
            for (int i = 0; i < 32; i++)
            {
                if ((x[i] == 0 && y[i] == 0 && z[i] == 1) ||
                    (x[i] == 0 && y[i] == 1 && z[i] == 1) ||
                    (x[i] == 1 && y[i] == 1 && z[i] == 0) ||
                    (x[i] == 1 && y[i] == 1 && z[i] == 1)) result[i] = 1;
                else result[i] = 0;
            }
            return new word(result);
        }
        private word G(word x, word y, word z)
        {
            short[] result = new short[32];
            for (int i = 0; i < 32; i++)
            {
                if ((x[i] == 0 && y[i] == 1 && z[i] == 0) ||
                    (x[i] == 1 && y[i] == 0 && z[i] == 1) ||
                    (x[i] == 1 && y[i] == 1 && z[i] == 0) ||
                    (x[i] == 1 && y[i] == 1 && z[i] == 1)) result[i] = 1;
                else result[i] = 0;
            }
            return new word(result);
        }
        private word H(word x, word y, word z)
        {
            short[] result = new short[32];
            for (int i = 0; i < 32; i++)
            {
                if ((x[i] == 0 && y[i] == 0 && z[i] == 1) ||
                    (x[i] == 0 && y[i] == 1 && z[i] == 0) ||
                    (x[i] == 1 && y[i] == 0 && z[i] == 0) ||
                    (x[i] == 1 && y[i] == 1 && z[i] == 1)) result[i] = 1;
                else result[i] = 0;
            }
            return new word(result);
        }
        private word I(word x, word y, word z)
        {
            short[] result = new short[32];
            for (int i = 0; i < 32; i++)
            {
                if ((x[i] == 0 && y[i] == 0 && z[i] == 0) ||
                    (x[i] == 0 && y[i] == 1 && z[i] == 1) ||
                    (x[i] == 1 && y[i] == 0 && z[i] == 0) ||
                    (x[i] == 1 && y[i] == 0 && z[i] == 1)) result[i] = 1;
                else result[i] = 0;
            }
            return new word(result);
        }
    }
    class word
    {
        uint raw_word;
        public uint Raw_word
        {
            get 
            {
                return raw_word;
            }
        }
        short[] data;
        public short this[int i]
        {
            get
            {
                return data[i];
            }
        }
        int length;
        public int Length
        { 
            get 
            { 
                return length;
            } 
        }
        public void SetData(string msgdigit)
        {
            data = new short[msgdigit.Length];
            length = msgdigit.Length;
            for (int i = 0; i < msgdigit.Length; i++)
            {
                if (msgdigit[i] == '1')
                {
                    raw_word = raw_word | (uint)(1 << msgdigit.Length - 1 - i);
                    data[i] = 1;
                }
                else data[i] = 0;
            }
        }
        public void SetData(uint msgdigit)
        {
            Stack<short> bufer = new Stack<short>();
            raw_word = msgdigit;
            for (int i = 0; i < 32; i++)
            {
                if ((msgdigit & (1 << i)) != 0)
                {
                    bufer.Push(1);
                }
                else bufer.Push(0);
            }
            while (bufer.Count!=0 && bufer.Peek() == 0) bufer.Pop();
            data = bufer.ToArray();
            //Array.Reverse(data);
            length = data.Length;
        }
        public void SetData(short[] msgdigit)
        {
            data = msgdigit;
            length = msgdigit.Length;
            raw_word = 0;
            for (int i = 0; i < msgdigit.Length; i++)
            {
                if (data[i] == 1)
                {
                    raw_word = raw_word | (uint)(1 << msgdigit.Length - 1 - i);
                }
            }
        }
        public word(uint msgdigit, bool to32size = true)
        {
            SetData(msgdigit);
            if (to32size)
            {
                int padding = 32 - length;
                if (padding != 0)
                {
                    short[] mas = new short[32];
                    for (int i = 0; i < length; i++)
                    {
                        mas[i + padding] = data[i];
                    }
                    data = mas;
                }
            }
        }
        public word(uint msgdigit, int To64)
        {
            SetData(msgdigit);
            if (To64!=0)
            {
                int padding = 64 - length;
                if (padding != 0)
                {
                    short[] mas = new short[64];
                    for (int i = 0; i < length; i++)
                    {
                        mas[i + padding] = data[i];
                    }
                    data = mas;
                }
            }
        }
        public word(string msgdigit)
        {
            SetData(msgdigit);
        }
        public word(short[] msgdigit)
        {
            SetData(msgdigit);
        }
        public uint LeftShift(int s)
        {
            short bufer;
            for (int iteration = 0; iteration < s; iteration++)
            {
                bufer = data[0];
                for (int i = 1; i < data.Length; i++)
                {
                    data[i - 1] = data[i];
                }
                data[data.Length - 1] = bufer;
            }
            SetData(data);
            return raw_word;
        }
    }
}
