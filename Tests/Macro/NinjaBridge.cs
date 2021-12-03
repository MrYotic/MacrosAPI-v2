using MacrosAPI_v2;
using System;
using System.Windows.Forms;

namespace Tests.Macro
{
    public class NinjaBridge : Macros
    {
        public override void Initialize()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Макрос загружен");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine();
        }
        public override bool OnKeyUp(Key key)
        {
            return false;
        }
        public override bool OnKeyDown(Key key, bool repeat)
        {
            return false;
        }
        public override void Update()
        {
            if (IsKeyPressed(Keys.R))
            {
                Console.WriteLine("R");
            }
        }
    }
}
