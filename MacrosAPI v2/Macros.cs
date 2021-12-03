using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DeviceID = System.Int32;
using KeyList = System.Collections.Generic.List<Key>;


namespace MacrosAPI_v2
{
    public abstract class Macros
    {
        #region Системное
        private MacrosManager _handler = null;
        private MacrosManager Handler
        {
            get
            {
                if (master != null)
                    return master.Handler;
                if (_handler != null)
                    return _handler;
                throw new InvalidOperationException("Error");
            }
        }
        public void SetHandler(MacrosManager handler)
        { 
            this._handler = handler;
        }
        private Macros master = null;
        protected void SetMaster(Macros master) { this.master = master; }
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int keys);
        #endregion

        #region Загрузка и выгрузка плагина
        protected void LoadPlugin(Macros bot)
        {
            Handler.UnLoadMacros(bot); Handler.LoadMacros(bot);
        }
        protected void UnLoadPlugin(Macros bot)
        {
            Handler.UnLoadMacros(bot);

            if (Handler.OnUnloadPlugin != null)
            {
                Handler.OnUnloadPlugin();
            }
        }
        protected void UnLoadPlugin()
        {
            UnLoadPlugin(this);
        }
        protected void RunScript(FileInfo filename)
        {
            Handler.LoadMacros(new Script(filename));
        }
        #endregion

        #region Ивенты плагина

        public virtual void Initialize() { }

        public virtual void Update() { }

        public virtual void ReceivedObject(object s) { }

        public virtual bool OnKeyDown(Key key, bool repeat) { return false; }
        public virtual bool OnKeyUp(Key key) { return false; }

        public virtual bool OnMouseDown(MouseKey key) { return false; }
        public virtual bool OnMouseUp(MouseKey key) { return false; }
        public virtual bool OnMouseMove(int x, int y) { return false; }
        #endregion

        #region Методы плагина
        protected void Sleep(int delay) { System.Threading.Thread.Sleep(delay); }
        #region Работа с клавиатурой
        protected bool IsKeyPressed(Keys key)
        {
            return GetAsyncKeyState((int)key) != 0;
        }
        protected bool IsKeyDown(DeviceID deviceID, Key key)
        {
            KeyList deviceDownedKeys;
            if (!Handler.downedKeys.TryGetValue(deviceID, out deviceDownedKeys))
                return false;
            return deviceDownedKeys.Contains(key);
        }

        protected bool IsKeyDown(Key key)
        {
            return IsKeyDown(Handler.keyboardDeviceID, key);
        }

        protected bool IsKeyUp(DeviceID deviceID, Key key)
        {
            return !IsKeyDown(deviceID, key);
        }

        protected bool IsKeyUp(Key key)
        {
            return IsKeyUp(Handler.keyboardDeviceID, key);
        }


        protected void KeyDown(DeviceID deviceID, params Key[] keys)
        {
            foreach (Key key in keys)
            {
                Interception.Stroke stroke = new Interception.Stroke();
                stroke.Key = Handler.ToKeyStroke(key, true);
                Interception.Send(Handler.keyboard, deviceID, ref stroke, 1);
            }
        }

        protected void KeyDown(params Key[] keys)
        {
            KeyDown(Handler.keyboardDeviceID, keys);
        }

        protected void KeyUp(DeviceID deviceID, params Key[] keys)
        {
            foreach (Key key in keys)
            {
                Interception.Stroke stroke = new Interception.Stroke();
                stroke.Key = Handler.ToKeyStroke(key, false);
                Interception.Send(Handler.keyboard, deviceID, ref stroke, 1);
            }
        }

        protected void KeyUp(params Key[] keys)
        {
            KeyUp(Handler.keyboardDeviceID, keys);
        }
        #endregion
        #region Работа с мышкой
        protected void MouseScroll(DeviceID deviceID, short rolling)
        {
            Interception.Stroke stroke = new Interception.Stroke();
            stroke.Mouse.State = Interception.MouseState.Wheel;
            stroke.Mouse.Rolling = rolling;
            Interception.Send(Handler.mouse, deviceID, ref stroke, 1);
        }
        protected void MouseScroll(short rolling)
        {
            MouseScroll(Handler.mouseDeviceID, rolling);
        }
        protected void MouseDown(DeviceID deviceID, params MouseKey[] keys)
        {
            foreach (MouseKey key in keys)
            {
                Interception.Stroke stroke = new Interception.Stroke();
                switch (key)
                {
                    case (MouseKey.Left):
                        stroke.Mouse.State = Interception.MouseState.LeftButtonDown;
                        break;
                    case (MouseKey.Right):
                        stroke.Mouse.State = Interception.MouseState.RightButtonDown;
                        break;
                    case (MouseKey.Middle):
                        stroke.Mouse.State = Interception.MouseState.MiddleButtonDown;
                        break;
                    case (MouseKey.Button1):
                        stroke.Mouse.State = Interception.MouseState.Button4Down;
                        break;
                    case (MouseKey.Button2):
                        stroke.Mouse.State = Interception.MouseState.Button5Down;
                        break;
                }    
                Interception.Send(Handler.mouse, deviceID, ref stroke, 1);
            }
        }

        protected void MouseUp(DeviceID deviceID, params MouseKey[] keys)
        {
            foreach (MouseKey key in keys)
            {
                Interception.Stroke stroke = new Interception.Stroke();
                switch (key)
                {
                    case (MouseKey.Left):
                        stroke.Mouse.State = Interception.MouseState.LeftButtonUp;
                        break;
                    case (MouseKey.Right):
                        stroke.Mouse.State = Interception.MouseState.RightButtonUp;
                        break;
                    case (MouseKey.Middle):
                        stroke.Mouse.State = Interception.MouseState.MiddleButtonUp;
                        break;
                    case (MouseKey.Button1):
                        stroke.Mouse.State = Interception.MouseState.Button4Up;
                        break;
                    case (MouseKey.Button2):
                        stroke.Mouse.State = Interception.MouseState.Button5Up;
                        break;
                }
                Interception.Send(Handler.mouse, deviceID, ref stroke, 1);
            }
        }

        protected void MouseMove(DeviceID deviceID, int x, int y)
        {
            Interception.Stroke stroke = new Interception.Stroke();
            stroke.Mouse.X = x;
            stroke.Mouse.Y = y;
            Interception.Send(Handler.mouse, deviceID, ref stroke, 1);
        }

        protected void MouseDown(params MouseKey[] keys)
        {
            MouseDown(Handler.mouseDeviceID, keys);
        }

        protected void MouseUp(params MouseKey[] keys)
        {
            MouseUp(Handler.mouseDeviceID, keys);
        }

        protected void MouseMove(int x, int y)
        {
            MouseMove(Handler.mouseDeviceID, x, y);
        }
        #endregion
        protected Bitmap GetScreenShot(Process process)
        {
            var hwnd = process.MainWindowHandle;

            WinAPI.GetWindowRect(hwnd, out var rect);
            using (var image = new Bitmap(rect.Right - rect.Left, rect.Bottom - rect.Top))
            {
                using (var graphics = Graphics.FromImage(image))
                {
                    var hdcBitmap = graphics.GetHdc();
                    WinAPI.PrintWindow(hwnd, hdcBitmap, 0);
                    graphics.ReleaseHdc(hdcBitmap);
                }
                return image;
            }
        }
        protected Process GetActiveProcess()
        {
            IntPtr h = WinAPI.GetForegroundWindow();
            int pid = 0;
            WinAPI.GetWindowThreadProcessId(h, ref pid);
            Process p = Process.GetProcessById(pid);
            return p;
        }
        protected void PluginPostObject(object obj)
        {
            Handler.OnMacrosPostObjectMethod(obj);
        }
        #endregion
    }
}
