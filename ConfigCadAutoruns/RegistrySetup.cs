using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ConfigCadAutoruns
{
    public class RegistrySetup
    {
        public enum RVER
        {
            R190,
            R191,
            R200,
            R201,
            R210,
            R220,
            R230,
            R231,
        }

        private static Dictionary<RVER, KeyValuePair<string, string>> rverTo804 = 
            new Dictionary<RVER, KeyValuePair<string, string>>()
        {
            { RVER.R190, new KeyValuePair<string, string>("R19.0", "ACAD-B001:804") },
            { RVER.R191, new KeyValuePair<string, string>("R19.1", "ACAD-D001:804") },
            { RVER.R200, new KeyValuePair<string, string>("R20.0", "ACAD-E001:804") },
            { RVER.R201, new KeyValuePair<string, string>("R20.1", "ACAD-F001:804") },
            { RVER.R210, new KeyValuePair<string, string>("R21.0", "ACAD-0001:804") },
            { RVER.R220, new KeyValuePair<string, string>("R22.0", "ACAD-1001:804") },
            { RVER.R230, new KeyValuePair<string, string>("R23.0", "ACAD-2001:804") },
            { RVER.R231, new KeyValuePair<string, string>("R23.1", "ACAD-3001:804") },
        };


        public static bool InstallAutoRun(RVER rver, string businessKey, string pluginPath, 
            string pluginDesc="", int loadCtrls=2, int managed=1)
        {
            string subKey = string.Format(@"SOFTWARE\Autodesk\AutoCAD\{0}\{1}\Applications", 
                rverTo804[rver].Key, rverTo804[rver].Value);

            try
            {
                RegistryKey appKey = Registry.LocalMachine.OpenSubKey(subKey, true);
                if (appKey == null)
                    return false;

                RegistryKey buzKey = appKey.CreateSubKey(businessKey);
                if (buzKey == null)
                    return false;

                buzKey.SetValue("DESCRIPTION", pluginDesc, RegistryValueKind.String);
                buzKey.SetValue("LOADCTRLS", 2, RegistryValueKind.DWord);
                buzKey.SetValue("LOADER", pluginPath, RegistryValueKind.String);
                buzKey.SetValue("MANAGED", 1, RegistryValueKind.DWord);
            }
            catch 
            {
                return false;
            }

            return true;
        }

        public static bool InstallSupportPath(RVER rver, string dirPath)
        {
            string subKey = string.Format(@"Software\Autodesk\AutoCAD\{0}\{1}\Profiles", 
                rverTo804[rver].Key, rverTo804[rver].Value);

            try
            {
                using (RegistryKey profilesKey = Registry.CurrentUser.OpenSubKey(subKey, false))
                {
                    if (profilesKey == null)
                        return false;

                    //string profilesName = profilesKey.GetValue(null) as string;

                    string[] subProfilesKeys = profilesKey.GetSubKeyNames();
                    foreach (string profilesName in subProfilesKeys)
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(profilesName))
                                continue;

                            using (var generalKey = profilesKey.OpenSubKey(profilesName + @"\General", true))
                            {
                                if (generalKey == null)
                                    continue;

                                var supportPathsStr = generalKey.GetValue("ACAD") as string;
                                var supportPaths = supportPathsStr.Split(new char[] { ';' }, 
                                    StringSplitOptions.RemoveEmptyEntries).ToList();
                                bool bHad = false;
                                foreach (var path in supportPaths)
                                {
                                    if (IsSamePath(path, dirPath))
                                    {
                                        bHad = true;
                                        break;
                                    }
                                }
                                if (bHad)
                                    continue;

                                supportPaths.Add(dirPath);
                                supportPathsStr = string.Join(";", supportPaths.ToArray());
                                generalKey.SetValue("ACAD", supportPathsStr, RegistryValueKind.ExpandString);
                            }
                        }
                        catch
                        {
                        }
                    }
                }                
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static bool InstallTrustedPath(RVER rver, string dirPath)
        {
            string subKey = string.Format(@"Software\Autodesk\AutoCAD\{0}\{1}\Profiles", 
                rverTo804[rver].Key, rverTo804[rver].Value);

            try
            {
                using (RegistryKey profilesKey = Registry.CurrentUser.OpenSubKey(subKey, false))
                {
                    if (profilesKey == null)
                        return false;

                    //string profilesName = profilesKey.GetValue(null) as string;

                    string[] subProfilesKeys = profilesKey.GetSubKeyNames();
                    foreach (string profilesName in subProfilesKeys)
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(profilesName))
                                continue;

                            using (var variablesKey = profilesKey.OpenSubKey(profilesName + @"\Variables", true))
                            {
                                if (variablesKey == null)
                                    continue;

                                var trustedPathsStr = variablesKey.GetValue("TRUSTEDPATHS") as string;
                                var trustedPaths = trustedPathsStr.Split(new char[] { ';' }, 
                                    StringSplitOptions.RemoveEmptyEntries).ToList();
                                bool bHad = false;
                                foreach (var path in trustedPaths)
                                {
                                    if (IsSamePath(path, dirPath))
                                    {
                                        bHad = true;
                                        break;
                                    }
                                }
                                if (bHad)
                                    continue;

                                trustedPaths.Add(dirPath);
                                trustedPathsStr = string.Join(";", trustedPaths.ToArray());
                                variablesKey.SetValue("TRUSTEDPATHS", trustedPathsStr, RegistryValueKind.String);
                            }
                        }
                        catch
                        {

                        }
                    }
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static bool UninstallAutoRun(RVER rver, string businessKey)
        {
            string subKey = string.Format(@"SOFTWARE\Autodesk\AutoCAD\{0}\{1}\Applications", 
                rverTo804[rver].Key, rverTo804[rver].Value);
            RegistryKey appKey = Registry.LocalMachine.OpenSubKey(subKey, true);
            if (appKey == null)
                return false;

            try
            {
                appKey.DeleteSubKey(businessKey);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static bool UninstallSupportPath(RVER rver, string dirPath)
        {
            string subKey = string.Format(@"Software\Autodesk\AutoCAD\{0}\{1}\Profiles", 
                rverTo804[rver].Key, rverTo804[rver].Value);

            try
            {
                using (RegistryKey profilesKey = Registry.CurrentUser.OpenSubKey(subKey, false))
                {
                    if (profilesKey == null)
                        return false;

                    //string profilesName = profilesKey.GetValue(null) as string;

                    string[] subProfilesKeys = profilesKey.GetSubKeyNames();
                    foreach (string profilesName in subProfilesKeys)
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(profilesName))
                                continue;

                            using (var generalKey = profilesKey.OpenSubKey(profilesName + @"\General", true))
                            {
                                if (generalKey == null)
                                    continue;

                                var supportPathsStr = generalKey.GetValue("ACAD") as string;
                                var supportPaths = supportPathsStr.Split(new char[] { ';' }, 
                                    StringSplitOptions.RemoveEmptyEntries).ToList();
                                var newSupportPaths = supportPaths.ToList();
                                for (int i = 0; i < supportPaths.Count; ++i)
                                {
                                    if (IsSamePath(supportPaths[i], dirPath))
                                    {
                                        newSupportPaths.RemoveAt(i);
                                    }
                                }

                                if (newSupportPaths.Count < supportPaths.Count)
                                {
                                    supportPathsStr = string.Join(";", newSupportPaths.ToArray());
                                    generalKey.SetValue("ACAD", supportPathsStr, RegistryValueKind.ExpandString);
                                }
                            }                            
                        }
                        catch
                        {
                        }
                    }
                }                      
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static bool UninstallTrustedPath(RVER rver, string dirPath)
        {
            string subKey = string.Format(@"Software\Autodesk\AutoCAD\{0}\{1}\Profiles", 
                rverTo804[rver].Key, rverTo804[rver].Value);

            try
            {
                using (RegistryKey profilesKey = Registry.CurrentUser.OpenSubKey(subKey, false))
                {
                    if (profilesKey == null)
                        return false;

                    //string profilesName = profilesKey.GetValue(null) as string;

                    string[] subProfilesKeys = profilesKey.GetSubKeyNames();
                    foreach (string profilesName in subProfilesKeys)
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(profilesName))
                                continue;

                            using (var variablesKey = profilesKey.OpenSubKey(profilesName + @"\Variables", true))
                            {
                                if (variablesKey == null)
                                    continue;

                                var trustedPathsStr = variablesKey.GetValue("TRUSTEDPATHS") as string;
                                var trustedPaths = trustedPathsStr.Split(new char[] { ';' }, 
                                    StringSplitOptions.RemoveEmptyEntries).ToList();
                                var newTrustedPaths = trustedPaths.ToList();
                                for (int i = 0; i < trustedPaths.Count; ++i)
                                {
                                    if (IsSamePath(trustedPaths[i], dirPath))
                                    {
                                        newTrustedPaths.RemoveAt(i);
                                    }
                                }

                                if (newTrustedPaths.Count < trustedPaths.Count)
                                {
                                    trustedPathsStr = string.Join(";", newTrustedPaths.ToArray());
                                    variablesKey.SetValue("TRUSTEDPATHS", trustedPathsStr, RegistryValueKind.String);
                                }
                            }           
                        }
                        catch
                        {
                        }
                    }
                }                
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static bool InstallByIni(string iniFile)
        {            
            string buzName = IniHelper.ReadString("AUTOLOAD", "Name", "", iniFile);//"BGY_SRD"
            string LOADER = IniHelper.ReadString("AUTOLOAD", "LOADER", "", iniFile);
            if (string.IsNullOrEmpty(buzName) || string.IsNullOrEmpty(LOADER))
            {
                return false;
            }
            string DESCRIPTION = IniHelper.ReadString("AUTOLOAD", "DESCRIPTION", "", iniFile);
            int LOADCTRLS = IniHelper.ReadInt("AUTOLOAD", "LOADCTRLS", 2, iniFile);
            int MANAGED = IniHelper.ReadInt("AUTOLOAD", "MANAGED", 1, iniFile);

            if (!LOADER.Contains(":"))
            {//相对路径
                string d = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
                LOADER = Path.Combine(d, Path.GetFileName(LOADER));
            }

            string dirPath = Path.GetDirectoryName(LOADER);

            //注册所有版本的AutoCAD
            foreach (RegistrySetup.RVER ver in Enum.GetValues(typeof(RegistrySetup.RVER)))
            {
                RegistrySetup.InstallAutoRun(ver, buzName, LOADER, DESCRIPTION, LOADCTRLS, MANAGED);
                RegistrySetup.InstallSupportPath(ver, dirPath);
                RegistrySetup.InstallTrustedPath(ver, dirPath);
            }

            //RegistrySetup.InstallAutoRun(RegistrySetup.RVER.R190, buzName, LOADER, DESCRIPTION, LOADCTRLS, MANAGED);
            //RegistrySetup.InstallAutoRun(RegistrySetup.RVER.R191, buzName, LOADER, DESCRIPTION, LOADCTRLS, MANAGED);
            //RegistrySetup.InstallAutoRun(RegistrySetup.RVER.R200, buzName, LOADER, DESCRIPTION, LOADCTRLS, MANAGED);
            //RegistrySetup.InstallAutoRun(RegistrySetup.RVER.R201, buzName, LOADER, DESCRIPTION, LOADCTRLS, MANAGED);
            //RegistrySetup.InstallAutoRun(RegistrySetup.RVER.R210, buzName, LOADER, DESCRIPTION, LOADCTRLS, MANAGED);
            //RegistrySetup.InstallAutoRun(RegistrySetup.RVER.R220, buzName, LOADER, DESCRIPTION, LOADCTRLS, MANAGED);
            //RegistrySetup.InstallAutoRun(RegistrySetup.RVER.R230, buzName, LOADER, DESCRIPTION, LOADCTRLS, MANAGED);
            //RegistrySetup.InstallAutoRun(RegistrySetup.RVER.R231, buzName, LOADER, DESCRIPTION, LOADCTRLS, MANAGED);

            //RegistrySetup.InstallSupportPath(RegistrySetup.RVER.R190, dirPath);
            //RegistrySetup.InstallSupportPath(RegistrySetup.RVER.R191, dirPath);
            //RegistrySetup.InstallSupportPath(RegistrySetup.RVER.R200, dirPath);
            //RegistrySetup.InstallSupportPath(RegistrySetup.RVER.R201, dirPath);
            //RegistrySetup.InstallSupportPath(RegistrySetup.RVER.R210, dirPath);
            //RegistrySetup.InstallSupportPath(RegistrySetup.RVER.R220, dirPath);
            //RegistrySetup.InstallSupportPath(RegistrySetup.RVER.R230, dirPath);
            //RegistrySetup.InstallSupportPath(RegistrySetup.RVER.R231, dirPath);

            //RegistrySetup.InstallTrustedPath(RegistrySetup.RVER.R190, dirPath);
            //RegistrySetup.InstallTrustedPath(RegistrySetup.RVER.R191, dirPath);
            //RegistrySetup.InstallTrustedPath(RegistrySetup.RVER.R200, dirPath);
            //RegistrySetup.InstallTrustedPath(RegistrySetup.RVER.R201, dirPath);
            //RegistrySetup.InstallTrustedPath(RegistrySetup.RVER.R210, dirPath);
            //RegistrySetup.InstallTrustedPath(RegistrySetup.RVER.R220, dirPath);
            //RegistrySetup.InstallTrustedPath(RegistrySetup.RVER.R230, dirPath);
            //RegistrySetup.InstallTrustedPath(RegistrySetup.RVER.R231, dirPath);

            return true;
        }

        public static bool UninstallByIni(string iniFile)
        {
            string buzName = IniHelper.ReadString("AUTOLOAD", "Name", "", iniFile);//"BGY_SRD"
            string LOADER = IniHelper.ReadString("AUTOLOAD", "LOADER", "", iniFile);
            if (string.IsNullOrEmpty(buzName) || string.IsNullOrEmpty(LOADER))
            {
                return false;
            }

            string dirPath = Path.GetDirectoryName(LOADER);

            //反注册所有版本的AutoCAD
            foreach (RegistrySetup.RVER ver in Enum.GetValues(typeof(RegistrySetup.RVER)))
            {
                RegistrySetup.UninstallAutoRun(ver, buzName);
                RegistrySetup.UninstallSupportPath(ver, dirPath);
                RegistrySetup.UninstallTrustedPath(ver, dirPath);
            }

            //RegistrySetup.UninstallAutoRun(RegistrySetup.RVER.R190, buzName);
            //RegistrySetup.UninstallAutoRun(RegistrySetup.RVER.R191, buzName);
            //RegistrySetup.UninstallAutoRun(RegistrySetup.RVER.R200, buzName);
            //RegistrySetup.UninstallAutoRun(RegistrySetup.RVER.R201, buzName);
            //RegistrySetup.UninstallAutoRun(RegistrySetup.RVER.R210, buzName);
            //RegistrySetup.UninstallAutoRun(RegistrySetup.RVER.R220, buzName);
            //RegistrySetup.UninstallAutoRun(RegistrySetup.RVER.R230, buzName);
            //RegistrySetup.UninstallAutoRun(RegistrySetup.RVER.R231, buzName);

            //RegistrySetup.UninstallSupportPath(RegistrySetup.RVER.R190, dirPath);
            //RegistrySetup.UninstallSupportPath(RegistrySetup.RVER.R191, dirPath);
            //RegistrySetup.UninstallSupportPath(RegistrySetup.RVER.R200, dirPath);
            //RegistrySetup.UninstallSupportPath(RegistrySetup.RVER.R201, dirPath);
            //RegistrySetup.UninstallSupportPath(RegistrySetup.RVER.R210, dirPath);
            //RegistrySetup.UninstallSupportPath(RegistrySetup.RVER.R220, dirPath);
            //RegistrySetup.UninstallSupportPath(RegistrySetup.RVER.R230, dirPath);
            //RegistrySetup.UninstallSupportPath(RegistrySetup.RVER.R231, dirPath);
            
            //RegistrySetup.UninstallTrustedPath(RegistrySetup.RVER.R190, dirPath);
            //RegistrySetup.UninstallTrustedPath(RegistrySetup.RVER.R191, dirPath);
            //RegistrySetup.UninstallTrustedPath(RegistrySetup.RVER.R200, dirPath);
            //RegistrySetup.UninstallTrustedPath(RegistrySetup.RVER.R201, dirPath);
            //RegistrySetup.UninstallTrustedPath(RegistrySetup.RVER.R210, dirPath);
            //RegistrySetup.UninstallTrustedPath(RegistrySetup.RVER.R220, dirPath);
            //RegistrySetup.UninstallTrustedPath(RegistrySetup.RVER.R230, dirPath);
            //RegistrySetup.UninstallTrustedPath(RegistrySetup.RVER.R231, dirPath);

            return true;
        }

        private static bool IsSamePath(string path1, string path2)
        {
            //忽略掉目录与路径的对比
            path1 = path1.TrimEnd('\\').TrimEnd('/');
            path2 = path2.TrimEnd('\\').TrimEnd('/');

            var uri1 = new Uri(path1);
            var uri2 = new Uri(path2);
            return uri1.Equals(uri2);
        }

    }
}
