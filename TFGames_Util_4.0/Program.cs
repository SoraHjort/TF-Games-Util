using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Additional namespaces
using System.IO;
using TFGames_Util_4._0.Cryptography;

namespace TFGames_Util_4._0
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Patrik Nusszer - TF Games Configuration Settings Coder");
            Console.WriteLine();

            bool isReady = true;

            if (File.Exists("isAuto"))
            {
                string[] files = Directory.GetFiles(Directory.GetCurrentDirectory());
                for (int i = 0; i < files.Length; i++)
                {
                    if (files[i].IndexOf("Decoded") != -1 || files[i].IndexOf("decoded") != -1 || files[i].IndexOf("Decrypted") != -1 || files[i].IndexOf("decrypt") != -1)
                    {
                        Crypter.Encrypt(files[i], "Coalesced" + ((i == 0) ? "" : i.ToString()) + ((files[i].IndexOf("Int") != -1 || files[i].IndexOf("int") != -1) ? ".int" : ".ini"));
                    }
                    if (files[i].IndexOf("Coalesced") != -1 || files[i].IndexOf("Encoded") != -1 || files[i].IndexOf("encoded") != -1 || files[i].IndexOf("Encrypted") != -1 || files[i].IndexOf("encrypted") != -1)
                    {
                        Crypter.Decrypt(files[i], "Decrypted" + ((i == 0) ? "" : i.ToString()) + ".txt");
                    }
                }
            }
            else
            {
                if (args.Length > 0)
                {
                    Console.WriteLine("Argument mode is ON");
                    Console.WriteLine();

                    if (args[0] != "D" && args[0] != "d" && args[0] != "E" && args[0] != "e" && args[0] != "A" && args[0] != "a" && args[0] != "U" && args[0] != "u" && args[0] != "S" && args[0] != "s")
                    {
                        Console.WriteLine("The operation type was not provided or was mistaken");
                        isReady = false;
                    }

                    if (isReady)
                    {
                        if (args[0] == "E" || args[0] == "e")
                        {
                            if (!File.Exists(args[1]))
                            {
                                Console.WriteLine("The input file does not exist");
                                isReady = false;
                            }
                            if (string.IsNullOrWhiteSpace(args[2]))
                            {
                                Console.WriteLine("The output was not provided");
                                isReady = false;
                            }

                            if (isReady)
                            {
                                Crypter.Encrypt(args[1], args[2]);

                                if (Crypter.wasBE)
                                {
                                    Console.WriteLine("Encryption is done [XBOX/PS3] [Big Endian]");
                                }
                                else
                                {
                                    Console.WriteLine("Encryption is done [Windows] [Little Endian]");
                                }

                                Crypter.wasBE = false;
                            }
                        }
                        else if (args[0] == "D" || args[0] == "d")
                        {
                            if (!File.Exists(args[1]))
                            {
                                Console.WriteLine("The input file does not exist");
                                isReady = false;
                            }
                            if (string.IsNullOrWhiteSpace(args[2]))
                            {
                                Console.WriteLine("The output was not provided");
                                isReady = false;
                            }

                            if (isReady)
                            {
                                Crypter.Decrypt(args[1], args[2]);

                                if (Crypter.wasBE)
                                {
                                    Console.WriteLine("Decryption is done [XBOX/PS3] [Big Endian]");
                                }
                                else
                                {
                                    Console.WriteLine("Decryption is done [Windows] [Little Endian]");
                                }

                                Crypter.wasBE = false;
                            }
                        }
                        else if (args[0] == "A" || args[0] == "a")
                        {
                            FileStream fs = new FileStream("isAuto", FileMode.Create);
                            fs.Close();
                        }
                        else if (args[0] == "U" || args[0] == "u")
                        {
                            File.Delete("isAuto");
                        }
                        else if (args[0] == "S" || args[0] == "s")
                        {
                            FileStream fs = new FileStream("src.rar", FileMode.Create);
                            fs.Write(TFGames_Util_4._0.Properties.Resources.src, 0, TFGames_Util_4._0.Properties.Resources.src.Length);
                            fs.Close();
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Direct mode is ON");
                    Console.WriteLine();
                    ConsoleKeyInfo cki = new ConsoleKeyInfo('R', ConsoleKey.R, false, false, false);

                    while (cki.KeyChar == 'r' || cki.KeyChar == 'R')
                    {
                        string operation = null;
                        string input = null;
                        string output = null;
                        isReady = false;

                        while (isReady == false)
                        {
                            Console.Write("Please, enter the operation (E/D/A/S): ");
                            operation = Convert.ToString(Console.ReadKey().KeyChar);
                            Console.WriteLine();
                            Console.WriteLine();
                            if (operation != "D" && operation != "d" && operation != "E" && operation != "e" && operation != "S" && operation != "s" && operation != "A" && operation != "a")
                            {
                                Console.WriteLine("The operation type was not provided or was mistaken");
                                Console.WriteLine();
                            }
                            else
                            {
                                isReady = true;
                            }
                        }

                        if (operation == "S" || operation == "s")
                        {
                            FileStream fs = new FileStream("src.rar", FileMode.Create);
                            fs.Write(TFGames_Util_4._0.Properties.Resources.src, 0, TFGames_Util_4._0.Properties.Resources.src.Length);
                            fs.Close();
                            Console.WriteLine("The source code was extracted successfully");
                            Console.WriteLine();
                        }
                        else if (operation == "A" || operation == "a")
                        {
                            FileStream fs = new FileStream("isAuto", FileMode.Create);
                            fs.Close();
                            Console.WriteLine("Automation file was created. Note that you can only stop auto mode by deleting the 'isAuto' file, or running the argument mode with U");
                            Console.WriteLine();
                        }
                        else
                        {
                            isReady = false;

                            while (isReady == false)
                            {
                                Console.Write("Please, enter the input file name: ");
                                input = Console.ReadLine();
                                Console.WriteLine();
                                if (!File.Exists(input))
                                {
                                    Console.WriteLine("The input file does not exist");
                                    Console.WriteLine();
                                }
                                else
                                {
                                    isReady = true;
                                }
                            }
                            isReady = false;

                            while (isReady == false)
                            {
                                Console.Write("Please, enter the output file name: ");
                                output = Console.ReadLine();
                                if (string.IsNullOrWhiteSpace(output))
                                {
                                    Console.WriteLine("The output was not provided");
                                    Console.WriteLine();
                                }
                                else
                                {
                                    isReady = true;
                                }
                            }

                            Console.WriteLine();

                            if (operation == "E" || operation == "e")
                            {
                                Crypter.Encrypt(input, output);

                                if (Crypter.wasBE)
                                {
                                    Console.WriteLine("Encryption is done [XBOX/PS3] [Big Endian]");
                                }
                                else
                                {
                                    Console.WriteLine("Encryption is done [Windows] [Little Endian]");
                                }

                                Crypter.wasBE = false;
                            }
                            else
                            {
                                Crypter.Decrypt(input, output);

                                if (Crypter.wasBE)
                                {
                                    Console.WriteLine("Decryption is done [XBOX/PS3] [Big Endian]");
                                }
                                else
                                {
                                    Console.WriteLine("Decryption is done [Windows] [Little Endian]");
                                }

                                Crypter.wasBE = false;
                            }

                            Console.WriteLine();
                            Console.Write("Press 'r' to repeat, anything else to exit: ");
                            cki = Console.ReadKey();
                            Console.WriteLine();
                            Console.WriteLine();
                        }
                    }
                }
            }
        }
    }
}
