using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pricom_IoT.Libs
{
    public struct WavHeader
    {
        public byte[] riffID;
        public uint size;
        public byte[] wavID;
        public byte[] fmtID;
        public uint fmtSize;
        public ushort format;
        public ushort channels;
        public uint sampleRate;
        public uint bytePerSec;
        public ushort blockSize;
        public ushort bit;
        public byte[] dataID;
        public uint dataSize;
    }

    public static class WavController
    {

        public static WavHeader OpenWavFile(string File, out double[] left, out double[] right)
        {
            WavHeader Header = new WavHeader();
            using (FileStream fs = new FileStream(File, FileMode.Open, FileAccess.Read))
            using (BinaryReader br = new BinaryReader(fs))
            {
                Header.riffID = br.ReadBytes(4);
                Header.size = br.ReadUInt32();
                Header.wavID = br.ReadBytes(4);
                br.ReadBytes(36); // junk
                Header.fmtID = br.ReadBytes(4);
                Header.fmtSize = br.ReadUInt32();
                Header.format = br.ReadUInt16();
                Header.channels = br.ReadUInt16();
                Header.sampleRate = br.ReadUInt32();
                Header.bytePerSec = br.ReadUInt32();
                Header.blockSize = br.ReadUInt16();
                Header.bit = br.ReadUInt16();
                br.ReadBytes(2); // junk
                Header.dataID = br.ReadBytes(4);
                Header.dataSize = br.ReadUInt32();
                left = new double[Header.dataSize / Header.blockSize];
                right = new double[left.Length];
                short toDouble;
                for (int i = 0; i < Header.dataSize / Header.blockSize; i++)
                {
                    toDouble = br.ReadInt16();
                    left[i] = toDouble / 32768.0;

                    toDouble = br.ReadInt16();
                    right[i] = toDouble / 32768.0;
                }
            }
            return Header;
        }

        public static void SaveWavFile(string File, double[] dpcmLeft,double[] dpcmRight, ushort BitDepth, WavHeader Header)
        {
            using (FileStream fs = new FileStream(File, FileMode.Create, FileAccess.Write))
            using (BinaryWriter br = new BinaryWriter(fs))
            {
                br.Write(Header.riffID);//Header.riffID = br.ReadBytes(4);
                br.Write((uint)(36 + dpcmLeft.Length * 2 * BitDepth / 8));//Header.size = br.ReadUInt32();
                br.Write(Header.wavID);//Header.wavID = br.ReadBytes(4);
                br.Write(Header.fmtID);// Header.fmtID = br.ReadBytes(4);
                br.Write(16);// Header.fmtSize = br.ReadUInt32();
                br.Write(Header.format);// Header.format = br.ReadUInt16();
                br.Write((ushort)2);// Header.channels = br.ReadUInt16();
                br.Write(Header.sampleRate);// Header.sampleRate = br.ReadUInt32();
                br.Write(Header.sampleRate * 2 * BitDepth / 8); //Header.bytePerSec = br.ReadUInt32();
                br.Write((ushort)(2 * BitDepth / 8)); //Header.blockSize = br.ReadUInt16();
                br.Write(BitDepth);// Header.bit = br.ReadUInt16();
                br.Write(Header.dataID);//Header.dataID = br.ReadBytes(4);
                br.Write(dpcmLeft.Length * 2 * BitDepth / 8);// Header.dataSize = br.ReadUInt32();
                //left = new double[Header.dataSize / Header.blockSize];
                //right = new double[left.Length];
                for (int i = 0; i < dpcmLeft.Length; i++)
                {
                    if (BitDepth == 8)
                    {
                        br.Write((byte)(dpcmLeft[i] * 128 + 128));//toDouble = br.ReadInt16();
                        br.Write((byte)(dpcmRight[i] * 128 + 128));
                    }
                    else
                    {
                        br.Write((ushort)(dpcmLeft[i] * 32768));
                        br.Write((ushort)(dpcmRight[i] * 32768));
                    }
                    //left[i] = toDouble / 32768.0;

                    //toDouble = br.ReadInt16();
                    //right[i] = toDouble / 32768.0;
                }
            }
        }

    }
}
