using I_Fly.Models.Genetic_Algorithm;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.InteropServices;

namespace I_Fly.Models
{
    [Serializable]
    public class Transaction
    {
        public int Id { get; set; }

        public string Type { get; set; }

        public DateTime Entry_Datetime { get; set; }

        public DateTime Transaction_Datetime { get; set; }

        public string Reference { get; set; }

        //Infos
        [NotMapped]
        public string Ressource_Name { get; set; } = "";
        public int Ressource_Id { get; set; }

        [NotMapped]
        public string Post_From_Name { get; set; } = "";
        public int Post_From_Id { get; set; }

        [NotMapped]
        public string Post_To_Name { get; set; } = "";
        public int Post_To_Id { get; set; }

        public bool Open { get; set; } = true;

        [NotMapped]
        public string Status_Text { get; set; } = "Closed";
        public int Status { get; set; } = 2; //1 = open, 2 = closed, 3 = lost

        //Buy
        public double Buy_Price { get; set; } = 0;
        public int Buy_SCU { get; set; } = 0;

        public int Player_Id { get; set; }

        //Sell
        public double Sale_Price { get; set; } = 0;

        //Analytics
        [NotMapped]
        public double Profit
        {
            get
            {
                if(Status == 1)
                {
                    return 0;
                }
                else if (Status == 2)
                {
                    return (Sale_Price * Buy_SCU) - (Buy_Price * Buy_SCU) > 0 ? Math.Round((Sale_Price * Buy_SCU) - (Buy_Price * Buy_SCU), 2) : 0;
                }
                else
                {
                    return (Buy_Price * Buy_SCU) * (-1);
                }
            }
        }

        [NotMapped]
        public double Fitness { get; set; } = 0;
    }

    public static class Transaction_Extensions
    {
        #region Transaction Dictionary

        public static List<Transaction> Get_Transactions_Dictionary(this List<Transaction> result, List<Post> p_post_universe, List<Ressource> p_ressource_universe, List<Price_Ledger> p_purchase_price_universe, List<Price_Ledger> p_sales_price_universe, GA_Player p_player)
        {
            if (p_post_universe.Count > 0 && p_purchase_price_universe.Count > 0 && p_sales_price_universe.Count > 0)
            {
                //Generating all the possible transactions
                for (var x = 0; x < p_post_universe.Count; x++)
                {
                    List<Price_Ledger> post_prices = p_purchase_price_universe.Where(k => k.Post_Id == p_post_universe[x].Id).ToList();

                    List<Price_Ledger> sellable_prices_lines = p_sales_price_universe.Where(w => post_prices.Select(f => f.Ressource_Id).Contains(w.Ressource_Id)).ToList();

                    List<Post> reachable_posts = p_post_universe.Where(y => sellable_prices_lines.Select(j => j.Post_Id).Contains(y.Id)).ToList();

                    for (var t = 0; t < reachable_posts.Count; t++)
                    {
                        result.Add(p_post_universe[x].Get_Sale(reachable_posts[t], post_prices, sellable_prices_lines, p_ressource_universe, p_player.Balance, p_player.Cargo));
                    }
                }

                //Calculating the fitness of each transaction
                result.Fitness_Evaluation_Transaction();
            }

            return result;
        }

        public static List<Transaction> Generate_Transaction_Dictionary(this List<Post> p_posts, List<Ressource> p_ressource, List<Price_Ledger> p_prices, bool p_pirate_zones, bool p_rest_areas, bool p_vice, GA_Player p_player)
        {
            List<Transaction> result = new List<Transaction>();

            //---------- PRE LOADED DATA ----------
            //Filters
            List<Post> post_universe = p_posts.Where(k => k.Is_Pirate == false && k.Is_Rest_Area == false).ToList();

            if (p_pirate_zones)
            {
                List<Post> list_pirate_zones = p_posts.Where(k => k.Is_Pirate == p_pirate_zones).ToList();

                for (int x = 0; x < list_pirate_zones.Count(); x++)
                {
                    post_universe.Add(list_pirate_zones[x]);
                }
            }

            if (p_rest_areas)
            {
                List<Post> list_rest_areas = p_posts.Where(k => k.Is_Rest_Area == p_rest_areas).ToList();

                for (int x = 0; x < list_rest_areas.Count(); x++)
                {
                    post_universe.Add(list_rest_areas[x]);
                }
            }

            List<Ressource> ressources_universe = p_ressource.Where(k => k.Vice == false).ToList();

            if (p_vice)
            {
                List<Ressource> list_vice_ressources = p_ressource.Where(k => k.Vice == p_vice).ToList();

                for (int x = 0; x < list_vice_ressources.Count(); x++)
                {
                    ressources_universe.Add(list_vice_ressources[x]);
                }
            }

            //To pre-load all purchase prices
            List<Price_Ledger> purchase_price_universe = p_prices.Where(k =>
                    ressources_universe.Select(p => p.Id).Contains(k.Ressource_Id) &&
                    k.Is_Buy == true &&
                    k.Price > 0
                ).ToList();

            //To pre-load all sale prices
            List<Price_Ledger> sales_price_universe = p_prices.Where(s =>
                purchase_price_universe.Select(k => k.Ressource_Id).Contains(s.Ressource_Id) &&
                post_universe.Select(p => p.Id).Contains(s.Post_Id) &&
                s.Price > 0 &&
                s.Is_Buy == false
            ).ToList();

            result = result.Get_Transactions_Dictionary(post_universe, ressources_universe, purchase_price_universe, sales_price_universe, p_player);
            //---------- PRE LOADED DATA ----------

            return result;
        }

        #endregion

        public static Transaction Recalculate_Sale(this Transaction p_input, double p_balance, int p_cargo)
        {
            p_input.Buy_SCU = (p_balance / p_input.Buy_Price > p_cargo) ? p_cargo : Convert.ToInt32(Math.Round(p_balance / p_input.Buy_Price));

            return p_input;
        }

        public static List<Transaction> Fitness_Evaluation_Transaction(this List<Transaction> p_input)
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

        public static Transaction Get_Sale(this Post p_post_from, Post p_post_to, List<Price_Ledger> p_purchase_prices, List<Price_Ledger> p_sale_prices, List<Ressource> p_ressource_list, double p_balance, int p_cargo)
        {
            //Buyable items at starting location
            List<Price_Ledger> starting_post_buyables_prices = p_purchase_prices.Where(k => k.Post_Id == p_post_from.Id).ToList();
            List<Ressource> buyable_ressources = p_ressource_list.Where(k => starting_post_buyables_prices.Select(p => p.Ressource_Id).Contains(k.Id)).ToList();

            //Items of the starting location sellable at the destination location
            List<Price_Ledger> buyable_items_sale_prices = p_sale_prices.Where(k => k.Post_Id == p_post_to.Id && buyable_ressources.Select(y => y.Id).Contains(k.Ressource_Id)).ToList();

            //Get all sales routes
            List<Transaction> sale_routes = new List<Transaction>();
            for (var t = 0; t < buyable_items_sale_prices.Count; t++)
            {
                double sale_price = buyable_items_sale_prices[t].Price;
                double buy_price = starting_post_buyables_prices.Where(k => k.Ressource_Id == buyable_items_sale_prices[t].Ressource_Id).Select(k => k.Price).Max();

                int buy_scu = (p_balance / buy_price > p_cargo) ? p_cargo : Convert.ToInt32(Math.Round(p_balance / buy_price));

                sale_routes.Add(new Transaction()
                {
                    Ressource_Id = buyable_items_sale_prices[t].Ressource_Id,
                    Post_From_Id = p_post_from.Id,
                    Post_To_Id = p_post_to.Id,
                    Buy_Price = buy_price,
                    Buy_SCU = buy_scu,
                    Sale_Price = sale_price
                });
            }

            //Return the most profitable ones
            Transaction best_sale = sale_routes.OrderByDescending(k => k.Profit).FirstOrDefault();

            return best_sale;
        }

        public static Transaction Get_Reference(this Transaction p_input)
        {
            p_input.Reference =  p_input.Entry_Datetime.Year.ToString() + p_input.Entry_Datetime.Month.ToString() + p_input.Entry_Datetime.Day.ToString() + p_input.Entry_Datetime.Hour.ToString() + p_input.Entry_Datetime.Minute.ToString() + p_input.Entry_Datetime.Second.ToString() + p_input.Entry_Datetime.Millisecond.ToString() + p_input.Player_Id.ToString();

            return p_input;
        }
    }
}
