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

            try
            {
                file = new FileStream(replay.FullPath, FileMode.Open);
            } catch
            {
                return false;
            }

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
                    replay.replay.game = 0;
                    status = Read_T6RP(ref replay.replay);
                    break;
                case "54375250":
                    //T7RP
                    replay.replay.game = 1;
                    status = Read_T7RP(ref replay.replay);
                    break;
                case "54385250":
                    //T8RP
                    replay.replay.game = 2;
                    status = Read_T8RP(ref replay.replay);
                    break;
                case "54395250":
                    //T9RP
                    replay.replay.game = 3;
                    status = Read_T9RP(ref replay.replay);
                    break;
                case "74393572":
                    //t95r
                    replay.replay.game = 4;
                    status = Read_t95r(ref replay.replay);
                    break;
                case "74313072":
                    //t10r
                    replay.replay.game = 5;
                    status = Read_t10r(ref replay.replay);
                    break;
                case "74313172":
                    //t11r
                    replay.replay.game = 6;
                    status = Read_t11r(ref replay.replay);
                    break;
                case "74313272":
                    //t12r
                    replay.replay.game = 7;
                    status = Read_t12r(ref replay.replay);
                    break;
                case "74313235":
                    //t125
                    replay.replay.game = 8;
                    status = Read_t125(ref replay.replay);
                    break;
                case "31323872":
                    //128r
                    replay.replay.game = 9;
                    status = Read_128r(ref replay.replay);
                    break;
                case "74313372":
                    //t13r
                    //has both td and ddc for some fucking reason
                    //since im reading the user data at the end though it doesnt matter
                    //  replay game = either 10 or 11
                    status = Read_t13r(ref replay.replay);
                    break;
                case "74313433":
                    //t143
                    replay.replay.game = 12;
                    status = Read_t143(ref replay.replay);
                    break;
                case "74313572":
                    //t15r
                    replay.replay.game = 13;
                    status = Read_t15r(ref replay.replay);
                    break;
                case "74313672":
                    //t16r
                    replay.replay.game = 14;
                    status = Read_t16r(ref replay.replay);
                    break;
                case "74313536":
                    //t156
                    //shouldn't this be 165? gg zun
                    replay.replay.game = 15;
                    status = Read_t156(ref replay.replay);
                    break;
                case "74313772":
                    replay.replay.game = 16;
                    status = Read_t17r(ref replay.replay);
                    break;
                case "74313872":
                    replay.replay.game = 17;
                    status = Read_t18r(ref replay.replay);
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

        private static uint Read_BufferedUint(ref byte[] buffer, uint offset)
        {
            uint result = buffer[offset];
            result += (uint)buffer[offset + 1] << 8;
            result += (uint)buffer[offset + 2] << 16;
            result += (uint)buffer[offset + 3] << 24;
            return result;

        }

        private static uint get_bit(ref byte[] buffer, ref uint pointer, ref byte filter, byte length)
        {
            //function rewritten in C# from https://github.com/Fluorohydride/threp/blob/master/common.cpp
            uint result = 0;
            byte current = buffer[pointer];
            for(byte i = 0; i < length; ++i) {
                result <<= 1;
                if((current & filter) != 0x00) {
                    result |= 0x01;
                }
                filter >>= 1;
                if(filter == 0) {
                    pointer++;
                    current = buffer[pointer];
                    filter = 0x80;
                }
            }
            return result;
        }

        private static uint decompress(ref byte[] buffer, ref byte[] decode, uint length)
        {
            //function rewritten in C# from https://github.com/Fluorohydride/threp/blob/master/common.cpp
            uint pointer = 0, dest = 0, index, bits;
            byte filter = 0x80;
            byte[] dict = new byte[8208];   //0x2010
            while(pointer < length) {
                bits = get_bit(ref buffer, ref pointer, ref filter, 1);
                if(pointer >= length)
                    return dest;
                if(bits != 0) {
                    bits = get_bit(ref buffer, ref pointer, ref filter, 8);
                    if(pointer >= length)
                        return dest;
                    decode[dest] = (byte)bits;
                    dict[dest & 0x1fff] = (byte)bits;
                    dest++;
                } else {
                    bits = get_bit(ref buffer, ref pointer, ref filter, 13);
                    if(pointer >= length)
                        return dest;
                    index = bits - 1;
                    bits = get_bit(ref buffer, ref pointer, ref filter, 4);
                    if(pointer >= length)
                        return dest;
                    bits += 3;
                    for(int i = 0; i < bits; i++) {
                        dict[dest & 0x1fff] = dict[index + i];
                        decode[dest] = dict[index + i];
                        dest++;
                    }
                }
            }
            return dest;
        }

        private static void decode(ref byte[] buffer, int length, int block_size, byte _base, byte add)
        {
            //function rewritten in C# from https://github.com/Fluorohydride/threp/blob/master/common.cpp
            byte[] tbuf = new byte[length];
            Array.Copy(buffer, tbuf, length);
            int i, p = 0, tp1, tp2, hf, left = length;
            if((left % block_size) < (block_size / 4))
                left -= left % block_size;
            left -= length & 1;
            while(left != 0) {
                if(left < block_size)
                    block_size = left;
                tp1 = p + block_size - 1;
                tp2 = p + block_size - 2;
                hf = (block_size + (block_size & 0x1)) / 2;
                for(i = 0; i < hf; ++i, ++p) {
                    buffer[tp1] = (byte)(tbuf[p] ^ _base);
                    _base += add;
                    tp1 -= 2;
                }
                hf = block_size / 2;
                for(i = 0; i < hf; ++i, ++p) {
                    buffer[tp2] = (byte)(tbuf[p] ^ _base);
                    _base += add;
                    tp2 -= 2;
                }
                left -= block_size;
            }
        }

        private static bool Read_Userdata(ref ReplayEntry.ReplayInfo replay)
        {
            if(!JumpToUser(12))
                return false;

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

            byte[] buffer = new byte[file.Length];
            file.Seek(0, SeekOrigin.Begin);
            file.Read(buffer, 0, (int)file.Length);


            replay.character = chars[buffer[0x06]];
            replay.difficulty = difficulties[buffer[0x07]];
            byte key = buffer[0x0e];
            
            for (int i = 0x0f; i < buffer.Length; i++)
            {
                buffer[i] -= key;
                key += 7;
            }

            for (int i = 0x10; buffer[i] != 0x00; i++) //date[9], null terminated string
            {
                replay.date = string.Concat(replay.date, Convert.ToChar(buffer[i]).ToString());
            }

            for (int i = 0x19; buffer[i] != 0x00; i++)
            {
                replay.name = string.Concat(replay.name, Convert.ToChar(buffer[i]).ToString());
            }

            replay.score = Read_BufferedUint(ref buffer, 0x24).ToString("N0");

            uint[] score_offsets = new uint[7];
            uint max_stage = 0;
            for(uint i = 0; i < 7; i++) {
                score_offsets[i] = Read_BufferedUint(ref buffer, (uint)(0x34 + 4 * i));
                if(score_offsets[i] != 0x00) {
                    max_stage = i;
                }
            }

            if(max_stage == 6) {
                replay.splits = new ReplayEntry.ReplayInfo.ReplaySplits[1];
                uint offset = score_offsets[6];
                replay.splits[0] = new ReplayEntry.ReplayInfo.ReplaySplits();
                replay.splits[0].stage = 7;
                replay.splits[0].score = Read_BufferedUint(ref buffer, offset);
                replay.splits[0].power = buffer[offset + 0x8].ToString();
                replay.splits[0].lives = buffer[offset + 0x9].ToString();
                replay.splits[0].bombs = buffer[offset + 0xa].ToString();
                replay.splits[0].additional = "Rank: " + buffer[offset + 0xb];
            } else {
                max_stage += 1;
                replay.splits = new ReplayEntry.ReplayInfo.ReplaySplits[max_stage];
                for(uint i = 0; i < max_stage; i++) {
                    uint offset = score_offsets[i];
                    if(offset != 0x00) {
                        replay.splits[i] = new ReplayEntry.ReplayInfo.ReplaySplits();
                        replay.splits[i].stage = i + 1;
                        replay.splits[i].score = Read_BufferedUint(ref buffer, offset);
                        replay.splits[i].power = buffer[offset + 0x8].ToString();
                        replay.splits[i].lives = buffer[offset + 0x9].ToString();
                        replay.splits[i].bombs = buffer[offset + 0xa].ToString();
                        replay.splits[i].additional = "Rank: " + buffer[offset + 0xb];
                    }
                }
            }



            return true;
        }

        private static bool Read_T7RP(ref ReplayEntry.ReplayInfo replay)
        {
            string[] chars = new string[] { "ReimuA", "ReimuB", "MarisaA", "MarisaB", "SakuyaA", "SakuyaB" };
            string[] difficulties = new string[] { "Easy", "Normal", "Hard", "Lunatic", "Extra", "Phantasm" };
            //raw data starts at 84
            byte[] buffer = new byte[file.Length];
            file.Seek(0, SeekOrigin.Begin);
            file.Read(buffer, 0, (int)file.Length);

            byte key = buffer[0x0d];
            for (int i = 16; i < buffer.Length; ++i)
            {
                buffer[i] -= key;
                key += 7;
            }
            uint length = 0, dlength = 0;
            for (int i = 20; i < 24; i++)
            {
                length += (uint)(buffer[i] << ((i - 20) * 8));
                dlength += (uint)(buffer[i + 4] << ((i - 24) * 8));
            }

            uint[] score_offsets = new uint[7];
            uint max_stage = 0;
            for(uint i = 0; i < 7; i++) {
                score_offsets[i] = Read_BufferedUint(ref buffer, (uint)(0x1c + 4 * i));
                if(score_offsets[i] != 0x00) {
                    max_stage = i;
                }
            }

            Array.ConstrainedCopy(buffer, 0x54, buffer, 0, buffer.Length - 0x54);

            byte[] decodeData = new byte[dlength];
            uint rlength = decompress(ref buffer, ref decodeData, length);

            replay.character = chars[decodeData[2]];
            replay.difficulty = difficulties[decodeData[3]];
            for (int i = 4; i < 9; i++)
            {
                replay.date = string.Concat(replay.date, (Convert.ToChar(decodeData[i])).ToString());
            }

            for (int i = 10; i < 18; i++)
            {
                replay.name = string.Concat(replay.name, (Convert.ToChar(decodeData[i])).ToString());
            }

            uint score = Read_BufferedUint(ref decodeData, 24);
            score *= 10;
            replay.score = score.ToString("N0");

            

            if(max_stage == 6) {
                replay.splits = new ReplayEntry.ReplayInfo.ReplaySplits[1];
                uint offset = score_offsets[6];
                replay.splits[0] = new ReplayEntry.ReplayInfo.ReplaySplits();
                replay.splits[0].stage = 7;
                replay.splits[0].score = Read_BufferedUint(ref decodeData, offset); //  stored as end of stage score in pcb
                replay.splits[0].piv = Read_BufferedUint(ref decodeData, offset + 0x8);// + "/" + Read_BufferedUint(ref decodeData, offset + 0xc);
                replay.splits[0].additional = "Point Items: " + Read_BufferedUint(ref decodeData, offset + 0x4) + " | CherryMAX: " + Read_BufferedUint(ref decodeData, offset + 0xc);
                replay.splits[0].graze = Read_BufferedUint(ref decodeData, offset + 0x14);
                replay.splits[0].power = decodeData[offset + 0x22].ToString();
                replay.splits[0].lives = decodeData[offset + 0x23].ToString();
                replay.splits[0].bombs = decodeData[offset + 0x24].ToString();
            } else {
                max_stage += 1;
                replay.splits = new ReplayEntry.ReplayInfo.ReplaySplits[max_stage];
                for(uint i = 0; i < max_stage; i++) {
                    if(score_offsets[i] != 0x00) {
                        replay.splits[i] = new ReplayEntry.ReplayInfo.ReplaySplits();
                        uint offset = score_offsets[i] - 0x54;
                        replay.splits[i].stage = i + 1;
                        replay.splits[i].score = Read_BufferedUint(ref decodeData, offset); //  stored as end of stage score in pcb
                        replay.splits[i].piv = Read_BufferedUint(ref decodeData, offset + 0x8);// + "/" + Read_BufferedUint(ref decodeData, offset + 0xc);
                        replay.splits[i].additional = "Point Items: " + Read_BufferedUint(ref decodeData, offset + 0x4) + " | CherryMAX: " + Read_BufferedUint(ref decodeData, offset + 0xc);
                        replay.splits[i].graze = Read_BufferedUint(ref decodeData, offset + 0x14);
                        replay.splits[i].power = decodeData[offset + 0x22].ToString();
                        replay.splits[i].lives = decodeData[offset + 0x23].ToString();
                        replay.splits[i].bombs = decodeData[offset + 0x24].ToString();
                    }
                }
            }

            return true;
        }

        private static bool Read_T8RP(ref ReplayEntry.ReplayInfo replay)
        {
            if (!JumpToUser(12)) return false;

            ReadUInt32();
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

            //-----------------------

            byte[] buffer = new byte[file.Length];
            file.Seek(0, SeekOrigin.Begin);
            file.Read(buffer, 0, (int)file.Length);

            byte key = buffer[0x15];
            uint length = Read_BufferedUint(ref buffer, 0x0c);
            for(int i = 24; i < buffer.Length; ++i) {
                buffer[i] -= key;
                key += 7;
            }
            uint dlength = Read_BufferedUint(ref buffer, 0x1c);

            uint[] score_offsets = new uint[9];
            uint max_stage = 0;
            for(uint i = 0; i < score_offsets.Length; i++) {
                score_offsets[i] = Read_BufferedUint(ref buffer, (uint)(0x20 + 4 * i));
                if(score_offsets[i] != 0x00) {
                    max_stage = i;
                }
            }

            Array.ConstrainedCopy(buffer, 0x68, buffer, 0, buffer.Length - 0x68);

            byte[] decodeData = new byte[dlength];
            uint rlength = decompress(ref buffer, ref decodeData, length - 0x68);



            if(max_stage == score_offsets.Length - 1) {
                replay.splits = new ReplayEntry.ReplayInfo.ReplaySplits[1];
                uint offset = score_offsets[score_offsets.Length - 1] - 0x68;
                replay.splits[0] = new ReplayEntry.ReplayInfo.ReplaySplits();
                replay.splits[0].stage = 7;
                replay.splits[0].score = Read_BufferedUint(ref decodeData, offset) * 10; //  stored as end of stage score in pcb
                replay.splits[0].additional = "Point Items: " + Read_BufferedUint(ref decodeData, offset + 0x4) + " | Time: " + Read_BufferedUint(ref decodeData, offset + 0xc);
                replay.splits[0].graze = Read_BufferedUint(ref decodeData, offset + 0x8);
                replay.splits[0].piv = Read_BufferedUint(ref decodeData, offset + 0x14);// + "/" + Read_BufferedUint(ref decodeData, offset + 0xc);
                replay.splits[0].power = decodeData[offset + 0x1c].ToString();
                replay.splits[0].lives = decodeData[offset + 0x1d].ToString();
                replay.splits[0].bombs = decodeData[offset + 0x1e].ToString();
            } else {
                max_stage += 1;
                replay.splits = new ReplayEntry.ReplayInfo.ReplaySplits[max_stage];
                for(uint i = 0; i < max_stage; i++) {
                    if(score_offsets[i] != 0x00) {
                        replay.splits[i] = new ReplayEntry.ReplayInfo.ReplaySplits();
                        uint offset = score_offsets[i] - 0x68;
                        replay.splits[i].score = Read_BufferedUint(ref decodeData, offset) * 10; //  stored as end of stage score in pcb
                        replay.splits[i].additional = "Point Items: " + Read_BufferedUint(ref decodeData, offset + 0x4) + " | Time: " + Read_BufferedUint(ref decodeData, offset + 0xc);
                        replay.splits[i].graze = Read_BufferedUint(ref decodeData, offset + 0x8);
                        replay.splits[i].piv = Read_BufferedUint(ref decodeData, offset + 0x14);// + "/" + Read_BufferedUint(ref decodeData, offset + 0xc);
                        replay.splits[i].power = decodeData[offset + 0x1c].ToString();
                        replay.splits[i].lives = decodeData[offset + 0x1d].ToString();
                        replay.splits[i].bombs = decodeData[offset + 0x1e].ToString();
                        
                        switch(i) {
                            case 3:
                                replay.splits[i].stage = 4;
                                replay.splits[i].additional += " | Stage: 4A";
                                break;
                            case 4:
                                replay.splits[i].stage = 4;
                                replay.splits[i].additional += " | Stage: 4B";
                                break;
                            case 5:
                                replay.splits[i].stage = 5;
                                break;
                            case 6:
                                replay.splits[i].stage = 6;
                                replay.splits[i].additional += " | Stage: 6A";
                                break;
                            case 7:
                                replay.splits[i].stage = 6;
                                replay.splits[i].additional += " | Stage: 6B";
                                break;
                            default:
                                replay.splits[i].stage = i + 1;
                                break;
                        }
                    }
                }
            }

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
            byte[] buffer = new byte[file.Length];
            file.Seek(0, SeekOrigin.Begin);
            file.Read(buffer, 0, (int)file.Length);

            uint length = Read_BufferedUint(ref buffer, 28);
            uint dlength = Read_BufferedUint(ref buffer, 32);

            byte[] decodedata = new byte[dlength];
            Array.Copy(buffer, 36, buffer, 0, buffer.Length - 36);
            decode(ref buffer, (int)length, 0x400, 0xaa, 0xe1);
            decode(ref buffer, (int)length, 0x80, 0x3d, 0x7a);
            decompress(ref buffer, ref decodedata, length);

            uint stageoffset = 0x64, stage = decodedata[0x4c];
            if(stage > 6) {
                stage = 6;
            }

            replay.splits = new ReplayEntry.ReplayInfo.ReplaySplits[stage];

            for(int i = 0; i < stage; ++i) {
                replay.splits[i] = new ReplayEntry.ReplayInfo.ReplaySplits();
                replay.splits[i].stage = decodedata[stageoffset];
                replay.splits[i].score = Read_BufferedUint(ref decodedata, stageoffset + 0xc) * 10;
                replay.splits[i].power = (0.05f * (float)Read_BufferedUint(ref decodedata, stageoffset + 0x10)).ToString("0.00");
                replay.splits[i].piv = Read_BufferedUint(ref decodedata, stageoffset + 0x14);
                uint lives = decodedata[stageoffset + 0x1c];

                replay.splits[i].lives = lives.ToString();
                replay.splits[i].graze = 0;
                replay.splits[i].bombs = "0";
                stageoffset += Read_BufferedUint(ref decodedata, stageoffset + 0x8) + 0x1c4;
            }


            return Read_Userdata(ref replay);
        }

        private static bool Read_t11r(ref ReplayEntry.ReplayInfo replay)
        {
            byte[] buffer = new byte[file.Length];
            file.Seek(0, SeekOrigin.Begin);
            file.Read(buffer, 0, (int)file.Length);

            uint length = Read_BufferedUint(ref buffer, 28);
            uint dlength = Read_BufferedUint(ref buffer, 32);

            byte[] decodedata = new byte[dlength];
            Array.Copy(buffer, 36, buffer, 0, buffer.Length - 36);
            decode(ref buffer, (int)length, 0x800, 0xaa, 0xe1);
            decode(ref buffer, (int)length, 0x40, 0x3d, 0x7a);
            decompress(ref buffer, ref decodedata, length);

            uint stageoffset = 0x70, stage = decodedata[0x58];
            if(stage > 6) {
                stage = 6;
            }

            replay.splits = new ReplayEntry.ReplayInfo.ReplaySplits[stage];

            for(int i = 0; i < stage; ++i) {
                replay.splits[i] = new ReplayEntry.ReplayInfo.ReplaySplits();
                replay.splits[i].stage = decodedata[stageoffset];
                replay.splits[i].score = Read_BufferedUint(ref decodedata, stageoffset + 0xc) * 10;
                replay.splits[i].power = (0.05f * (float)Read_BufferedUint(ref decodedata, stageoffset + 0x10)).ToString("0.00");
                replay.splits[i].piv = Read_BufferedUint(ref decodedata, stageoffset + 0x14);
                uint lives = decodedata[stageoffset + 0x18];
                uint pieces = decodedata[stageoffset + 0x1a];

                replay.splits[i].lives = lives.ToString() + " (" + pieces + "/5)";
                replay.splits[i].graze = Read_BufferedUint(ref decodedata, stageoffset + 0x34);
                replay.splits[i].bombs = "0";
                stageoffset += Read_BufferedUint(ref decodedata, stageoffset + 0x8) + 0x90;
            }


            return Read_Userdata(ref replay);
        }

        private static bool Read_t12r(ref ReplayEntry.ReplayInfo replay)
        {
            byte[] buffer = new byte[file.Length];
            file.Seek(0, SeekOrigin.Begin);
            file.Read(buffer, 0, (int)file.Length);

            uint length = Read_BufferedUint(ref buffer, 28);
            uint dlength = Read_BufferedUint(ref buffer, 32);

            byte[] decodedata = new byte[dlength];
            Array.Copy(buffer, 36, buffer, 0, buffer.Length - 36);
            decode(ref buffer, (int)length, 0x800, 0x5e, 0xe1);
            decode(ref buffer, (int)length, 0x40, 0x7d, 0x3a);
            decompress(ref buffer, ref decodedata, length);

            uint stageoffset = 0x70, stage = decodedata[0x58];
            if(stage > 6) {
                stage = 6;
            }

            replay.splits = new ReplayEntry.ReplayInfo.ReplaySplits[stage];

            for(int i = 0; i < stage; ++i) {
                replay.splits[i] = new ReplayEntry.ReplayInfo.ReplaySplits();
                replay.splits[i].stage = decodedata[stageoffset];
                replay.splits[i].score = Read_BufferedUint(ref decodedata, stageoffset + 0xc) * 10;
                replay.splits[i].power = ((float)Read_BufferedUint(ref decodedata, stageoffset + 0x10) / 100f).ToString("0.00");
                replay.splits[i].piv = (Read_BufferedUint(ref decodedata, stageoffset + 0x14) / 1000) * 10;
                uint lives = decodedata[stageoffset + 0x18];
                uint lpieces = decodedata[stageoffset + 0x1a];
                if(lpieces > 0) {
                    lpieces -= 1;
                }
                uint bombs = decodedata[stageoffset + 0x1c];
                uint bpieces = decodedata[stageoffset + 0x1e];

                replay.splits[i].additional = "UFOs: ";
                for(int j = 0; j < 3; ++j) {
                    switch(decodedata[stageoffset + 0x20 + j * 4]) {
                        case 0:
                            replay.splits[i].additional += "None ";
                            break;
                        case 1:
                            replay.splits[i].additional += "Red ";
                            break;
                        case 2:
                            replay.splits[i].additional += "Blue ";
                            break;
                        case 3:
                            replay.splits[i].additional += "Green ";
                            break;
                    }
                }

                replay.splits[i].lives = lives.ToString() + " (" + lpieces + "/4)";
                replay.splits[i].bombs = bombs.ToString() + " (" + bpieces + "/3)";
                replay.splits[i].graze = Read_BufferedUint(ref decodedata, stageoffset + 0x44);
                stageoffset += Read_BufferedUint(ref decodedata, stageoffset + 0x8) + 0xa0;
            }

            return Read_Userdata(ref replay);
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
            if (!JumpToUser(12)) return false;

            //UInt32 length = ReadUInt32();
            ReadUInt32();
            file.Seek(4, SeekOrigin.Current);
            file.Seek(4, SeekOrigin.Current);   //which game

            byte ver = (byte)file.ReadByte();
            if (ver == 144) replay.game = 10;
            else replay.game = 11;

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

            if(replay.game == 10) {
                //  td decode
                byte[] buffer = new byte[file.Length];
                file.Seek(0, SeekOrigin.Begin);
                file.Read(buffer, 0, (int)file.Length);

                uint length = Read_BufferedUint(ref buffer, 28);
                uint dlength = Read_BufferedUint(ref buffer, 32);

                byte[] decodedata = new byte[dlength];
                Array.Copy(buffer, 36, buffer, 0, buffer.Length - 36);
                decode(ref buffer, (int)length, 0x400, 0x5c, 0xe1);
                decode(ref buffer, (int)length, 0x100, 0x7d, 0x3a);
                decompress(ref buffer, ref decodedata, length);

                uint stageoffset = 0x74, stage = decodedata[0x58];
                if(stage > 6) {
                    stage = 6;
                }

                replay.splits = new ReplayEntry.ReplayInfo.ReplaySplits[stage];

                for(int i = 0; i < stage; ++i) {
                    replay.splits[i] = new ReplayEntry.ReplayInfo.ReplaySplits();
                    replay.splits[i].stage = decodedata[stageoffset];
                    replay.splits[i].score = Read_BufferedUint(ref decodedata, stageoffset + 0x1c) * 10;
                    replay.splits[i].power = ((float)Read_BufferedUint(ref decodedata, stageoffset + 0x44) / 100f).ToString("0.00");
                    replay.splits[i].piv = (Read_BufferedUint(ref decodedata, stageoffset + 0x38) / 1000) *10;
                    uint lives = decodedata[stageoffset + 0x50];
                    uint lpieces = decodedata[stageoffset + 0x54];
                    uint bombs = decodedata[stageoffset + 0x5c];
                    uint bpieces = decodedata[stageoffset + 0x60];

                    replay.splits[i].additional = "Trance: " + decodedata[stageoffset + 0x64] + "/600";
                    replay.splits[i].lives = lives.ToString() + " (" + lpieces + ")";   //  i dont have the piece table rn
                    replay.splits[i].graze = Read_BufferedUint(ref decodedata, stageoffset + 0x2c);
                    replay.splits[i].bombs = bombs.ToString() + " (" + bpieces + "/8)";
                    stageoffset += Read_BufferedUint(ref decodedata, stageoffset + 0x8) + 0xc4;
                }
            } else {
                //  ddc decode
                byte[] buffer = new byte[file.Length];
                file.Seek(0, SeekOrigin.Begin);
                file.Read(buffer, 0, (int)file.Length);

                uint length = Read_BufferedUint(ref buffer, 28);
                uint dlength = Read_BufferedUint(ref buffer, 32);

                byte[] decodedata = new byte[dlength];
                Array.Copy(buffer, 36, buffer, 0, buffer.Length - 36);
                decode(ref buffer, (int)length, 0x400, 0x5c, 0xe1);
                decode(ref buffer, (int)length, 0x100, 0x7d, 0x3a);
                decompress(ref buffer, ref decodedata, length);

                uint stageoffset = 0x94, stage = decodedata[0x78];
                if(stage > 6) {
                    stage = 6;
                }

                replay.splits = new ReplayEntry.ReplayInfo.ReplaySplits[stage];

                for(int i = 0; i < stage; ++i) {
                    replay.splits[i] = new ReplayEntry.ReplayInfo.ReplaySplits();
                    replay.splits[i].stage = decodedata[stageoffset];
                    replay.splits[i].score = Read_BufferedUint(ref decodedata, stageoffset + 0x1c) * 10;
                    replay.splits[i].power = ((float)Read_BufferedUint(ref decodedata, stageoffset + 0x44) / 100f).ToString("0.00");
                    replay.splits[i].piv = (Read_BufferedUint(ref decodedata, stageoffset + 0x38) / 1000) * 10;
                    uint lives = decodedata[stageoffset + 0x50];
                    uint lpieces = decodedata[stageoffset + 0x54];
                    uint bombs = decodedata[stageoffset + 0x5c];
                    uint bpieces = decodedata[stageoffset + 0x60];

                    //replay.splits[i].additional = "Lives gained: " + decodedata[stageoffset + 0x58];
                    replay.splits[i].lives = lives.ToString() + " (" + lpieces + "/3)";   //  i dont have the piece table rn
                    replay.splits[i].graze = Read_BufferedUint(ref decodedata, stageoffset + 0x2c);
                    replay.splits[i].bombs = bombs.ToString() + " (" + bpieces + "/8)";
                    stageoffset += Read_BufferedUint(ref decodedata, stageoffset + 0x8) + 0xdc;
                }
            }

            return Read_Userdata(ref replay);
        }

        private static bool Read_t14r(ref ReplayEntry.ReplayInfo replay)
        {
            byte[] buffer = new byte[file.Length];
            file.Seek(0, SeekOrigin.Begin);
            file.Read(buffer, 0, (int)file.Length);

            uint length = Read_BufferedUint(ref buffer, 28);
            uint dlength = Read_BufferedUint(ref buffer, 32);

            byte[] decodedata = new byte[dlength];
            Array.Copy(buffer, 36, buffer, 0, buffer.Length - 36);
            decode(ref buffer, (int)length, 0x400, 0x5c, 0xe1);
            decode(ref buffer, (int)length, 0x100, 0x7d, 0x3a);
            decompress(ref buffer, ref decodedata, length);

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
            //  td decode
            byte[] buffer = new byte[file.Length];
            file.Seek(0, SeekOrigin.Begin);
            file.Read(buffer, 0, (int)file.Length);

            uint length = Read_BufferedUint(ref buffer, 28);
            uint dlength = Read_BufferedUint(ref buffer, 32);

            byte[] decodedata = new byte[dlength];
            Array.Copy(buffer, 36, buffer, 0, buffer.Length - 36);
            decode(ref buffer, (int)length, 0x400, 0x5c, 0xe1);
            decode(ref buffer, (int)length, 0x100, 0x7d, 0x3a);
            decompress(ref buffer, ref decodedata, length);

            uint stageoffset = 0xa4, stage = decodedata[0x88];
            if(stage > 6) {
                stage = 6;
            }

            replay.splits = new ReplayEntry.ReplayInfo.ReplaySplits[stage];

            for(int i = 0; i < stage; ++i) {
                replay.splits[i] = new ReplayEntry.ReplayInfo.ReplaySplits();
                replay.splits[i].stage = decodedata[stageoffset];
                replay.splits[i].score = Read_BufferedUint(ref decodedata, stageoffset + 0x30) * 10;
                replay.splits[i].power = ((float)Read_BufferedUint(ref decodedata, stageoffset + 0x64) / 100f).ToString("0.00");
                replay.splits[i].piv = (Read_BufferedUint(ref decodedata, stageoffset + 0x58) / 1000) * 10;
                uint lives = decodedata[stageoffset + 0x74];
                uint lpieces = decodedata[stageoffset + 0x78];
                uint bombs = decodedata[stageoffset + 0x80];
                uint bpieces = decodedata[stageoffset + 0x84];

                replay.splits[i].additional = "";
                replay.splits[i].lives = lives.ToString() + " (" + lpieces + "/3)";   //  i dont have the piece table rn
                replay.splits[i].graze = Read_BufferedUint(ref decodedata, stageoffset + 0x40);
                replay.splits[i].bombs = bombs.ToString() + " (" + bpieces + "/8)";
                stageoffset += Read_BufferedUint(ref decodedata, stageoffset + 0x8) + 0x238;
            }
            return Read_Userdata(ref replay);
        }

        private static bool Read_t156(ref ReplayEntry.ReplayInfo replay)
        {
            return Read_t143(ref replay);
        }

        private static bool Read_t16r(ref ReplayEntry.ReplayInfo replay)
        {
            //  td decode
            byte[] buffer = new byte[file.Length];
            file.Seek(0, SeekOrigin.Begin);
            file.Read(buffer, 0, (int)file.Length);

            uint length = Read_BufferedUint(ref buffer, 28);
            uint dlength = Read_BufferedUint(ref buffer, 32);

            byte[] decodedata = new byte[dlength];
            Array.Copy(buffer, 36, buffer, 0, buffer.Length - 36);
            decode(ref buffer, (int)length, 0x400, 0x5c, 0xe1);
            decode(ref buffer, (int)length, 0x100, 0x7d, 0x3a);
            decompress(ref buffer, ref decodedata, length);

            uint stageoffset = 0xa0, stage = decodedata[0x80];
            if(stage > 6) {
                stage = 6;
            }

            replay.splits = new ReplayEntry.ReplayInfo.ReplaySplits[stage];

            for(int i = 0; i < stage; ++i) {
                replay.splits[i] = new ReplayEntry.ReplayInfo.ReplaySplits();
                replay.splits[i].stage = decodedata[stageoffset];
                replay.splits[i].score = Read_BufferedUint(ref decodedata, stageoffset + 0x34) * 10;
                replay.splits[i].power = ((float)Read_BufferedUint(ref decodedata, stageoffset + 0x68) / 100f).ToString("0.00");
                replay.splits[i].piv = (Read_BufferedUint(ref decodedata, stageoffset + 0x5c) / 1000) * 10;
                uint lives = decodedata[stageoffset + 0x78];
                uint lpieces = decodedata[stageoffset + 0x7c];
                uint bombs = decodedata[stageoffset + 0x84];
                uint bpieces = decodedata[stageoffset + 0x88];

                replay.splits[i].additional = "Season: " + Read_BufferedUint(ref decodedata, stageoffset + 0x8c) + "/" + Read_BufferedUint(ref decodedata, stageoffset + 0x90);
                replay.splits[i].lives = lives.ToString();   //  i dont have the piece table rn
                replay.splits[i].graze = Read_BufferedUint(ref decodedata, stageoffset + 0x44);
                replay.splits[i].bombs = bombs.ToString() + " (" + bpieces + "/5)";
                stageoffset += Read_BufferedUint(ref decodedata, stageoffset + 0x8) + 0x294;
            }
            return Read_Userdata(ref replay);
        }

        private static bool Read_t17r(ref ReplayEntry.ReplayInfo replay)
        {
            //  td decode
            byte[] buffer = new byte[file.Length];
            file.Seek(0, SeekOrigin.Begin);
            file.Read(buffer, 0, (int)file.Length);

            uint length = Read_BufferedUint(ref buffer, 28);
            uint dlength = Read_BufferedUint(ref buffer, 32);

            byte[] decodedata = new byte[dlength];
            Array.Copy(buffer, 36, buffer, 0, buffer.Length - 36);
            decode(ref buffer, (int)length, 0x400, 0x5c, 0xe1);
            decode(ref buffer, (int)length, 0x100, 0x7d, 0x3a);
            decompress(ref buffer, ref decodedata, length);

            uint stageoffset = 0xa0, stage = decodedata[0x84];
            if(stage > 6) {
                stage = 6;
            }

            replay.splits = new ReplayEntry.ReplayInfo.ReplaySplits[stage];

            for(int i = 0; i < stage; ++i) {
                replay.splits[i] = new ReplayEntry.ReplayInfo.ReplaySplits();
                replay.splits[i].stage = decodedata[stageoffset];
                replay.splits[i].score = Read_BufferedUint(ref decodedata, stageoffset + 0x34) * 10;
                replay.splits[i].power = ((float)Read_BufferedUint(ref decodedata, stageoffset + 0x68) / 100f).ToString("0.00");
                replay.splits[i].piv = (Read_BufferedUint(ref decodedata, stageoffset + 0x5c) / 1000) * 10;
                uint lives = decodedata[stageoffset + 0x78];
                uint lpieces = decodedata[stageoffset + 0x7c];
                uint bombs = decodedata[stageoffset + 0x84];
                uint bpieces = decodedata[stageoffset + 0x88];

                //  token stuff starts at 0x9c
                //  hyper might be c8 or dc

                replay.splits[i].additional = "";
                replay.splits[i].lives = lives.ToString() + " (" + lpieces + "/3)";   //  i dont have the piece table rn
                replay.splits[i].graze = Read_BufferedUint(ref decodedata, stageoffset + 0x44);
                replay.splits[i].bombs = bombs.ToString() + " (" + bpieces + "/3)";
                stageoffset += Read_BufferedUint(ref decodedata, stageoffset + 0x8) + 0x158;
            }
            return Read_Userdata(ref replay);
        }

        private static bool Read_t18r(ref ReplayEntry.ReplayInfo replay)
        {
            //  td decode
            byte[] buffer = new byte[file.Length];
            file.Seek(0, SeekOrigin.Begin);
            file.Read(buffer, 0, (int)file.Length);

            uint length = Read_BufferedUint(ref buffer, 28);
            uint dlength = Read_BufferedUint(ref buffer, 32);

            byte[] decodedata = new byte[dlength];
            Array.Copy(buffer, 36, buffer, 0, buffer.Length - 36);
            decode(ref buffer, (int)length, 0x400, 0x5c, 0xe1);
            decode(ref buffer, (int)length, 0x100, 0x7d, 0x3a);
            decompress(ref buffer, ref decodedata, length);

            uint stageoffset = 0xc8, stage = decodedata[0xa8];
            if(stage > 6) {
                stage = 6;
            }

            replay.splits = new ReplayEntry.ReplayInfo.ReplaySplits[stage];

            for(int i = 0; i < stage; ++i) {
                replay.splits[i] = new ReplayEntry.ReplayInfo.ReplaySplits();
                replay.splits[i].stage = decodedata[stageoffset];
                replay.splits[i].score = Read_BufferedUint(ref decodedata, stageoffset + 0x88) * 10;
                replay.splits[i].power = ((float)Read_BufferedUint(ref decodedata, stageoffset + 0xc4) / 100f).ToString("0.00");
                replay.splits[i].piv = Read_BufferedUint(ref decodedata, stageoffset + 0xbc);
                uint lives = decodedata[stageoffset + 0xd4];
                uint lpieces = decodedata[stageoffset + 0xd8];
                uint bombs = decodedata[stageoffset + 0xe4];
                uint bpieces = decodedata[stageoffset + 0xe8];

                replay.splits[i].additional = "";
                replay.splits[i].lives = lives.ToString() + " (" + lpieces + "/3)";   //  i dont have the piece table rn
                replay.splits[i].graze = 0;
                replay.splits[i].bombs = bombs.ToString() + " (" + bpieces + "/3)";
                stageoffset += Read_BufferedUint(ref decodedata, stageoffset + 0x8) + 0x126c;
            }
            return Read_Userdata(ref replay);
        }
    }

    public class ReplayEntry
    {
        public string Filename { get; set; }
        public string Filesize { get; set; }
        public string Date { get; set; }
        public string FullPath;
        public string ReplayName { get { return replay.name; } }
        public string ReplayDifficulty { get { return replay.difficulty; } }
        public string ReplayCharacter { get { return replay.character; } }
        public string ReplayStage { get { return replay.stage; } }
        public ReplayInfo replay;

        public class ReplayInfo
        {
            public int game;
            public string name;
            public string date;
            public string character;
            public string difficulty;
            public string stage;
            public string score;
            public ReplaySplits[] splits;
            
            public class ReplaySplits
            {
                public uint stage;
                public uint score;
                public string power;
                public uint piv;
                public string lives;
                public string bombs;
                public string additional;
                public uint graze;
            }
        }
    }
}
