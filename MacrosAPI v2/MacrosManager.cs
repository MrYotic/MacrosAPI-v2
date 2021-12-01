using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using DeviceID = System.Int32;
using KeyList = System.Collections.Generic.List<Key>;

namespace MacrosAPI_v2
{
    public class MacrosManager
    {
        public IntPtr context;
        public DeviceID currentDeviceID = 1;

        public Dictionary<DeviceID, KeyList> downedKeys = new Dictionary<DeviceID, KeyList>();

        public MacrosManager(MacrosUpdater updater)
        {
            context = Interception.CreateContext();
            Interception.SetFilter(context, Interception.IsKeyboard, Interception.Filter.All);

            updater.SetHandler(this);
            updater.StartUpdater();
        }
        private KeyList GetOrCreateKeyList(Dictionary<DeviceID, KeyList> dictionary, DeviceID deviceID)
        {
            KeyList result;
            if (!dictionary.TryGetValue(deviceID, out result))
            {
                result = new KeyList();
                dictionary[deviceID] = result;
            }
            return result;
        }
        public Key ToKey(Interception.KeyStroke keyStroke)
        {
            ushort result = (ushort)keyStroke.Code;

            if ((keyStroke.State & Interception.KeyState.E0) != 0)
                result += 0x100;

            return (Key)result;
        }
        public Interception.KeyStroke ToKeyStroke(Key key, bool down)
        {
            Interception.KeyStroke result = new Interception.KeyStroke();

            if (!down)
                result.State = Interception.KeyState.Up;

            ushort code = (ushort)key;
            if (code >= 0x100)
            {
                code -= 0x100;
                result.State |= Interception.KeyState.E0;
            }
            result.Code = code;

            return result;
        }
        private bool showKeys = false;
        public void DriverUpdater()
        {
            DeviceID deviceID = Interception.WaitWithTimeout(context, 0);
            if (deviceID != 0)
            {
                currentDeviceID = deviceID;

                
            }
            Interception.Stroke stroke = new Interception.Stroke();

            while (Interception.Receive(context, deviceID, ref stroke, 1) > 0)
            {
                Key key = ToKey(stroke.Key);

                if (showKeys)
                    Console.WriteLine("Key: {0}; Scancode: 0x{1:X2}; State: {2}", key, stroke.Key.Code, stroke.Key.State);

                bool processed;

                KeyList deviceDownedKeys = GetOrCreateKeyList(downedKeys, deviceID);

                if (stroke.Key.State.IsKeyDown())
                {
                    if (!deviceDownedKeys.Contains(key))
                    {
                        deviceDownedKeys.Add(key);
                        processed = OnKeyDown(key, false);
                    }
                    else
                    {
                        processed = OnKeyDown(key, true);
                    }
                }
                else
                {
                    deviceDownedKeys.Remove(key);
                    processed = OnKeyUp(key);
                }

                if (!processed)
                    Interception.Send(context, deviceID, ref stroke, 1);
            }
        }

        public void Quit()
        {
            Interception.DestroyContext(context);
            foreach (Macros p in plugins)
            {
                UnLoadMacros(p);
            }
        }
        private readonly Dictionary<string, List<Macros>> registeredPluginsPluginChannels = new Dictionary<string, List<Macros>>();
        private readonly List<Macros> plugins = new List<Macros>();

        #region Системное
        public bool isDriverInstalled
        {
            get
            {
                if (File.Exists(Path.Combine(Environment.SystemDirectory, "drivers\\mouse.sys")) && File.Exists(Path.Combine(Environment.SystemDirectory, "drivers\\keyboard.sys")))
                {
                    return true;
                }
                else { return false; }
            }
        }
        public static bool isUsingMono
        {
            get
            {
                return Type.GetType("Mono.Runtime") != null;
            }
        }
        public void OnUpdate()
        {
            foreach (Macros bot in plugins.ToArray())
            {
                try
                {
                    bot.Update();
                }
                catch { }
            }
        }
        #endregion

        #region Получение и отправка данных от плагина
        public Action OnUnloadPlugin { set; get; }
        public Action<object> OnMacrosPostObject { set; get; }
        public Action<Macros> OnMacrosLoad { set; get; }
        public void OnMacrosPostObjectMethod(object ob)
        {
            if (OnMacrosPostObject != null)
            {
                OnMacrosPostObject(ob);
            }
        }

        #endregion

        #region Управление плагином
        public void LoadMacros(Macros b, bool init = true)
        {
            b.SetHandler(this);
            plugins.Add(b);
            if (init)
            {
                List<Macros> temp = new List<Macros>();
                temp.Add(b);
                //new Plugin[] { b }
                DispatchPluginEvent(bot => bot.Initialize(), temp);
                if (OnMacrosLoad != null)
                {
                    OnMacrosLoad(b);
                }
            }
        }

        private bool OnKeyDown(Key key, bool repeat)
        {
            foreach (Macros macros in plugins.ToArray())
            {
                try { return macros.OnKeyDown(key, repeat); }
                catch { return false; }
            }
            return false;
        }
        private bool OnKeyUp(Key key)
        {
            foreach (Macros macros in plugins.ToArray())
            {
                try { return macros.OnKeyUp(key); }
                catch { return false; }
            }
            return false;
        }

        public void MacrosPostObject(Macros m, object obj)
        {
            foreach (Macros macros in plugins.ToArray())
            {
                try { if (m == macros) { macros.ReceivedObject(obj); } }
                catch { }
            }
        }
        public void UnLoadMacros(Macros m)
        {
            plugins.RemoveAll(item => object.ReferenceEquals(item, m));

            var botRegistrations = registeredPluginsPluginChannels.Where(entry => entry.Value.Contains(m)).ToList();
            foreach (var entry in botRegistrations)
            {
                UnregisterPluginChannel(entry.Key, m);
            }
        }
        #endregion

        #region Регистрация плагинов
        private void DispatchPluginEvent(Action<Macros> action, IEnumerable<Macros> botList = null)
        {
            Macros[] selectedBots;

            if (botList != null)
            {
                selectedBots = botList.ToArray();
            }
            else
            {
                selectedBots = plugins.ToArray();
            }

            foreach (Macros bot in selectedBots)
            {
                try
                {
                    action(bot);
                    //Console.WriteLine("Выполнил: " + action.Method);
                }
                catch (Exception e)
                {
                    if (!(e is ThreadAbortException))
                    {
                        //Retrieve parent method name to determine which event caused the exception
                        System.Diagnostics.StackFrame frame = new System.Diagnostics.StackFrame(1);
                        System.Reflection.MethodBase method = frame.GetMethod();
                        string parentMethodName = method.Name;

                        //Display a meaningful error message to help debugging the ChatBot
                        Console.WriteLine(parentMethodName + ": Got error from " + bot.ToString() + ": " + e.ToString());
                    }
                    else throw;
                }
            }
        }

        public void UnregisterPluginChannel(string channel, Macros bot)
        {
            if (registeredPluginsPluginChannels.ContainsKey(channel))
            {
                List<Macros> registeredBots = registeredPluginsPluginChannels[channel];
                registeredBots.RemoveAll(item => object.ReferenceEquals(item, bot));
                if (registeredBots.Count == 0)
                {
                    registeredPluginsPluginChannels.Remove(channel);
                }
            }
        }
        #endregion
    }
}
