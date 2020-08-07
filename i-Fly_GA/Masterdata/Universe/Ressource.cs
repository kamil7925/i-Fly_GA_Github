using System;
using System.Collections.Generic;

namespace I_Fly.Models
{
    public class Ressource
    {
        public int Id { get; set; }
        public string Description { get; set; }

        public bool Vice { get; set; }

        public byte[] Picture { get; set; } = new byte[0];
    }

    public static class Ressource_Extensions
    {
        public static int Get_Id(this Ressource p_input, List<Ressource> p_ressources)
        {
            for (var i = 0; i < p_ressources.Count; i++)
            {
                if (p_ressources[i].Description == p_input.Description)
                {
                    return p_ressources[i].Id;
                }
            }

            throw new Exception("Ressource not found " + p_input.Description);
        }
    }
}
