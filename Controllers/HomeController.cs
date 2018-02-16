using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ElectronNET.API;
using Microsoft.AspNetCore.Mvc;

namespace CivLeagueJP.Client.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (HybridSupport.IsElectronActive)
            {
                Electron.IpcMain.On("StartCiv", async (args) =>
                {
                    Models.Civ6.MainProgram.StartCiv();
                });
                Electron.IpcMain.On("PopulateClient", async (args) =>
                {
                    Models.Civ6.MainProgram.PopulateClient();
                });
                Electron.IpcMain.On("SendQuery", async (text) =>
                {
                    Models.Civ6.MainProgram.SendQuery(text.ToString());
                });
            }

            return View();
        }
    }
}