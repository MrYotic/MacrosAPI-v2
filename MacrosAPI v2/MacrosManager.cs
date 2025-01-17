﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using KeyList = System.Collections.Generic.List<Key>;
namespace MacrosAPI_v2
{
    public class MacrosManager
    {
        private readonly List<Macros> plugins = new List<Macros>();
        private readonly Dictionary<string, List<Macros>> registeredPluginsPluginChannels = new Dictionary<string, List<Macros>>();
        public Dictionary<int, KeyList> downedKeys = new Dictionary<int, KeyList>();
        public IntPtr keyboard;
        public int keyboardDeviceID = 1;
        public IntPtr mouse;
        public int mouseDeviceID = 1;
        private readonly MacrosUpdater updater;
        public MacrosManager(MacrosUpdater updater)
        {
            keyboard = Interception.CreateContext();
            Interception.SetFilter(keyboard, Interception.IsKeyboard, Interception.Filter.All);
            mouse = Interception.CreateContext();
            Interception.SetFilter(mouse, Interception.IsMouse, Interception.Filter.All);
            this.updater = updater;
            updater.SetHandler(this);
            updater.StartUpdater();
        }
        private KeyList GetOrCreateKeyList(Dictionary<int, KeyList> dictionary, int deviceID)
        {
            KeyList result;
            if (!dictionary.TryGetValue(deviceID, out result))
                dictionary[deviceID] = result = new KeyList();
            return result;
        }
        public Key ToKey(Interception.KeyStroke keyStroke)
        {
            var result = keyStroke.Code;
            if ((keyStroke.State & Interception.KeyState.E0) != 0)
                result += 0x100;
            return (Key)result;
        }
        public Interception.KeyStroke ToKeyStroke(Key key, bool down)
        {
            var result = new Interception.KeyStroke();
            if (!down)
                result.State = Interception.KeyState.Up;
            var code = (ushort) key;
            if (code >= 0x100)
            {
                code -= 0x100;
                result.State |= Interception.KeyState.E0;
            }
            result.Code = code;
            return result;
        }
        public void DriverUpdaterMouse()
        {
            var mousedeviceID = Interception.WaitWithTimeout(mouse, 0);
            switch (mousedeviceID)
            {
                case 0: break;
                default:
                    mouseDeviceID = mousedeviceID;
                    break;
            }
            var stroke = new Interception.Stroke();
            while (Interception.Receive(mouse, mousedeviceID, ref stroke, 1) > 0)
            {
                var processed = false;
                switch (stroke.Mouse.State)
                {
                    case Interception.MouseState.LeftButtonDown:
                        processed = OnMouseDown(MouseKey.Left);
                        break;
                    case Interception.MouseState.RightButtonDown:
                        processed = OnMouseDown(MouseKey.Right);
                        break;
                    case Interception.MouseState.MiddleButtonDown:
                        processed = OnMouseDown(MouseKey.Middle);
                        break;
                    case Interception.MouseState.Button4Down:
                        processed = OnMouseDown(MouseKey.Button1);
                        break;
                    case Interception.MouseState.Button5Down:
                        processed = OnMouseDown(MouseKey.Button2);
                        break;
                    case Interception.MouseState.LeftButtonUp:
                        processed = OnMouseUp(MouseKey.Left);
                        break;
                    case Interception.MouseState.RightButtonUp:
                        processed = OnMouseUp(MouseKey.Right);
                        break;
                    case Interception.MouseState.MiddleButtonUp:
                        processed = OnMouseUp(MouseKey.Middle);
                        break;
                    case Interception.MouseState.Button4Up:
                        processed = OnMouseUp(MouseKey.Button1);
                        break;
                    case Interception.MouseState.Button5Up:
                        processed = OnMouseUp(MouseKey.Button2);
                        break;
                    case Interception.MouseState.Wheel:
                        processed = OnMouseWheel(stroke.Mouse.Rolling);
                        break;
                }
                processed = OnMouseMove(stroke.Mouse.X, stroke.Mouse.Y);
                if (!processed)
                    Interception.Send(mouse, mousedeviceID, ref stroke, 1);
            }
        }
        public void DriverUpdaterKeyBoard()
        {
            var keyboardDeviceIDdeviceID = Interception.WaitWithTimeout(keyboard, 0);
            switch (keyboardDeviceIDdeviceID)
            {
                case 0: break;
                default:
                    keyboardDeviceID = keyboardDeviceIDdeviceID;
                    break;
            }
            var stroke = new Interception.Stroke();
            while (Interception.Receive(keyboard, keyboardDeviceIDdeviceID, ref stroke, 1) > 0)
            {
                var key = ToKey(stroke.Key);
                var processed = false;
                var deviceDownedKeys = GetOrCreateKeyList(downedKeys, keyboardDeviceIDdeviceID);
                switch (stroke.Key.State.IsKeyDown())
                {
                    case true:
                        switch (!deviceDownedKeys.Contains(key))
                        {
                            case true:
                                deviceDownedKeys.Add(key);
                                processed = OnKeyDown(key, false);
                                break;
                            case false:
                                processed = OnKeyDown(key, true);
                                break;
                        }
                        break;
                    case false:
                        deviceDownedKeys.Remove(key);
                        processed = OnKeyUp(key);
                        break;
                }
                if (!processed)
                    Interception.Send(keyboard, keyboardDeviceIDdeviceID, ref stroke, 1);
            }
        }
        public void Quit()
        {
            if (updater != null) updater.Stop();
            Interception.DestroyContext(keyboard);
            Interception.DestroyContext(mouse);
            plugins.ForEach(x => UnLoadMacros(x));
        }
        #region Системное
        public static bool isUsingMono => Type.GetType("Mono.Runtime") != null;
        public void OnUpdate() => plugins.ForEach(x => x.Update());
        #endregion
        #region Получение и отправка данных от плагина
        public Action OnUnloadPlugin { set; get; }
        public Action<object> OnMacrosPostObject { set; get; }
        public Action<Macros> OnMacrosLoad { set; get; }
        public void OnMacrosPostObjectMethod(object ob)
        {
            if (OnMacrosPostObject != null) OnMacrosPostObject(ob);
        }
        #endregion
        #region Управление плагином
        public void LoadMacros(Macros macros, bool init = true)
        {
            macros.SetHandler(this);
            plugins.Add(macros);
            if (init)
            {
                var temp = new List<Macros>();
                temp.Add(macros);
                DispatchPluginEvent(bot => bot.Initialize(), temp);
                if (OnMacrosLoad != null) OnMacrosLoad(macros);
            }
        }
        private bool OnMouseMove(int x, int y)
        {
            foreach (var macros in plugins.ToArray())
                try { return macros.OnMouseMove(x, y); } catch { return false; }
            return false;
        }
        private bool OnMouseDown(MouseKey key)
        {
            foreach (var macros in plugins.ToArray())
                try { return macros.OnMouseDown(key); } catch { return false; }
            return false;
        }
        private bool OnMouseUp(MouseKey key)
        {
            foreach (var macros in plugins.ToArray())
                try { return macros.OnMouseUp(key); } catch { return false; }
            return false;
        }
        private bool OnMouseWheel(int rolling)
        {
            foreach (var macros in plugins.ToArray())
                try { return macros.OnMouseWheel(rolling); } catch { return false; }
            return false;
        }
        private bool OnKeyDown(Key key, bool repeat)
        {
            foreach (var macros in plugins.ToArray())
                try { return macros.OnKeyDown(key, repeat); } catch { return false; }
            return false;
        }
        private bool OnKeyUp(Key key)
        {
            foreach (var macros in plugins.ToArray())
                try { return macros.OnKeyUp(key); } catch { return false; }
            return false;
        }
        public void MacrosPostObject(Macros macros, object obj)
        {
            foreach (var smacros in plugins.ToArray())
                try { if (macros == smacros) smacros.ReceivedObject(obj); } catch { }
        }
        public void UnLoadMacros(Macros m)
        {
            plugins.RemoveAll(item => ReferenceEquals(item, m));
            var botRegistrations = registeredPluginsPluginChannels.Where(entry => entry.Value.Contains(m)).ToList();
            foreach (var entry in botRegistrations) UnregisterPluginChannel(entry.Key, m);
        }
        #endregion
        #region Регистрация плагинов
        private void DispatchPluginEvent(Action<Macros> action, IEnumerable<Macros> botList = null)
        {
            Macros[] selectedBots;
            if (botList != null) selectedBots = botList.ToArray();
            else selectedBots = plugins.ToArray();
            foreach (var bot in selectedBots)
                try { action(bot); }
                catch (Exception e)
                {
                    if (!(e is ThreadAbortException))
                    {
                        var frame = new StackFrame(1);
                        var method = frame.GetMethod();
                        var parentMethodName = method.Name;
                        Console.WriteLine(parentMethodName + ": Got error from " + bot + ": " + e);
                    }
                    else {throw;}
                }
        }
        public void UnregisterPluginChannel(string channel, Macros bot)
        {
            if (registeredPluginsPluginChannels.ContainsKey(channel))
            {
                var registeredBots = registeredPluginsPluginChannels[channel];
                registeredBots.RemoveAll(x => ReferenceEquals(x, bot));
                if (registeredBots.Count == 0) registeredPluginsPluginChannels.Remove(channel);
            }
        }
        #endregion
    }
}