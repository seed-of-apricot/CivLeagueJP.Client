using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CivLeagueJP.Client.Models.Civ6
{
    class MessageDecoder
    {
        internal List<City> CityInfoDecoder(byte[] cityList, byte[] statusList, byte[] buildingList, byte[] citizenList)
        {
            string str = Encoding.ASCII.GetString(cityList);
            List<string> strList = str.Split('\0').ToList();
            strList.RemoveAll(t => !(t.Contains("NAME")));
            List<City> list = new List<City>();
            for (int i = 0; i < strList.Count; i++)
            {
                var item = strList[i];
                City info = new City();
                info.Id = i;
                info.PlayerId = int.Parse(item.Split(';')[0]);
                info.DisplayName = item.Split(';')[1];
                info.Location = new int[] { int.Parse(item.Split(';')[2]), int.Parse(item.Split(';')[3]) };
                list.Add(info);
            }
            return list;
        }
    }
}
