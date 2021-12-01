using System;
using System.IO;
using System.Runtime.InteropServices;
using DeviceID = System.Int32;
using KeyList = System.Collections.Generic.List<Key>;


namespace MacrosAPI_v2
{
    public abstract class Macros
    {
        #region Системное
        [Flags]
        private enum MouseFlags
        {
            Move = 0x0001, LeftDown = 0x0002, LeftUp = 0x0004, RightDown = 0x0008,
            RightUp = 0x0010, Absolute = 0x8000
        };
        [DllImport("User32.dll")]
        private static extern void mouse_event(MouseFlags dwFlags, int dx, int dy, int dwData, UIntPtr dwExtraInfo);
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
        public virtual bool OnMouseUp(Key key) { return false; }
        public virtual bool OnMouseMove(int x, int y) { return false; }
        #endregion

        #region Методы плагина

        #region Работа с клавиатурой
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

        
        protected void RightDown()
        {
            mouse_event(MouseFlags.RightDown, 0, 0, 0, UIntPtr.Zero);
        }
        protected void RightUp()
        {
            mouse_event(MouseFlags.RightUp, 0, 0, 0, UIntPtr.Zero);
        }
        protected void LeftDown()
        {
            mouse_event(MouseFlags.LeftDown, 0, 0, 0, UIntPtr.Zero);
        }
        protected void LeftUp()
        {
            mouse_event(MouseFlags.LeftUp, 0, 0, 0, UIntPtr.Zero);
        }
        protected void RightClick()
        {
            mouse_event(MouseFlags.RightDown | MouseFlags.RightUp, 0, 0, 0, UIntPtr.Zero);
        }
        protected void RightClick(int delay)
        {
            RightDown();
            System.Threading.Thread.Sleep(delay);
            RightUp();
        }
        protected void MouseMove(int x, int y)
        {
            mouse_event(MouseFlags.Move, x, y, 0, UIntPtr.Zero);
        }
        protected void AbsoluteMouseMove(int x, int y)
        {
            mouse_event(MouseFlags.Move | MouseFlags.Absolute, x, y, 0, UIntPtr.Zero);
        }

        protected void PluginPostObject(object obj)
        {
            Handler.OnMacrosPostObjectMethod(obj);
        }
        #endregion
    }
}
