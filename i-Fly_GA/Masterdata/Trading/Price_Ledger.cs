using System.ComponentModel.DataAnnotations.Schema;

namespace I_Fly.Models
{
    public class Price_Ledger
    {
        public int Id { get; set; }

        public int Ressource_Id { get; set; } = 0;

        public int Post_Id { get; set; } = 0;

        public double Price { get; set; } = 0;

        public bool Is_Buy { get; set; } = false;

        [NotMapped]
        public string Ressource_Descr { get; set; } = "";

        [NotMapped]
        public string Post_Descr { get; set; } = "";
    }
}
