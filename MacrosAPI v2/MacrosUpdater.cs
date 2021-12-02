using System;
using System.Diagnostics;
using System.Threading;

namespace MacrosAPI_v2
{
    public class MacrosUpdater
    {
        Thread updater = null;
        Thread driverupdater = null;

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

            if (driverupdater != null)
            {
                driverupdater.Abort();
                driverupdater = null;
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

            if (driverupdater == null)
            {
                driverupdater = new Thread(new ThreadStart(DriverUpdater));
                driverupdater.Name = "DriverUpdater";
                driverupdater.Start();
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
        private void DriverUpdater()
        {
            try
            {
                bool keepUpdating = true;
                Stopwatch stopWatch = new Stopwatch();
                while (keepUpdating)
                {
                    stopWatch.Start();
                    try { _handler.DriverUpdater(); } catch { }
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
