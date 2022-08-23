using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Additional namespaces
using System.IO;
using TFGames_Util_4._0.Math;

namespace TFGames_Util_4._0.Cryptography
{
    static class Crypter
    {
        // The encryption key
        const string key = "as;dwepo2345098]qw]{}p2039458pseasdfzcvvp;aseiurwefsdcfszdcvn";
        public static bool wasBE = false;

        public static void Encrypt(string input, string output)
        {
            FileStream instream = File.OpenRead(input);
            BinaryReader instreamReader = new BinaryReader(instream);
            byte[] mark = new byte[2];
            // Later on we will check whether these two bytes are actually mark bytes
            mark = instreamReader.ReadBytes(2);
            byte[] realContent = new byte[instream.Length - 2];
            realContent = instreamReader.ReadBytes((int)instream.Length - 2);
            string[] blocks = null;
            Int32 totalSize = 0;
            List<byte> outputBuffList = new List<byte>();
            bool isLittleEndian = true;
            bool isUnicode = false;

            // Checking whether the read bytes are mark bytes
            if (Encoding.GetEncoding(1252).GetString(mark) == "..")
            {
                // If marked, it means it is pure Windows-1252 encoded... However, we only convert realContent, which doesn't include the mark
                blocks = Encoding.GetEncoding(1252).GetString(realContent).Split(new string[] { "ENDBLOCK" }, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                // ...if not, it will be Unicode, and since in this case there is no mark, we need to convert the whole content
                isUnicode = true;
                instream.Seek(0, SeekOrigin.Begin);
                blocks = Encoding.Unicode.GetString(instreamReader.ReadBytes((int)instream.Length)).Split(new string[] { "ENDBLOCK" }, StringSplitOptions.RemoveEmptyEntries);
            }

            // Detecting endianess for which I put a mark into the text (LE = Little Endian, BE = BigEndian)
            if (blocks[0].Substring(0, 2).IndexOf("LE") != -1)
            {
                isLittleEndian = true;
                outputBuffList.AddRange(MathConverter.MathToBytes(blocks.Length, true));
            }
            else
            {
                isLittleEndian = false;
                wasBE = true;
                outputBuffList.AddRange(MathConverter.MathToBytes(blocks.Length, false));
            }

            instream.Close();

            // Removing the endianess mark from the first block
            blocks[0] = blocks[0].Substring(2);

            for (int i = 0; i < blocks.Length; i++)
            {
                if (blocks[i].IndexOf("EMPTYBLOCK") != -1)
                {
                    // I emphasize, only buffer 32 bit integers to the file
                    outputBuffList.AddRange(MathConverter.MathToBytes((Int32)0, true));
                }
                else
                {
                    bool isUnicode16BitBlock = false;
                    byte[] inputBuffer;

                    /* Detect whether the block WAS Unicode block before decryption.
                       If so, the whole content is Unicode necoded, we can just read it as a Unicode block
                       If not, we have to detect whether the whole content is Unicode, if so,
                       we need to convert it back to Windows-1252 encoding. If not, then we
                       can read it as Windows-1252, because it is pure WIndows-1252 encoded, there are/were NOT
                       Unicode blocks in it. 
                    */
                    if (blocks[i].IndexOf("UNICODE16BITBLOCK") != -1)
                    {
                        inputBuffer = Encoding.Unicode.GetBytes(blocks[i].Replace("UNICODE16BITBLOCK", ""));

                        // Writing block size as Int32 according to the endianess
                        if (isLittleEndian)
                        {
                            outputBuffList.AddRange(MathConverter.MathToBytes(((inputBuffer.Length + 2) / -2), true));
                            isUnicode16BitBlock = true;
                        }
                        else
                        {
                            outputBuffList.AddRange(MathConverter.MathToBytes(((inputBuffer.Length + 2) / -2), false));
                            isUnicode16BitBlock = true;
                        }
                    }
                    else
                    {
                        // If the whole content is Unicode, but was a Windows-1252 block, convert it back
                        if (isUnicode)
                        {
                            inputBuffer = Encoding.Convert(Encoding.Unicode, Encoding.GetEncoding(1252), Encoding.Unicode.GetBytes(blocks[i]));
                        }
                        else
                        {
                            inputBuffer = Encoding.GetEncoding(1252).GetBytes(blocks[i]);
                        }

                        if (isLittleEndian)
                        {
                            outputBuffList.AddRange(MathConverter.MathToBytes((inputBuffer.Length + 1), true));
                        }
                        else
                        {
                            outputBuffList.AddRange(MathConverter.MathToBytes((inputBuffer.Length + 1), false));
                        }
                    }

                    byte[] outputBuffer = new byte[(isUnicode16BitBlock ? inputBuffer.Length + 2 : inputBuffer.Length + 1)];
                    outputBuffer[outputBuffer.Length - 1] = 0;

                    if (isUnicode16BitBlock)
                    {
                        outputBuffer[outputBuffer.Length - 2] = 0;
                    }

                    // If the block is Unicode, only the first byte should be encrypted, the second one can be put back next to it
                    for (int g = 0; g < (isUnicode16BitBlock ? outputBuffer.Length - 2 : outputBuffer.Length - 1); g++)
                    {
                        // Encryption/decryption is XOR
                        outputBuffer[g] = Convert.ToByte(inputBuffer[g] ^ key[totalSize % key.Length]);
                        totalSize++;

                        if (isUnicode16BitBlock)
                        {
                            g++;
                            outputBuffer[g] = inputBuffer[g];
                        }
                    }

                    outputBuffList.AddRange(outputBuffer);
                }
            }

            FileStream outstream = new FileStream(output, FileMode.Create);
            outstream.Write(outputBuffList.ToArray(), 0, outputBuffList.Count);
            outstream.Close();
        }

        public static void Decrypt(string input, string output)
        {
            FileStream instream = new FileStream(input, FileMode.Open);
            BinaryReader instreamReader = new BinaryReader(instream);
            bool isLittleEndian = true;
            bool is16BitUnicodeBlock = false;
            bool was16BitUnicodeBlockAtLeastOnceBefore = false;
            bool was16BitUnicodePreviousBlock = false;
            // The first four bytes (32 bits) are converetd to a number which tells the number of blocks
            Int32 numberOfBlocks = instreamReader.ReadInt32();
            Int32 totalSize = 0;
            List<byte> outputBuffList = new List<byte>();

            // Detecting endianess. Little Endian is Windows, Big Endian is eighter XBOX 360 or PS3
            if (numberOfBlocks > instream.Length)
            {
                instream.Seek(0, SeekOrigin.Begin);
                numberOfBlocks = MathConverter.MathToInt32(instreamReader.ReadBytes(4), 0, false);
                isLittleEndian = false;
                wasBE = true;
            }

            for (int i = 1; i <= numberOfBlocks; i++)
            {
                Int32 blockSize = 0;

                // The next 4 bytes tell the size of the next block in bytes, including the null terminator(s) (1: Windows-1252, 2: Unicode) at the end of the block, which is NOT decrypted
                blockSize = MathConverter.Int32Converter(instreamReader.ReadBytes(4), isLittleEndian, ref is16BitUnicodeBlock, ref was16BitUnicodeBlockAtLeastOnceBefore);

                // Sometiems there are zero size blocks too
                if (blockSize == 0)
                {
                    if (was16BitUnicodeBlockAtLeastOnceBefore)
                    {
                        outputBuffList.AddRange(Encoding.Unicode.GetBytes("EMPTYBLOCK"));
                    }
                    else
                    {
                        outputBuffList.AddRange(Encoding.GetEncoding(1252).GetBytes("EMPTYBLOCK"));
                    }
                }
                else
                {
                    byte[] blockBuffer = instreamReader.ReadBytes(blockSize);
                    byte[] decryptedBlock = new byte[(is16BitUnicodeBlock ? blockSize - 2 : blockSize - 1)];

                    // If it is a Windows-1252 block, the last byte, if Unicode, the last 2 bytes are NOT processed
                    for (int g = 0; g < (is16BitUnicodeBlock ? blockSize - 2 : blockSize - 1); g++)
                    {
                        // Encryption/decryption is XOR
                        decryptedBlock[g] = Convert.ToByte(blockBuffer[g] ^ key[totalSize % key.Length]);
                        // The position in the key is continued, shouldn't start from 0 for each block
                        totalSize++;

                        // The second byte of the two-byte Unicode characters is NOT processed, only the first one
                        if (is16BitUnicodeBlock)
                        {
                            g++;
                            decryptedBlock[g] = blockBuffer[g];
                        }
                    }

                    // If a Unicode block exist, our attempt to keep it Windows-1252 encoded failed... We need to convert the whole already processed blocks to Unicode to maintain lossless quality (which will make the output twice as big, however)
                    if (was16BitUnicodePreviousBlock != was16BitUnicodeBlockAtLeastOnceBefore)
                    {
                        outputBuffList = Encoding.Convert(Encoding.GetEncoding(1252), Encoding.Unicode, outputBuffList.ToArray()).ToList();
                    }

                    if (is16BitUnicodeBlock)
                    {
                        outputBuffList.AddRange(Encoding.Unicode.GetBytes("UNICODE16BITBLOCK"));
                        outputBuffList.AddRange(decryptedBlock);
                    }
                    else
                    {
                        // Only convert to Unicode if really needed
                        if (was16BitUnicodeBlockAtLeastOnceBefore)
                        {
                            outputBuffList.AddRange(Encoding.Convert(Encoding.GetEncoding(1252), Encoding.Unicode, decryptedBlock));
                        }
                        else
                        {
                            outputBuffList.AddRange(decryptedBlock);
                        }
                    }
                }

                is16BitUnicodeBlock = false;
                was16BitUnicodePreviousBlock = was16BitUnicodeBlockAtLeastOnceBefore;

                if (was16BitUnicodeBlockAtLeastOnceBefore)
                {
                    outputBuffList.AddRange(Encoding.Unicode.GetBytes("ENDBLOCK"));
                }
                else
                {
                    outputBuffList.AddRange(Encoding.GetEncoding(1252).GetBytes("ENDBLOCK"));
                }
            }

            outputBuffList.InsertRange(0, (was16BitUnicodeBlockAtLeastOnceBefore ? (isLittleEndian ? Encoding.Unicode.GetBytes("LE") : Encoding.Unicode.GetBytes("BE")) : (isLittleEndian ? Encoding.GetEncoding(1252).GetBytes("LE") : Encoding.GetEncoding(1252).GetBytes("BE"))));

            if (!was16BitUnicodeBlockAtLeastOnceBefore)
            {
                outputBuffList.InsertRange(0, Encoding.GetEncoding(1252).GetBytes(".."));
            }

            instream.Close();
            FileStream outstream = new FileStream(output, FileMode.Create);
            outstream.Write(outputBuffList.ToArray(), 0, outputBuffList.Count);
            outstream.Close();
        }
    }
}
