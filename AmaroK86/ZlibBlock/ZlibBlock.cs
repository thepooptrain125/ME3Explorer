using System;
using System.IO;
using Gibbed.IO;
using System.Threading.Tasks;
using ZlibHelper;

namespace AmaroK86.MassEffect3.ZlibBlock
{
    public static class ZBlock
    {
        public const uint magic = 0x9E2A83C1;
        public const uint maxSegmentSize = 0x20000; //this will be removed later

        public static Task<byte[]> DecompressAsync(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException();
            return Task.Run(() =>
            {
                return Decompress(buffer);
            });
        }


        /*
 * Name function: Compress
 * Purpose: compress a part of the byte array into a Zlib Block
 * Input: - buffer: byte array
 * Output: compressed byte array block, the structure is:
 *         - magic word
 *         - max segment size
 *         - total compressed size
 *         - total uncompressed size
 *         - segment list
 *         - compressed data list
 */
        public static byte[] Compress(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException();

            MemoryStream headBlock = new MemoryStream();
            MemoryStream dataBlock = new MemoryStream();

            int numSeg = (int)Math.Ceiling(buffer.Length / (double)maxSegmentSize);

            headBlock.WriteValueU32(magic);
            headBlock.WriteValueU32(maxSegmentSize);
            headBlock.WriteValueU32(0x0);            //total compressed size, still to calculate
            headBlock.WriteValueS32(buffer.Length);          //total uncompressed size

            int offset = 0;
            for (int i = buffer.Length; i > 0; i -= (int)maxSegmentSize)
            {
                int copyBytes = Math.Min(i, (int)maxSegmentSize);
                uint precCompSize = (uint)dataBlock.Length;
                byte[] src = new byte[copyBytes];
                Buffer.BlockCopy(buffer, offset, src, 0, copyBytes);
                byte[] dst = Zlib.Compress(src);
                if (dst.Length == 0)
                    throw new Exception("Zlib compression failed!");

                dataBlock.WriteBytes(dst);
                offset += dst.Length;
                headBlock.WriteValueU32((uint)dst.Length); //compressed segment size
                headBlock.WriteValueS32(copyBytes); //uncompressed segment size
                //Console.WriteLine("  Segment size: {0}, total read: {1}, compr size: {2}", maxSegmentSize, copyBytes, (uint)dataBlock.Length - precCompSize);
            }

            headBlock.Seek(8, SeekOrigin.Begin);
            headBlock.WriteValueS32((int)dataBlock.Length); // total compressed size

            byte[] finalBlock = new byte[headBlock.Length + dataBlock.Length];
            Buffer.BlockCopy(headBlock.ToArray(), 0, finalBlock, 0, (int)headBlock.Length);
            Buffer.BlockCopy(dataBlock.ToArray(), 0, finalBlock, (int)headBlock.Length, (int)dataBlock.Length);
            headBlock.Close();
            dataBlock.Close();

            return finalBlock;
        }

        public static byte[] Decompress(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException();

            using (MemoryStream buffStream = new MemoryStream(buffer))
            {
                uint magicStream = buffStream.ReadValueU32();
                if (magicStream != magic && magicStream.Swap() != magic) //this should be removed as this is from when we used to try and support .xxx files
                {
                    throw new InvalidDataException("found an invalid zlib block");
                }

                uint buffMaxSegmentSize = buffStream.ReadValueU32();
                if (buffMaxSegmentSize != maxSegmentSize)
                {
                    throw new FormatException();
                }

                uint totComprSize = buffStream.ReadValueU32();
                uint totUncomprSize = buffStream.ReadValueU32();

                byte[] outputBuffer = new byte[totUncomprSize];
                int numBlocks = (int)Math.Ceiling(totUncomprSize / (double)maxSegmentSize);
                int startOfBlocksOffset = 16;
                int blockDataOffset = startOfBlocksOffset + (numBlocks * 8);
                int buffOff = 0;

                for (int i = 0; i < numBlocks; i++)
                {
                    buffStream.Seek(startOfBlocksOffset, SeekOrigin.Begin);
                    int compresesedSize = buffStream.ReadValueS32(); //compressed size
                    int uncompressedSize = buffStream.ReadValueS32(); //uncompressed size
                    startOfBlocksOffset = (int)buffStream.Position; //next block header position

                    buffStream.Seek(blockDataOffset, SeekOrigin.Begin);
                    //Console.WriteLine("compr size: {0}, uncompr size: {1}, data offset: 0x{2:X8}", comprSegm, uncomprSegm, dataSegm);
                    byte[] src = buffStream.ReadBytes(compresesedSize);
                    byte[] dst = new byte[uncompressedSize];
                    if (Zlib.Decompress(src, (uint)src.Length, dst) != uncompressedSize)
                        throw new Exception("Zlib decompression failed! Amount decompressed does not match expected size");

                    Buffer.BlockCopy(dst, 0, outputBuffer, buffOff, uncompressedSize);

                    buffOff += uncompressedSize;
                    blockDataOffset += compresesedSize;
                }
                buffStream.Close();
                return outputBuffer;
            }
        }
    }
}
