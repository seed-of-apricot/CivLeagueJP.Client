using ElectronNET.API;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.IO;

namespace CivLeagueJP.Client.Models.Civ6
{
    class MainProgram
    {
        public static void PopulateClient()
        {
            try
            {
                //Request demo panel to communicate city and unit info
                Client.Init();
                Client.Instance.OpenConnection();
                Communicator communicator = new Communicator();
                communicator.SendRequestQuery("LOAD:UI:CLClient");
            }
            catch (Exception)
            {
                throw;
            }
        }


        internal static void SendQuery(string str)
        {
            Communicator communicator = new Communicator();
            communicator.SendRequestQuery("function()\r\nreturn GetCityInfo()\r\nend");
        }

        internal static void StartCiv()
        {
            string path = FindAppPath("Sid Meier's Civilization VI");
            if (path == null)
            {
                return;
            }
            if (File.Exists(path + "\\CivilizationVI.exe"))
            {
                Process Civ6 = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        WorkingDirectory = path,
                        FileName = path + "\\CivilizationVI.exe",
                        Arguments = string.Empty
                    }
                };
                Civ6.Start();
            }
        }

        private static string FindAppPath(string name)
        {
            return "D:\\SteamLibrary\\steamapps\\common\\Sid Meier's Civilization VI\\Base\\Binaries\\Win64Steam";
            //Contract.Requires(!string.IsNullOrEmpty(name));
            //const string keyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            //using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath))
            //{
            //    var appList = key.GetSubKeyNames();
            //    var appInfo = appList.Select(t => key.OpenSubKey(t)).Select(t => new { appName = t.GetValue("DisplayName") as string, appPath = t.GetValue("InstallLocation") as string }).ToList();

            //    var app = appInfo.FindAll(t => t.appName != null && t.appName.Contains(name) && !string.IsNullOrEmpty(t.appPath));

            //    return (app.Count > 0) ? app[0].appPath : null;
            //}
            //
        }
    }
}
