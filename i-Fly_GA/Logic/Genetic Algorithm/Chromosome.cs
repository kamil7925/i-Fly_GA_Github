using I_Fly.Models;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace I_Fly.Logic.Genetic_Algorithm
{
    public class Chromosome
    {
        public int Id { get; set; } = 0;

        //Array of post ids
        public int[] Route { get; set; }

        //Associated transactions (= genes)
        public List<Transaction> Transation_List { get; set; } = new List<Transaction>();

        //Calculated property
        public double Profit
        {
            get
            {
                return Transation_List.Sum(k => k.Profit);
            }
        }

        public double Fitness
        {
            get
            {
                return Math.Round(Transation_List.Sum(k => k.Fitness), 2);
            }
        }
    }

    public static class Chromosome_Extensions
    {
        #region Fitness

        public static List<Chromosome> Select(this List<Chromosome> p_input, double p_fitness_level)
        {
            List<Chromosome> result = new List<Chromosome>();

            if (p_input.Count > 0)
            {
                double fitness_filter = Math.Round(p_input.Max(k => k.Fitness) * p_fitness_level, 2);

                result = p_input.Where(k => k.Fitness > fitness_filter).ToList();
            }

            return result;
        }

        #endregion

        #region General

        public static bool Contains(this List<Chromosome> p_input, int[] p_route)
        {
            bool result = true;

            List<int[]> routes = p_input.Select(k => k.Route).ToList();

            //Check if exist in current list
            if (routes.Any(p => p.SequenceEqual(p_route)) == false)
            {
                result = false;
            }

            return result;
        }

        public static void Add_Unique(this List<Chromosome> p_input, List<Chromosome> p_new_list)
        {
            List<int[]> routes = p_input.Select(k => k.Route).ToList();

            //Adding to population
            if (p_new_list.Count > 0)
            {
                for (var w = 0; w < p_new_list.Count; w++)
                {
                    if (routes.Any(p => p.SequenceEqual(p_new_list[w].Route)) == false)
                    {
                        p_input.Add(p_new_list[w]);
                    }
                }
            }
        }

        public static void Add_Unique(this List<Chromosome> p_input, Chromosome p_new)
        {
            List<int[]> routes = p_input.Select(k => k.Route).ToList();

            //Adding to population
            if (routes.Any(p => p.SequenceEqual(p_new.Route)) == false)
            {
                p_input.Add(p_new);
            }
        }

        #endregion

        #region Trading

        public static List<Chromosome> Salesman_Constrain(this List<Chromosome> p_input)
        {
            List<Chromosome> salesman_results = new List<Chromosome>();

            for (var b = 0; b < p_input.Count; b++)
            {
                int[] route_array = p_input[b].Route;

                bool is_duplicate = false;

                for (int i = 0; i < route_array.Length; i++)
                {
                    for (int j = 0; j < i; j++)
                    {
                        if (route_array[i] == route_array[j])
                        {
                            is_duplicate = true;
                            break;
                        }
                    }
                }

                //Add only unique routes
                if (is_duplicate == false)
                {
                    salesman_results.Add(p_input[b]);
                }
            }

            salesman_results = salesman_results.OrderByDescending(k => k.Profit).ToList();

            return salesman_results;
        }

        public static Chromosome Profit_Over_time(this Chromosome p_input, ApplicationUser p_player)
        {
            double balance = p_player.Balance;

            for (var x = 0; x < p_input.Transation_List.Count; x++)
            {
                p_input.Transation_List[x] = p_input.Transation_List[x].Recalculate_Sale(balance, p_player.Cargo);

                balance += p_input.Transation_List[x].Profit;
            }

            return p_input;
        }

        #endregion
    }
}
