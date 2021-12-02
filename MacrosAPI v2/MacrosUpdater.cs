using System;
using System.Diagnostics;
using System.Threading;

namespace MacrosAPI_v2
{
    public class MacrosUpdater
    {
        Thread updater = null;
        Thread driverupdaterkeyboard = null;
        Thread driverupdatermouse = null;

        #region Системное
        MacrosManager _handler;
        public void SetHandler(MacrosManager _handler)
        {
            this._handler = _handler;
        }
        public void Stop()
        {
            if (updater != null)
            {
                updater.Abort();
                updater = null;
            }

            if (driverupdaterkeyboard != null)
            {
                driverupdaterkeyboard.Abort();
                driverupdaterkeyboard = null;
            }

            if (driverupdatermouse != null)
            {
                driverupdatermouse.Abort();
                driverupdatermouse = null;
            }
        }
        public void StartUpdater()
        {
            if (updater == null)
            {
                updater = new Thread(new ThreadStart(Updater));
                updater.Name = "Updater";
                updater.Start();
            }

            if (driverupdaterkeyboard == null)
            {
                driverupdaterkeyboard = new Thread(new ThreadStart(DriverUpdaterKB));
                driverupdaterkeyboard.Name = "DriverUpdaterKB";
                driverupdaterkeyboard.Start();
            }

            if (driverupdatermouse == null)
            {
                driverupdatermouse = new Thread(new ThreadStart(DriverUpdaterMS));
                driverupdatermouse.Name = "DriverUpdaterMS";
                driverupdatermouse.Start();
            }
        }
        private void Updater()
        {
            try
            {
                bool keepUpdating = true;
                Stopwatch stopWatch = new Stopwatch();
                while (keepUpdating)
                {
                    stopWatch.Start();
                    try { _handler.OnUpdate(); } catch { }
                    stopWatch.Stop();
                    int elapsed = stopWatch.Elapsed.Milliseconds;
                    stopWatch.Reset();
                    if (elapsed < 1)
                    {
                        Thread.Sleep(1 - elapsed);
                    }
                }
            }
            catch (System.IO.IOException) { }
            catch (ObjectDisposedException) { }
        }
        private void DriverUpdaterKB()
        {
            try
            {
                bool keepUpdating = true;
                Stopwatch stopWatch = new Stopwatch();
                while (keepUpdating)
                {
                    stopWatch.Start();
                    _handler.DriverUpdaterKeyBoard();
                    stopWatch.Stop();
                    int elapsed = stopWatch.Elapsed.Milliseconds;
                    stopWatch.Reset();
                    if (elapsed < 1)
                    {
                        Thread.Sleep(1 - elapsed);
                    }
                }
            }
            catch (System.IO.IOException) { }
            catch (ObjectDisposedException) { }
        }

        private void DriverUpdaterMS()
        {
            try
            {
                bool keepUpdating = true;
                Stopwatch stopWatch = new Stopwatch();
                while (keepUpdating)
                {
                    stopWatch.Start();
                    _handler.DriverUpdaterMouse();
                    stopWatch.Stop();
                    int elapsed = stopWatch.Elapsed.Milliseconds;
                    stopWatch.Reset();
                    if (elapsed < 1)
                    {
                        Thread.Sleep(1 - elapsed);
                    }
                }
            }
            catch (System.IO.IOException) { }
            catch (ObjectDisposedException) { }
        }
        #endregion
    }
}
