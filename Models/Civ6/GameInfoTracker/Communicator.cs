using CivLeagueJP.Client.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CivLeagueJP.Client.Models.Civ6
{
    class Communicator
    {
        private int listener = -1;

        public Communicator()
        {
            listener = Client.Instance.AddListener(new Client.Listener(OnCommunicatorCreatd));
        }

        private void OnCommunicatorCreatd(List<string> response)
        {
            Debug.WriteLine("Communicator Loaded");
        }

        internal void SendRequestQuery(string str)
        {
            Client.Instance.Request(str, listener);
        }
    }
}
