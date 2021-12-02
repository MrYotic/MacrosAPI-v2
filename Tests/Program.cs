using MacrosAPI_v2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests.Macro;

namespace Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            /* Создание API */
            MacrosUpdater updater = new MacrosUpdater();
            MacrosManager manager = new MacrosManager(updater);

            manager.LoadMacros(new NinjaBridge());
        }
    }

    
}
