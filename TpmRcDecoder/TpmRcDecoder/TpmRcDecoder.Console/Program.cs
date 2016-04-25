using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TpmRcDecoder.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                System.Console.WriteLine("Usage: TpmRcDecoder <TPM Return Code>");
                return;
            }

            string input;
            if (args[0].Trim().StartsWith("0x"))
            {
                input = args[0].Trim();
            }
            else
            {
                UInt32 errorCode = 0;
                try
                {
                    errorCode = UInt32.Parse(args[0].Trim(), NumberStyles.Integer);
                }
                catch (FormatException)
                { }

                input = string.Format("0x{0:x}", errorCode);
            }

            Decoder decoder = new Decoder();
            String output = decoder.Decode(input);
            System.Console.WriteLine(output);
        }
    }
}
