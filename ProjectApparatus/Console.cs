using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ProjectApparatus
{
    public class MainConsole
    {
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int pid);
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool AllocConsole();
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool FreeConsole();

        private static bool b_DoesConsoleExist;

        public static bool DoesConsoleExist()
        {
            return b_DoesConsoleExist;
        }

        public static void InitConsole()
        {
            Allocate();
            while (true)
            {
                Console.ReadLine(); // keep it open
            }
        }

        public static void Allocate()
        {
            b_DoesConsoleExist = true;
            string processName = "Lethal Company";

            int pid = Process.GetProcessesByName(processName)[0].Id;
            AttachConsole(pid);
            AllocConsole();
            Console.WriteLine("test");
            Console.ReadLine();
        }
        public static void log(object msg)
        {
            Console.WriteLine($"{msg}");
        }
        public static void DeAllocate()
        {
            FreeConsole();
            b_DoesConsoleExist = false;
        }
    }
}
