using MacrosAPI_v2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            /* Создание API */
            MacrosUpdater updater = new MacrosUpdater();
            MacrosManager manager = new MacrosManager(updater);

            /* Проверка установленного драйвера */
            if (manager.isDriverInstalled)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Драйвер установлен");
                Console.ForegroundColor = ConsoleColor.Gray;

                manager.PluginLoad(new Test());
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Драйвер не установлен");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }


    }

    class Test : Macros
    {
        public override void Initialize()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Макрос загружен");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public override bool OnKeyDown(Key key, bool repeat)
        {
            if (key == Key.R)
            {
                KeyDown(Key.A);
                KeyUp(Key.A);

                return true;
            }

            return false;
        }
    }
}
