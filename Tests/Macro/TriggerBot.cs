using MacrosAPI_v2;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tests.Macro
{
    public class TriggerBot : Macros
    {
        public bool IfTarget
        {
            get
            {
                Bitmap printscreen = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);

                Graphics graphics = Graphics.FromImage(printscreen as Image);

                graphics.CopyFromScreen(0, 0, 0, 0, printscreen.Size);

                var pixel = printscreen.GetPixel(838, 525);

                if (pixel.R == 255 && pixel.G == 236 && pixel.B == 0)
                {
                    return true;
                }
                else
                {
                    Console.WriteLine(pixel);
                    return false;
                }
            }
        }
        public override void Update()
        {
            if (IfTarget && activ)
            {
                Console.WriteLine("Бью");
                MouseUp(MouseKey.Left);
                MouseDown(MouseKey.Left);
                MouseUp(MouseKey.Left);
            }
        }
        bool activ = false;
        public override bool OnMouseDown(MouseKey key)
        {
            if (key == MouseKey.Left)
            {
                activ = true;
            }
            return false;
        }
        public override bool OnMouseUp(MouseKey key)
        {
            if (key == MouseKey.Left)
            {
                activ = false;
            }
            return false;
        }
    }
}
