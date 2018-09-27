using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp86;

namespace ConDos
{
    class PC : CPU, IMemoryBus, IPortBus
    {
        public PC()
        {
            _mem = new byte[1024 * 1024];
            this.MemoryBus = this;
            this.PortBus = this;

            _debugger = new TextGuiDebugger();
            _debugger.CPU = this;
            //_debugger.Break();
        }

        byte[] _mem;
        TextGuiDebugger _debugger;

        public void RunComProgram()
        {
            // Setup the CPU
            ss = 0;
            sp = 0;
            cs = 0;
            ip = 0x100;

            // Write HALT at address 0:0000
            WriteByte(0, 0, 0xF4);

            // Setup return address to 0:0000
            sp -= 2;
            this.WriteWord(ss, sp, 0);

            while (!Halted)
            {
                Run(10000);
            }
        }

        public bool IsExecutableSelector(ushort seg)
        {
            return true;
        }

        public byte ReadByte(ushort seg, ushort offset)
        {
            return _mem[(seg << 4) + offset];
        }

        public void WriteByte(ushort seg, ushort offset, byte value)
        {
            _mem[(seg << 4) + offset] = value;
        }

        public override void RaiseInterrupt(byte interruptNumber)
        {
            if (interruptNumber == 0x21)
            {
                switch (ah)
                {

                    case 0x09:
                        while (true)
                        {
                            al = ReadByte(ds, dx++);
                            if (al == '$')
                                break;
                            Console.Write((char)al);
                        }

                        return;
                }
            }

            base.RaiseInterrupt(interruptNumber);
        }

        public byte ReadPortByte(ushort port)
        {
            throw new NotImplementedException();
        }

        public void WritePortByte(ushort port, byte value)
        {
            throw new NotImplementedException();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Create PC
            var pc = new PC();

            if (args.Length < 1)
            {
                throw new ArgumentException("Specify DOS program on command line");
            }

            var programFile = args[0];

            if (programFile.EndsWith(".com", StringComparison.InvariantCultureIgnoreCase))
            {
                // Load the test program from disk
                var program = System.IO.File.ReadAllBytes(args[0]);
                for (int i=0; i<program.Length; i++)
                {
                    // Write at 0x100
                    pc.WriteByte(0, (ushort)(0x100 + i), program[i]);
                }

                // Run it!
                pc.RunComProgram();
            }   

            if (programFile.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase))
            {
                var file = System.IO.File.OpenRead(programFile);
                var header = file.ReadStruct<MZHEADER>();

                // Check signature
                if (header.signature !=MZHEADER.SIGNATURE)
                {
                    throw new InvalidDataException("Not a valid exe file (no mz header)");
                }

                var totalSize = (header.pages - 1) * 512 + header.extraBytes;
                Console.WriteLine("Pages + Extra = {0} + {1} = {2} (0x{3:X8})", header.pages, header.extraBytes, totalSize, totalSize);
                Console.WriteLine("Header Size = {0} (0x{1:X8})", header.headerSize * 16, header.headerSize * 16);
                Console.WriteLine("Relocation table = {0} (0x{1:X8})", header.relocationTable, header.relocationTable);
                Console.WriteLine("CS:IP = {0:X4}:{1:X4}", header.initialCS, header.initialIP);
                Console.WriteLine("SS:SP = {0:X4}:{1:X4}", header.initialSS, header.initialSP);

                file.Seek(header.relocationTable, SeekOrigin.Begin);
                for (int i=0; i<header.relocationItems; i++)
                {
                    var reloc = file.ReadStruct<MZRELOC>();
                    Console.WriteLine("reloc: {0:X4}:{1:X4}", reloc.segment, reloc.offset);
                }
            }

            Console.WriteLine();
        }

    }
}
