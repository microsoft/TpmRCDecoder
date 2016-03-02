using System;
using System.Collections.Generic;
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

            Decoder decoder = new Decoder();
            String output = decoder.Decode(args[0]);
            System.Console.WriteLine(output);
        }
    }
}
