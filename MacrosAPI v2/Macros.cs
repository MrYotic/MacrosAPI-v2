using System;
using System.IO;
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
        #endregion

        #region Методы плагина
        protected bool IsKeyDown(DeviceID deviceID, Key key)
        {
            KeyList deviceDownedKeys;
            if (!Handler.downedKeys.TryGetValue(deviceID, out deviceDownedKeys))
                return false;
            return deviceDownedKeys.Contains(key);
        }

        protected bool IsKeyDown(Key key)
        {
            return IsKeyDown(Handler.currentDeviceID, key);
        }

        protected bool IsKeyUp(DeviceID deviceID, Key key)
        {
            return !IsKeyDown(deviceID, key);
        }

        protected bool IsKeyUp(Key key)
        {
            return IsKeyUp(Handler.currentDeviceID, key);
        }


        protected void KeyDown(DeviceID deviceID, params Key[] keys)
        {
            foreach (Key key in keys)
            {
                Interception.Stroke stroke = new Interception.Stroke();
                stroke.Key = Handler.ToKeyStroke(key, true);
                Interception.Send(Handler.context, deviceID, ref stroke, 1);
            }
        }

        protected void KeyDown(params Key[] keys)
        {
            KeyDown(Handler.currentDeviceID, keys);
        }

        protected void KeyUp(DeviceID deviceID, params Key[] keys)
        {
            foreach (Key key in keys)
            {
                Interception.Stroke stroke = new Interception.Stroke();
                stroke.Key = Handler.ToKeyStroke(key, false);
                Interception.Send(Handler.context, deviceID, ref stroke, 1);
            }
        }

        protected void KeyUp(params Key[] keys)
        {
            KeyUp(Handler.currentDeviceID, keys);
        }


        protected void PluginPostObject(object obj)
        {
            Handler.OnMacrosPostObjectMethod(obj);
        }
        #endregion
    }
}
