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
    public class City
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }
        public int[] Location { get; set; }
        public List<int[]> PlotsOwned { get; set; }
        public int PlayerId { get; set; }
        public int Population { get; set; }
        public double PopulationProgress { get; set; }
        public int ProductionId { get; set; }
        public double ProductionProgress { get; set; }
        public List<int[]> Citizens { get; set; }
        public double Housing { get; set; }
        public double HousingRequired { get; set; }
        public double Amenity { get; set; }
        public int[] ReligionFollowers { get; set; }
    }

    public class MatchSettings
    {
        public MatchSettings Settings { get; set; }
    }

    public class Plot
    {
        public int Terrain { get; set; }
        public int Feature { get; set; }
        public int Resource { get; set; }
        public int Improvement { get; set; }
        public int Pillaged { get; set; }
        public int Yield { get; set; }
    }

    public class Unit
    {
        public int Id { get; set; }
        public int TypeId { get; set; }
        public int PlayerId { get; set; }
        public int[] Location { get; set; }
        public int HP { get; set; }
        public int[] Promotion { get; set; }
    }

    public class TechCivic
    {
        public int[] TechCompleted { get; set; }
        public int[] CivicCompleted { get; set; }
        public int TechCurrent { get; set; }
        public int CivicCurrent { get; set; }
        public int TechProgess { get; set; }
        public int CivicProgress { get; set; }
    }

    public class YieldAndStatus
    {
        public double Science { get; set; }
        public double Culture { get; set; }
        public double Gold { get; set; }
        public double Faith { get; set; }
        public double Tourism { get; set; }
        public double Military { get; set; }
    }

    public class Religion
    {
        public int TypeId { get; set; }
        public int FounderId { get; set; }
        public string DisplayName { get; set; }
        public int[] Beliefs { get; set; }
    }

    public class Artifact
    {
        public int Id { get; set; }
        public int TypeId { get; set; }
        public int LocatedCityId { get; set; }
        public string DisplayName { get; set; }
    }


}
