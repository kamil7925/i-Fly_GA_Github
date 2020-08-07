using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace I_Fly.Models.Genetic_Algorithm
{
    [Serializable]
    public class GA_Player
    {
        public string UserName { get; set; }

        public double Balance { get; set; }
        public int Cargo { get; set; }
    }

    public static class GA_Player_Extensions
    {
    }
}