using System;
using System.IO;
using DeviceID = System.Int32;

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
            Handler.PluginUnLoad(bot); Handler.PluginLoad(bot);
        }
        protected void UnLoadPlugin(Macros bot)
        {
            Handler.PluginUnLoad(bot);

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
            Handler.PluginLoad(new Script(filename));
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

        protected void KeyDown(DeviceID deviceID, params Key[] keys)
        {
            foreach (Key key in keys)
            {
                Interception.Stroke stroke = new Interception.Stroke();
                stroke.Key = _handler.ToKeyStroke(key, true);
                Interception.Send(_handler.context, deviceID, ref stroke, 1);
            }
        }

        protected void KeyDown(params Key[] keys)
        {
            KeyDown(_handler.currentDeviceID, keys);
        }

        protected void KeyUp(DeviceID deviceID, params Key[] keys)
        {
            foreach (Key key in keys)
            {
                Interception.Stroke stroke = new Interception.Stroke();
                stroke.Key = _handler.ToKeyStroke(key, false);
                Interception.Send(_handler.context, deviceID, ref stroke, 1);
            }
        }

        protected void KeyUp(params Key[] keys)
        {
            KeyUp(_handler.currentDeviceID, keys);
        }


        protected void PluginPostObject(object obj)
        {
            Handler.OnPluginPostObjectMethod(obj);
        }
        #endregion
    }
}
