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
        public override void Update()
        {
            if (IsKeyPressed(Keys.R))
            {
                KeyDown(Key.A);
                KeyUp(Key.A);
            }
        }
        public override bool OnMouseMove(int x, int y)
        {
            return false;
        }
    }
}
