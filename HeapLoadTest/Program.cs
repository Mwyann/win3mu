#define NO_LOGGING

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Win3muCoreUnitTests;

namespace HeapLoadTest
{
    class Program
    {
        [Conditional("LOGGING")]
        public static void Write(string str)
        {
            Console.Write(str);
        }

        [Conditional("LOGGING")]
        public static void WriteLine(string str)
        {
            Console.WriteLine(str);
        }

        [Conditional("LOGGING")]
        public static void Write(string str, params object[] args)
        {
            Console.Write(str, args);
        }

        [Conditional("LOGGING")]
        public static void WriteLine(string str, params object[] args)
        {
            Console.WriteLine(str, args);
        }

        static void Main(string[] args)
        {
            var heap = new TestHeap(32768);

            var r = new Random(0);

            var allocations = new List<TestHeap.TestAllocation>();

            var sw = new Stopwatch();
            sw.Start();

            int operations = 1000000;
            for (int i = 1; i < operations; i++)
            {
                if ((i % 10000)==0)
                {
                    Console.WriteLine("After {0} operations: allocation count:{1} free space:{2} free entries:{3} ", 
                        i * 100, 
                        heap.AllocationCount, 
                        heap.FreeSpace, 
                        heap.FreeSegmentCount
                        );
                }

                // Defrag?
                if (r.Next(50) == 1)
                {
                    WriteLine("Defragging...");
                    heap.Defrag();
                    continue;
                }

                // Lock or unlock?
                if (r.Next(20) == 1)
                {
                    if (allocations.Count>0)
                    {
                        var alloc = allocations[r.Next(allocations.Count)];
                        if (r.Next(2)==1)
                        {
                            if (!alloc.locked)
                            {
                                WriteLine("Locking {0}...", alloc.id);
                                alloc.locked = true;
                                continue;
                            }
                        }
                        else
                        {
                            if (alloc.locked)
                            {
                                WriteLine("Unlocking {0}...", alloc.id);
                                alloc.locked = false;
                                continue;
                            }
                        }
                    }
                }

                if ((r.Next(10) == 1 && allocations.Count!=0))
                {
                    // Free 
                    int index = r.Next(allocations.Count);
                    var a = allocations[index];

                    int size = r.Next(250)+1;
                    bool defrag = r.Next(5) < 4;
                    bool movable = r.Next(5) < 4;

                    Write("ReAlloc #{0} from {1} to {2} bytes {3}",
                        a.id, a.size, size, defrag ? "allow defrag" : "no defrag");

                    if (heap.ReAlloc(a, size, movable, defrag))
                    {
                        Debug.Assert(a.size == size);
                        WriteLine(" = {0}", a.position);
                    }
                    else
                    {
                        WriteLine(" Failed!");
                    }
                    continue;
                }

                if (r.Next(15000) < (i % 15000) || allocations.Count==0)
                {
                    // Allocate
                    int size = r.Next(120) + 1;
                    bool movable = r.Next(5) < 4;
                    bool defrag = r.Next(5) < 4;

                    Write("Allocating {0} bytes {1} {2}",
                        size,
                        movable ? "moveable" : "fixed",
                        defrag ? "allow defrag" : "no defrag");

                    
                    var a = heap.Alloc(size, movable, defrag);
                    if (a != null)
                        allocations.Add(a);

                    if (a != null)
                        WriteLine(" = #{0} @ {1}", a.id, a.position);
                    else
                        WriteLine(" Failed!");

                }
                else
                {
                    // Free 
                    int index = r.Next(allocations.Count);
                    var a = allocations[index];

                    WriteLine("Free #{0}", a.id);
                    heap.Free(a);
                    allocations.RemoveAt(index);
                }
            }

            sw.Stop();

            Console.WriteLine("\nPost Test:");
            heap.Defrag();
            heap.CheckAll();
            //heap.Allocator.Dump();

            /*
            for (int i=0; i<allocations.Count; i++)
            {
                allocations[i].locked = false;
            }

            Console.WriteLine("\nPost Unlock All:");
            heap.Defrag();
            heap.CheckAll();
            heap.Allocator.Dump();
            */

            Console.WriteLine("Final address space size: {0}", heap.AddressSpaceSize);
            Console.WriteLine("Total heap operations: {0}", operations * 100);
            Console.WriteLine("Main test completed in {0} seconds", sw.Elapsed.TotalSeconds * 100);
        }
    }
}
