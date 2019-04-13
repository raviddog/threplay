using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace threplay
{
    public static class GameReplayDecoder
    {
        private static FileStream file;

        public static bool ReadFile(ref ReplayEntry replay)
        {
            bool status = false;
            if (replay.replay != null)
            {
                return true;
            }
            else
            {
                replay.replay = new ReplayEntry.ReplayInfo();
            }

            file = new FileStream(replay.FullPath, FileMode.Open);

            //read first 4 bytes
            int hexIn;
            String hex = string.Empty;

            for (int i = 0; i < 4; i++)
            {
                if ((hexIn = file.ReadByte()) != -1)
                {
                    hex = string.Concat(hex, string.Format("{0:X2}", hexIn));
                }
                else
                {
                    file.Close();
                    return false;
                }
            }

            switch (hex)
            {
                case "54365250":
                    //T6RP
                    status = Read_T6RP(ref replay.replay);
                    break;
                case "54375250":
                    //T7RP
                    //status = Read_T7RP(ref replay.replay);
                    break;
                case "54385250":
                    //T8RP
                    status = Read_T8RP(ref replay.replay);
                    break;
                case "54395250":
                    //T9RP
                    status = Read_T9RP(ref replay.replay);
                    break;
                case "74393572":
                    //t95r
                    status = Read_t95r(ref replay.replay);
                    break;
                case "74313072":
                    //t10r
                    status = Read_t10r(ref replay.replay);
                    break;
                case "74313172":
                    //t11r
                    status = Read_t11r(ref replay.replay);
                    break;
                case "74313272":
                    //t12r
                    status = Read_t12r(ref replay.replay);
                    break;
                case "74313235":
                    //t125
                    status = Read_t125(ref replay.replay);
                    break;
                case "31323872":
                    //128r
                    status = Read_128r(ref replay.replay);
                    break;
                case "74313372":
                    //t13r
                    //has both td and ddc for some fucking reason
                    //since im reading the user data at the end though it doesnt matter
                    status = Read_t13r(ref replay.replay);
                    break;
                case "74313433":
                    //t143
                    status = Read_t143(ref replay.replay);
                    break;
                case "74313572":
                    //t15r
                    status = Read_t15r(ref replay.replay);
                    break;
                case "74313672":
                    //t16r
                    status = Read_t16r(ref replay.replay);
                    break;
                case "74313536":
                    //t156
                    //shouldn't this be 165? gg zun
                    status = Read_t156(ref replay.replay);
                    break;
                default:
                    break;
            }

            file.Close();
            return status;
        }

        private static UInt32 ReadUInt32()
        {
            uint buf = new uint();
            UInt32 val = new UInt32();

            for (int i = 0; i < 4; i++)
            {
                buf = (uint)file.ReadByte();
                val += buf << (i * 8);
            }

            return val;
        }

        private static string ReadStringANSI()
        {
            int[] buf = new int[3];
            string val = string.Empty;
            buf[0] = file.ReadByte();
            buf[1] = file.ReadByte();
            if (buf[0] != 13 && buf[1] != 10)
            {
                buf[2] = file.ReadByte();
                do
                {
                    val = string.Concat(val, Convert.ToChar(buf[0]).ToString());
                    buf[0] = buf[1];
                    buf[1] = buf[2];
                    buf[2] = file.ReadByte();
                } while (buf[0] != 13 && buf[1] != 10);
            }

            file.Seek(-1, SeekOrigin.Current);

            return val;
        }

        //attempting to read japanese characters and failing
        private static string ReadStringWide()
        {
            int[] buf = new int[3];
            List<char> val = new List<char>();

            buf[0] = file.ReadByte();
            buf[1] = file.ReadByte();
            if (buf[0] != 13 && buf[1] != 10)
            {
                buf[2] = file.ReadByte();
                do
                {
                    val.Add((char)buf[0]);
                    buf[0] = buf[1];
                    buf[1] = buf[2];
                    buf[2] = file.ReadByte();
                } while (buf[0] != 13 && buf[1] != 10);
            }

            file.Seek(-1, SeekOrigin.Current);

            Encoding iso = Encoding.GetEncoding("ISO-8859-1");
            Encoding utf16 = Encoding.Unicode;

            byte[] valUTF16 = utf16.GetBytes(val.ToArray());
            byte[] valISO = Encoding.Convert(utf16, iso, valUTF16);
            return iso.GetString(valISO);
        }

        private static bool JumpToUser(int loc)
        {
            file.Seek(loc, SeekOrigin.Begin);
            UInt32 offset = ReadUInt32();
            try { file.Seek(offset, SeekOrigin.Begin); }
            catch { return false; }

            string val = string.Empty;
            int buf;
            for (int i = 0; i < 4; i++)
            {
                buf = file.ReadByte();
                val = string.Concat(val, string.Format("{0:X2}", buf));
            }
            return val == "55534552" ? true : false;
        }

        /*
         * Format of the Decoding Functions for Touhou 8 - onwards
         * 
         * Offset to USER info is stored at offset 0xC, length 4
         * Jump to offset, return false if exception thrown
         * Read USER to confirm correct jump, return false if not
         * 
         * Store the USER data length, it may come in handy?
         * Terminating bytes for a string are 0x0D, 0x0A
         * Jump to and read PLAYER NAME as string
         * Jump to and read DATE as string
         * Jump to and read CHARACTER as string, if the replay was encoded by a japanese copy of the game this reading will be gibberish
         *      - Implement a lookup table for each game to convert the hex into a readable string
         * Jump to and read SCORE as long, then format it to a string with thousand separators
         * Jump to and read DIFFICULTY as string
         * 
         * */

        private static bool Read_T6RP(ref ReplayEntry.ReplayInfo replay)
        {
            //lookup table
            string[] chars = new string[4] { "ReimuA", "ReimuB", "MarisaA", "MarisaB" };
            string[] difficulties = new string[5] { "Easy", "Normal", "Hard", "Lunatic", "Extra" };


            int[] buf = new int[2];
            file.Seek(2, SeekOrigin.Current);   //skip version number
            buf[0] = file.ReadByte();   //shot type
            replay.character = chars[buf[0]];
            buf[1] = file.ReadByte();   //difficulty
            replay.difficulty = difficulties[buf[1]];
            file.Seek(6, SeekOrigin.Current);   //skip checksum to encryption key
            byte key = (byte)file.ReadByte();

            byte[] buffer = new byte[65];
            for (int i = 0; i < 65; i++)
            {
                buffer[i] = (byte)file.ReadByte();
                buffer[i] -= key;
                key += 7;
            }

            for (int i = 1; i < 10; i++) //date[9], null terminated string
            {
                if (buffer[i] == 0x00) { i = 10; }
                else
                {
                    replay.date = string.Concat(replay.date, Convert.ToChar(buffer[i]).ToString());
                }
            }

            for (int i = 10; i < 19; i++)
            {
                if (buffer[i] == 0x00) { i = 19; }
                else
                {
                    replay.name = string.Concat(replay.name, Convert.ToChar(buffer[i]).ToString());
                }
            }

            uint score = new uint();
            for (int i = 21; i < 25; i++)
            {
                score += (uint)buffer[i] << ((i - 21) * 8);
            }
            replay.score = score.ToString("N0");

            return true;
        }

        private static bool Read_T7RP(ref ReplayEntry.ReplayInfo replay)
        {
            file.Seek(13, SeekOrigin.Begin);
            byte key = (byte)file.ReadByte();
            file.Seek(16, SeekOrigin.Begin);
            byte[] buffer = new byte[65];
            for (int i = 0; i < 65; i++)
            {
                buffer[i] = (byte)file.ReadByte();
                buffer[i] -= key;
                key += 7;
            }
            FileStream test = new FileStream("test.raw", FileMode.Create, FileAccess.Write);
            test.Write(buffer, 0, buffer.Length);


            return true;
        }

        private static bool Read_T8RP(ref ReplayEntry.ReplayInfo replay)
        {
            if (!JumpToUser(12)) return false;

            UInt32 length = ReadUInt32();
            file.Seek(17, SeekOrigin.Current);
            replay.name = ReadStringANSI();
            file.Seek(11, SeekOrigin.Current);
            replay.date = ReadStringANSI();
            file.Seek(9, SeekOrigin.Current);
            replay.character = ReadStringANSI();
            file.Seek(8, SeekOrigin.Current);
            long.TryParse(ReadStringANSI(), out long scoreConv);
            replay.score = scoreConv.ToString("N0");
            file.Seek(8, SeekOrigin.Current);
            replay.difficulty = ReadStringANSI();
            replay.stage = ReadStringANSI();

            //check if spell practice or game replay
            //actually do this later

            return true;
        }

        private static bool Read_T9RP(ref ReplayEntry.ReplayInfo replay)
        {
            if (!JumpToUser(12)) return false;

            UInt32 length = ReadUInt32();
            file.Seek(17, SeekOrigin.Current);
            replay.name = ReadStringANSI();
            file.Seek(11, SeekOrigin.Current);
            replay.date = ReadStringANSI();
            file.Seek(8, SeekOrigin.Current);
            replay.difficulty = ReadStringANSI();
            file.Seek(8, SeekOrigin.Current);
            replay.stage = ReadStringANSI();
            return true;
        }

        private static bool Read_t95r(ref ReplayEntry.ReplayInfo replay)
        {
            if (!JumpToUser(12)) return false;

            UInt32 length = ReadUInt32();
            file.Seek(4, SeekOrigin.Current);
            ReadStringANSI();
            ReadStringANSI();
            file.Seek(5, SeekOrigin.Current);
            replay.name = ReadStringANSI();
            replay.stage = ReadStringANSI() + " " + ReadStringANSI();
            file.Seek(5, SeekOrigin.Current);
            replay.date = ReadStringANSI();
            file.Seek(6, SeekOrigin.Current);
            long.TryParse(ReadStringANSI(), out long scoreConv);
            replay.score = scoreConv.ToString("N0");

            return true;
        }

        //  the replay user data format for touhou 10 through to 16 (and presumably from here on in) is identical

        private static bool Read_t10r(ref ReplayEntry.ReplayInfo replay)
        {
            if (!JumpToUser(12)) return false;

            UInt32 length = ReadUInt32();
            file.Seek(4, SeekOrigin.Current);
            ReadStringANSI();   //SJIS, 東方XYZ リプレイファイル情報, Touhou XYZ replay file info
            ReadStringANSI();   //Skip over game version info
            file.Seek(5, SeekOrigin.Current);
            replay.name = ReadStringANSI();
            file.Seek(5, SeekOrigin.Current);
            replay.date = ReadStringANSI();
            file.Seek(6, SeekOrigin.Current);
            replay.character = ReadStringANSI();
            file.Seek(5, SeekOrigin.Current);
            replay.difficulty = ReadStringANSI();
            // file.Seek(6, SeekOrigin.Current);
            replay.stage = ReadStringANSI();   //stage
            file.Seek(6, SeekOrigin.Current);
            long.TryParse(ReadStringANSI() + "0", out long scoreConv);  //replay stores the value without the 0
            replay.score = scoreConv.ToString("N0");

            return true;
        }

        private static bool Read_t11r(ref ReplayEntry.ReplayInfo replay)
        {
            return Read_t10r(ref replay);
        }

        private static bool Read_t12r(ref ReplayEntry.ReplayInfo replay)
        {
            return Read_t10r(ref replay);
        }

        private static bool Read_t125(ref ReplayEntry.ReplayInfo replay)
        {
            if (!JumpToUser(12)) return false;

            UInt32 length = ReadUInt32();
            file.Seek(4, SeekOrigin.Current);
            ReadStringANSI();
            ReadStringANSI();
            file.Seek(5, SeekOrigin.Current);
            replay.name = ReadStringANSI();
            file.Seek(5, SeekOrigin.Current);
            replay.date = ReadStringANSI();
            file.Seek(6, SeekOrigin.Current);
            replay.character = ReadStringANSI();
            replay.stage = ReadStringANSI();
            file.Seek(6, SeekOrigin.Current);
            long.TryParse(ReadStringANSI(), out long scoreConv);
            replay.score = scoreConv.ToString("N0");

            return true;
        }

        private static bool Read_128r(ref ReplayEntry.ReplayInfo replay)
        {
            if (!JumpToUser(12)) return false;

            UInt32 length = ReadUInt32();
            file.Seek(4, SeekOrigin.Current);
            ReadStringANSI();
            ReadStringANSI();
            file.Seek(5, SeekOrigin.Current);
            replay.name = ReadStringANSI();
            file.Seek(5, SeekOrigin.Current);
            replay.date = ReadStringANSI();
            file.Seek(6, SeekOrigin.Current);
            replay.stage = ReadStringANSI();
            file.Seek(5, SeekOrigin.Current);
            replay.difficulty = ReadStringANSI();
            file.Seek(6, SeekOrigin.Current);
            ReadStringANSI();   //stage
            file.Seek(6, SeekOrigin.Current);
            long.TryParse(ReadStringANSI() + "0", out long scoreConv);  //replay stores the value without the 0
            replay.score = scoreConv.ToString("N0");

            return true;
        }

        private static bool Read_t13r(ref ReplayEntry.ReplayInfo replay)
        {
            return Read_t10r(ref replay);
        }

        private static bool Read_t14r(ref ReplayEntry.ReplayInfo replay)
        {
            return Read_t10r(ref replay);
        }

        private static bool Read_t143(ref ReplayEntry.ReplayInfo replay)
        {
            if (!JumpToUser(12)) return false;

            UInt32 length = ReadUInt32();
            file.Seek(4, SeekOrigin.Current);
            ReadStringANSI();
            ReadStringANSI();
            file.Seek(5, SeekOrigin.Current);
            replay.name = ReadStringANSI();
            file.Seek(5, SeekOrigin.Current);
            replay.date = ReadStringANSI();
            replay.stage = ReadStringANSI() + " " + ReadStringANSI();
            file.Seek(6, SeekOrigin.Current);
            long.TryParse(ReadStringANSI() + "0", out long scoreConv);  //replay stores the value without the 0
            replay.score = scoreConv.ToString("N0");

            return true;
        }

        private static bool Read_t15r(ref ReplayEntry.ReplayInfo replay)
        {
            return Read_t10r(ref replay);
        }

        private static bool Read_t156(ref ReplayEntry.ReplayInfo replay)
        {
            return Read_t143(ref replay);
        }

        private static bool Read_t16r(ref ReplayEntry.ReplayInfo replay)
        {
            return Read_t10r(ref replay);
        }
    }
}
