using I_Fly.Logic.Genetic_Algorithm;
using I_Fly.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace I_Fly.Logic.Extensions.Genetic_Algorithm
{
    public static class Genetic_Algorithm_Extensions
    {
        private static readonly int deeper_ga_stops_level_1 = 9;
        private static readonly int deeper_ga_stops_level_2 = 12;

        #region Parameters determination

        public static int Population_Seeding_Calculation(this int p_nb_stops)
        {
            if (p_nb_stops <= deeper_ga_stops_level_1)
            {
                return p_nb_stops * 1000;
            }
            else if (p_nb_stops > deeper_ga_stops_level_1 && p_nb_stops <= deeper_ga_stops_level_2)
            {
                return p_nb_stops * 1000;
            }
            else
            {
                return p_nb_stops * 1000;
            }
        }

        public static bool Multi_Population_Trigger(this int p_nb_stops)
        {
            if (p_nb_stops <= deeper_ga_stops_level_1)
            {
                return false;
            }
            else if (p_nb_stops > deeper_ga_stops_level_1 && p_nb_stops <= deeper_ga_stops_level_2)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static int Generation_Convergence_Calculation(this int p_nb_stops)
        {
            if (p_nb_stops <= deeper_ga_stops_level_1)
            {
                return p_nb_stops;
            }
            else if (p_nb_stops > deeper_ga_stops_level_1 && p_nb_stops <= deeper_ga_stops_level_2)
            {
                return Convert.ToInt32(Math.Round(p_nb_stops * 0.75));
            }
            else
            {
                return Convert.ToInt32(Math.Round(p_nb_stops * 0.5));
            }
        }

        public static int Population_Convergence_Calculation(this int p_nb_stops)
        {
            if (p_nb_stops <= deeper_ga_stops_level_1)
            {
                return p_nb_stops;
            }
            else if (p_nb_stops > deeper_ga_stops_level_1 && p_nb_stops <= deeper_ga_stops_level_2)
            {
                return Convert.ToInt32(Math.Round(p_nb_stops * 0.5));
            }
            else
            {
                return Convert.ToInt32(Math.Round(p_nb_stops * 0.25));
            }
        }

        public static double Fitness_Calculation(this int p_nb_stops)
        {
            if (p_nb_stops <= deeper_ga_stops_level_1)
            {
                return 0.75;
            }
            else if (p_nb_stops > deeper_ga_stops_level_1 && p_nb_stops <= deeper_ga_stops_level_2)
            {
                return 0.5;
            }
            else
            {
                return 0.25;
            }
        }

        public static List<Chromosome> Fitness_Evaluation(this List<Chromosome> p_input)
        {
            if (p_input.Count > 0)
            {
                //To get the max profit from all the transactions in all the chromosomes
                List<Transaction> transaction_list = new List<Transaction>();

                for (var f = 0; f < p_input.Count; f++)
                {
                    for (var d = 0; d < p_input[f].Transation_List.Count; d++)
                    {
                        transaction_list.Add(p_input[f].Transation_List[d]);
                    }
                }

                //Getting max profit
                double max_profit = transaction_list.Max(k => k.Profit);

                //Calculating the fitness of each transaction
                for (var f = 0; f < p_input.Count; f++)
                {
                    for (var d = 0; d < p_input[f].Transation_List.Count; d++)
                    {
                        p_input[f].Transation_List[d].Fitness = Math.Round(p_input[f].Transation_List[d].Profit / max_profit, 2);
                    }
                }
            }

            return p_input;
        }

        public static List<Transaction> Fitness_Evaluation(this List<Transaction> p_input)
        {
            //Getting max profit
            double max_profit = p_input.Max(k => k.Profit);

            //Calculating the fitness of each transaction
            for (var y = 0; y < p_input.Count; y++)
            {
                p_input[y].Fitness = Math.Round(p_input[y].Profit / max_profit, 2);
            }

            return p_input;
        }

        #endregion

        #region Frequency calculations

        public static double Frequency_Calculation_Modulo(this int p_input, int p_max_limit)
        {
            //The result follows the following function : y/x. y being the (scale start)² and x the input number. As 1/x, the result value of this function will dicrease quickly at the scale_start value.
            if (p_input > 0)
            {
                double modulo_result = Math.Ceiling(Math.Pow(p_max_limit, 2) / Convert.ToDouble(p_input)) > p_max_limit ? p_max_limit : Math.Ceiling(Math.Pow(p_max_limit, 2) / Convert.ToDouble(p_input));

                return p_input % modulo_result;
            }
            else
            {
                return 1;
            }
        }

        public static double Frequency_Calculation(this int p_input, int p_nb_stops)
        {
            //The result follows the following function : y/x. y being the (scale start)² and x the input number. As 1/x, the result value of this function will dicrease quickly at the scale_start value.
            double result = p_nb_stops.Population_Convergence_Calculation();

            if (p_input > 0)
            {
                result = Math.Ceiling(Math.Pow(p_nb_stops, 2) / Convert.ToDouble(p_input)) > p_nb_stops ? p_nb_stops : Math.Ceiling(Math.Pow(p_nb_stops, 2) / Convert.ToDouble(p_input));
            }

            return result;
        }

        #endregion
    }
}
