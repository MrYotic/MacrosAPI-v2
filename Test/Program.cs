using MacrosAPI_v2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            MacrosUpdater updater = new MacrosUpdater();
            MacrosManager manager = new MacrosManager(updater);
            manager.LoadMacros(new Test());
        }
    }
    public class Test : Macros
    {
        private void Click()
        {
            Sleep(1);
            MouseDown(MouseKey.Left);
            MouseUp(MouseKey.Left);
            Sleep(1);
        }
        private void Exit()
        {
            Sleep(15);
            KeyDown(Key.E);
            KeyUp(Key.E);
            Sleep(1);
        }
        private void FirstSlot()
        {
            MouseSet(27100, 25000);
        }
        public override void Update()
        {
            if (enabled)
            {
                enabled = false;
                MouseSet(695, 400);
            }
        }

        public override bool OnMouseWheel(int rolling)
        {
            Console.WriteLine("Прокрутил на: " + rolling);
            return base.OnMouseWheel(rolling);
        }

        private bool enabled = false;
        public override bool OnMouseDown(MouseKey key)
        {
            Console.WriteLine(key);
            if (key == MouseKey.Button2)
            {
                enabled = true;
            }
            return false;
        }
    }
}
