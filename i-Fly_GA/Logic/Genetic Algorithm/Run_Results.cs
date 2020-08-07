using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace I_Fly.Models.Genetic_Algorithm
{
    [Serializable]
    public class Run_Results
    {
        public double Max_Profit { get; set; }

        public string Runtime { get; set; }

        public List<Transaction> Transaction_List { get; set; }
    }
}
