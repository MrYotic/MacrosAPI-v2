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

            manager.LoadMacros(new Test());
        }
    }

    class Test : Macros
    {
        public override void Initialize()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Макрос загружен");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine();
        }
        private bool clicker = false;
        private bool clicker2 = false;
        public override bool OnKeyDown(Key key, bool repeat)
        {
            if (key == Key.R)
            {
                clicker = !clicker;
            }
            return false;
        }
        public override bool OnMouseMove(int x, int y)
        {
            return false;
        }
        public override void Update()
        {
            if (clicker && clicker2)
            {
                MouseUp(MouseKey.Right);
                MouseDown(MouseKey.Left);
                MouseUp(MouseKey.Left);
                Sleep(1000 / 12);
            }
        }
        public override bool OnMouseDown(MouseKey key)
        {
            if (key == MouseKey.Left && clicker)
            {
                clicker2 = true;
                Console.WriteLine("Кликер включен");
                return true;
            }
            return false;
        }
        public override bool OnMouseUp(MouseKey key)
        {
            if (key == MouseKey.Left && clicker)
            {
                clicker2 = false;
                Console.WriteLine("Кликер выключен");
                return true;
            }
            return false;
        }
    }
}
