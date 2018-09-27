using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp86;

namespace Sharp86AluTests
{
    class Program
    {
        static void Main(string[] args)
        {
            var alu = new ALU();
            var file = new System.IO.StreamReader("..\\..\\x86flags\\aludata.txt");
            int failed = 0;
            int resultFailures = 0;
            int flagFailures = 0;
            int total = 0;
            while (!file.EndOfStream)
            {
                total++;
                // Read the line and split it
                var str = file.ReadLine();
                var parts = str.Split(' ');

                // Get the method we need to call
                var mi = alu.GetType().GetMethod(parts[0],
                    System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                    

                // Setup the flags
                alu.EFlags = Convert.ToUInt16(parts[1], 16);

                // Setup parameters
                var paramInfos = mi.GetParameters();
                var paramValues = new object[paramInfos.Length];
                for (int i=0; i<paramInfos.Length; i++)
                {
                    var val = Convert.ToUInt32(parts[i + 2], 16);
                    paramValues[i] = Convert.ChangeType(val, paramInfos[i].ParameterType);
                }

                uint result;
                try
                {
                    result = (uint)Convert.ChangeType(mi.Invoke(alu, paramValues), typeof(uint));
                }
                catch (Exception)
                {
                    if (parts[paramInfos.Length + 2] != "????")
                    {
                        resultFailures++;
                        failed++;
                        Console.WriteLine("FAILED: {0} // didn't expect exception", str);
                    }
                    continue;
                }

                if (parts[paramInfos.Length + 2] == "????")
                {
                    resultFailures++;
                    failed++;
                    Console.WriteLine("FAILED: {0} // expected exception", str);
                    alu.EFlags = Convert.ToUInt16(parts[1], 16);
                    result = (uint)Convert.ChangeType(mi.Invoke(alu, paramValues), typeof(uint));
                    continue;
                }

                var expectedResult = Convert.ToUInt32(parts[paramInfos.Length + 2], 16);
                var expectedFlags = Convert.ToUInt16(parts[paramInfos.Length + 3], 16);

                if (result != expectedResult)
                {
                    resultFailures++;
                }

                if (alu.EFlags != expectedFlags)
                {
                    flagFailures++;
                }

                if (result != expectedResult || alu.EFlags != expectedFlags)
                {
                    failed++;
                    Console.WriteLine("FAILED: {0} // {1:X} vs {2:X} // {3:X} vs {4:X}", str, result, expectedResult, alu.EFlags, expectedFlags);
                    alu.EFlags = Convert.ToUInt16(parts[1], 16);
                    result = (uint)Convert.ChangeType(mi.Invoke(alu, paramValues), typeof(uint));
//                    break;
                }
            }

            Console.WriteLine("Total Tests: {0}", total);
            Console.WriteLine("Result fails: {0}", resultFailures);
            Console.WriteLine("Flag fails: {0}", flagFailures);
            Console.WriteLine("Finished.");
        }
    }
}
