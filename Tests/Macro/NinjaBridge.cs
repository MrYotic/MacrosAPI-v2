using MacrosAPI_v2;
using System;

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
            EnableMouseMoveEvent(true);
        }
        private bool enable = false;
        public override bool OnKeyUp(Key key)
        {
            if (key == Key.R)
            {
                enable = false;
                return false;
            }
            else if (key == Key.LShift && enable)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public override bool OnKeyDown(Key key, bool repeat)
        {
            if (key == Key.R)
            {
                enable = true;
                return false;
            }
            else if (key == Key.LShift && enable)
            {
                return true;
            }
            else 
            { 
                return false; 
            }
        }
        public override void Update()
        {
            if (enable)
            {
                //MouseDown(MouseKey.Right);
                //Sleep(5);
                //MouseUp(MouseKey.Right);
                //KeyDown(Key.S);
                //Sleep(240);
                //KeyDown(Key.LShift);
                //Sleep(145);
                //KeyUp(Key.S);
                //Sleep(5);
                //KeyUp(Key.LShift);
                //MouseDown(MouseKey.Right);
                //Sleep(5);
                //MouseUp(MouseKey.Right);
            }
        }
    }
}
