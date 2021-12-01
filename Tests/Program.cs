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

                manager.LoadMacros(new Test());
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
            switch (key)
            {
                case Key.R:
                    MouseMove(100, 100);
                    //MouseDown(MouseKey.Left);
                    //MouseUp(MouseKey.Left);
                    return true;
            }
            return false;
        }
        public override bool OnMouseMove(int x, int y)
        {
            return false;
        }
        public override bool OnMouseDown(MouseKey key)
        {
            Console.WriteLine("Клавиша: " + key + " нажата");
            return false;
        }
        public override bool OnMouseUp(MouseKey key)
        {
            Console.WriteLine("Клавиша: " + key + " отжата");
            return false;
        }
    }
}
