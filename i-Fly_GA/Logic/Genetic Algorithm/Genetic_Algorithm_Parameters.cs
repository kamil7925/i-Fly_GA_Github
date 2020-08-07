using I_Fly.Models.Genetic_Algorithm;
using System;
using System.Collections.Generic;

namespace I_Fly.Models
{
    [Serializable]
    public class Genetic_Algorithm_Parameters
    {
        public Post Starting_Post { get; set; }

        public GA_Player Player { get; set; }

        public List<Transaction> Transactions_Dictionary { get; set; }

        public bool Quick_Run { get; set; }

        public  double Max_Runtime { get; set; }

        public int Nb_Stops { get; set; }

        public bool Salesman { get; set; }
    }
}
