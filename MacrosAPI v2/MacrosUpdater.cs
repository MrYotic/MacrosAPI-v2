using System;
using System.Diagnostics;
using System.Threading;

namespace MacrosAPI_v2
{
    public class MacrosUpdater
    {
        Thread netRead = null;

        #region Системное
        MacrosManager _handler;
        public void SetHandler(MacrosManager _handler)
        {
            this._handler = _handler;
        }
        public void Stop()
        {
            if (netRead != null)
            {
                netRead.Abort();
                netRead = null;
            }
        }
        public void StartUpdater()
        {
            if (netRead == null)
            {
                netRead = new Thread(new ThreadStart(Updater));
                netRead.Name = "PacketHandler";
                netRead.Start();
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
                    keepUpdating = Update();
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
        private bool Update()
        {
            try
            {
                _handler.DriverUpdater();
                _handler.OnUpdate();

            }
            catch (System.IO.IOException) { return false; }
            catch (NullReferenceException) { return false; }
            return true;
        }
        #endregion
    }
}
