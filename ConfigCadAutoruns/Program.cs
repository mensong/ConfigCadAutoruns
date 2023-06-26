using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace ConfigCadAutoruns
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            string iniFile = "";

            List<string> args = System.Environment.GetCommandLineArgs().ToList();

            int f = args.IndexOf("-f") != -1 ? args.IndexOf("-f") 
                : args.IndexOf("-F") != -1 ? args.IndexOf("-F") 
                : args.IndexOf("/f") != -1 ? args.IndexOf("/f") 
                : args.IndexOf("/F") != -1 ? args.IndexOf("/F") 
                : -1;
            if (f != -1 && args.Count > f + 1)
            {
                iniFile = args[f + 1];
            }
            else
            {
                iniFile = System.IO.Path.GetDirectoryName(
                    Process.GetCurrentProcess().MainModule.FileName) + "\\autoloadconfig.ini";
            }

            int i = args.IndexOf("-i") != -1 ? args.IndexOf("-i") 
                : args.IndexOf("-I") != -1 ? args.IndexOf("-I") 
                : args.IndexOf("/i") != -1 ? args.IndexOf("/i") 
                : args.IndexOf("/I") != -1 ? args.IndexOf("/I") 
                : -1;
            int u = args.IndexOf("-u") != -1 ? args.IndexOf("-u") 
                : args.IndexOf("-U") != -1 ? args.IndexOf("-U") 
                : args.IndexOf("/u") != -1 ? args.IndexOf("/u") 
                : args.IndexOf("/U") != -1 ? args.IndexOf("/U") 
                : -1;

            if (i == -1 && u == -1)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                var win = new MainForm();
                win.InitFile = iniFile;
                Application.Run(win);
            }
            else if (i != -1)
            {
                if (RegistrySetup.InstallByIni(iniFile))
                    System.Console.WriteLine("OK");
                else
                    System.Console.WriteLine("FAILD");
            }
            else if (u != -1)
            {
                if (RegistrySetup.UninstallByIni(iniFile))
                    System.Console.WriteLine("OK");
                else
                    System.Console.WriteLine("FAILD");
            }
        }
    }
}
