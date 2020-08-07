using I_Fly.Logic;
using I_Fly.Logic.Extensions.Genetic_Algorithm;
using I_Fly.Logic.Genetic_Algorithm;
using System;
using System.Collections.Generic;
using System.Linq;

namespace I_Fly.Models.Genetic_Algorithm
{
    public static class Genetic_Algorithm_Process
    {
        #region Caller

        public static Run_Results Run_Genetic_ALgorithm(this Run_Results p_result, List<Transaction> p_transaction_dictionary, GA_Player p_player, Post p_starting_post, int p_nb_stops, bool p_salesman_constrain)
        {
            Chromosome best_route = new Chromosome();

            int counter_max_profit = 0;
            double max_profit = 0;

            bool multi_population = p_nb_stops.Multi_Population_Trigger();

            int g = 0;

            DateTime start_ouille = DateTime.Now;

            while (true)
            {
                //Running genetic algorithm
                List<Chromosome> run_results = best_route.Launch(p_starting_post, p_transaction_dictionary, p_nb_stops, multi_population);

                if (run_results.Count > 0)
                {
                    best_route = run_results[0];
                }

                if (max_profit < best_route.Profit)
                {
                    max_profit = best_route.Profit;
                    counter_max_profit = 0;
                }

                counter_max_profit += 1;

                if (multi_population == false) //if using a single population
                {
                    if (p_salesman_constrain)
                    {
                        run_results = run_results.Salesman_Constrain();

                        if (run_results.Count > 0)
                        {
                            best_route = run_results[0];
                        }
                        else
                        {
                            best_route = new Chromosome();
                        }
                    }

                    break;
                }
                else  if (counter_max_profit >= g.Frequency_Calculation(p_nb_stops))
                {
                    if (p_salesman_constrain)
                    {
                        run_results = run_results.Salesman_Constrain();

                        if (run_results.Count > 0)
                        {
                            best_route = run_results[0];
                        }
                        else
                        {
                            best_route = new Chromosome();
                        }
                    }

                    break;
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                g += 1;
            }

            best_route = best_route.Profit_Over_time(new ApplicationUser() { Balance = p_player.Balance, Cargo = p_player.Cargo });

            //Result
            p_result = new Run_Results() { Max_Profit = best_route.Profit, Runtime = DateTime.Now.Subtract(start_ouille).TotalMilliseconds.ToString(), Transaction_List = best_route.Transation_List };

            return p_result;
        }

        #endregion

        #region Process

        public static List<Chromosome> Launch(this Chromosome p_input, Post p_starting_post, List<Transaction> p_transactions_dictionary, int p_nb_stops, bool p_multi_population)
        {
            //Counter variables
            int counter_max_profit = 0;
            double max_profit = 0;

            int population_seed = p_nb_stops.Population_Seeding_Calculation();

            List<Chromosome> final_population = new List<Chromosome>();

            //Generation Convergence
            int generation_convergence = p_nb_stops.Generation_Convergence_Calculation();

            //Generating initial population
            List<Chromosome> initial_population = new List<Chromosome>();
            initial_population = initial_population.Create_Initial_Population(population_seed, p_starting_post, p_nb_stops, p_transactions_dictionary);

            //Calculating fitness
            initial_population.Fitness_Evaluation();

            //Preparing the final population by adding the initial one
            for (int x = 0; x < initial_population.Count; x++)
            {
                final_population.Add(initial_population[x]);
            }

            double p_fitness = p_nb_stops.Fitness_Calculation();

            //Creating new generation and adding them to the initial population
            while (true)
            {
                if (counter_max_profit <= generation_convergence)
                {
                    final_population = final_population.Generation_Process(p_transactions_dictionary, p_nb_stops, p_fitness, p_input, p_multi_population);

                    if (final_population.Count == 0)
                    {
                        break;
                    }

                    double generation_max_profit = final_population.Max(k => k.Profit);

                    //To limit run until convergence point
                    if (generation_max_profit > max_profit)
                    {
                        max_profit = generation_max_profit;
                    }
                    else
                    {
                        //Re-inject new population
                        if (counter_max_profit.Frequency_Calculation_Modulo(5) == 0)
                        {
                            final_population = final_population.Create_Initial_Population(population_seed / p_nb_stops, p_starting_post, p_nb_stops, p_transactions_dictionary);
                        }
                    }

                    counter_max_profit += 1;
                }
                else
                {
                    break;
                }
            }

            //Final selection
            final_population = final_population.Select(p_fitness);
            final_population = final_population.OrderByDescending(s => s.Profit).ToList();

            return final_population;
        }

        public static List<Chromosome> Generation_Process(this List<Chromosome> p_input, List<Transaction> p_transactions_dictionary, int p_nb_stops, double p_fitness, Chromosome p_best_route, bool p_multi_population)
        {
            double mutation_probability = 0; //Mutation disabled

            p_input = p_input.Best_Route_Injection(p_best_route, p_multi_population);

            //Selecting the best parents by fitness
            List<Chromosome> filtered_parents = p_input.Select(p_fitness);

            List<Chromosome> generation_parents = new List<Chromosome>();

            //Keeping unique parents - Avoids genetic panic (same parents being created over and over again -> increasing population size for next run to again create the same parents -> useless)
            generation_parents.Add_Unique(filtered_parents);

            //Crossover process
            List<Chromosome> crossover_population = generation_parents.Crossover_Process(p_transactions_dictionary, p_nb_stops);

            //Adding to population
            p_input.Add_Unique(crossover_population);

            //Mutation process
            List<Chromosome> mutation_generation = new List<Chromosome>();

            if (mutation_probability > 0)
            {
                for (var x = 0; x < crossover_population.Count; x++)
                {
                    int mutation_random_score = new Random().Next(100);

                    if (mutation_random_score <= (mutation_probability * 100)) // Will be true "p_mutation_probability" % of the time
                    {
                        mutation_generation = mutation_generation.Mutation_Process(crossover_population[x], p_transactions_dictionary);
                    }
                }
            }

            //Adding to population
            p_input.Add_Unique(mutation_generation);

            p_input.Fitness_Evaluation();

            return p_input;
        }

        #endregion

        #region Initial Population

        public static List<Chromosome> Create_Initial_Population(this List<Chromosome> p_input, int p_sedding, Post p_post_from, int p_nb_stops, List<Transaction> p_transaction_dictionnary)
        {
            List<Transaction> temporary_dictionnary = p_transaction_dictionnary.ToList();

            for (var t = 0; t < p_sedding; t++)
            {
                List<Transaction> transactions = new List<Transaction>();
                int[] gene = new int[p_nb_stops];

                gene[0] = p_post_from.Id;

                for (var x = 0; x < p_nb_stops - 1; x++)
                {
                    List<Transaction> possible_transactions = temporary_dictionnary.Where(k => k.Post_From_Id == gene[x]).ToList();

                    if (possible_transactions.Count != 0)
                    {
                        Transaction transaction_to_add = possible_transactions[new int().Random(0, possible_transactions.Count)];

                        gene[x + 1] = transaction_to_add.Post_To_Id;
                        transactions.Add(transaction_to_add);

                        temporary_dictionnary.RemoveAll(z => z.Post_From_Id == transaction_to_add.Post_From_Id && z.Post_To_Id == transaction_to_add.Post_To_Id);
                    }
                    else
                    {
                        break;
                    }
                }

                if (transactions.Count == p_nb_stops - 1)
                {
                    p_input.Add_Unique(new Chromosome() { Route = gene, Transation_List = transactions });
                }
            }

            return p_input;
        }

        public static List<Chromosome> Best_Route_Injection(this List<Chromosome> p_input, Chromosome p_chromosome_to_add, bool p_multi_population)
        {
            if (p_chromosome_to_add.Transation_List.Count > 0)
            {
                if (p_multi_population)
                {
                    if (p_input.Where(k => k.Id == 7925).FirstOrDefault() == null)
                    {
                        p_chromosome_to_add.Id = 7925;
                        p_input.Add(p_chromosome_to_add);
                    }
                }
            }

            return p_input;
        }

        #endregion

        #region Crossover

        public static List<Chromosome> Crossover_Process(this List<Chromosome> p_input, List<Transaction> p_transaction_dictionnary, int p_nb_stops)
        {
            //Crossover point calculation
            int min_crossover = (p_nb_stops / 2);
            int max_crossover = Convert.ToInt32((p_nb_stops * 0.75) + 1); //+ 1 because the max limit is exclusive and 0.75 to avoid selecting the last gene in the chromosome

            int crossover_point = new Random().Next(min_crossover, max_crossover);

            //Parents generation
            List<Chromosome> parent_1_list = new List<Chromosome>();
            List<Chromosome> parent_2_list = new List<Chromosome>();

            for (var g = 0; g < p_input.Count; g++)
            {
                List<int> parent_1 = new List<int>();
                List<int> parent_2 = new List<int>();

                List<Transaction> transactions_parent_1 = new List<Transaction>();
                List<Transaction> transactions_parent_2 = new List<Transaction>();

                Chromosome parent_2_chromosome = new Chromosome();

                for (var w = 0; w < p_input[g].Route.Count(); w++)
                {
                    if (w < crossover_point)
                    {
                        parent_1.Add(p_input[g].Route[w]);

                        if (w < crossover_point - 1)
                        {
                            transactions_parent_1.Add(p_input[g].Transation_List[w]);
                        }
                    }
                    else
                    {
                        parent_2.Add(p_input[g].Route[w]);

                        if (w < p_input[g].Route.Count() - 1)
                        {
                            transactions_parent_2.Add(p_input[g].Transation_List[w]);
                        }

                    }
                }

                if (parent_1_list.Contains(parent_1.ToArray()) == false) // Do not re-initialize transactions
                {
                    parent_1_list.Add(new Chromosome() { Route = parent_1.ToArray(), Transation_List = transactions_parent_1 });
                }

                if (parent_2_list.Contains(parent_2.ToArray()) == false)
                {
                    parent_2_list.Add(new Chromosome() { Route = parent_2.ToArray(), Transation_List = transactions_parent_2 });
                }
            }

            //Childs generation
            List<Chromosome> childs = new List<Chromosome>();

            for (var w = 0; w < parent_1_list.Count; w++)
            {
                List<int> connecting_nodes = p_transaction_dictionnary.Where(k => k.Post_From_Id == parent_1_list[w].Route.Last()).Select(k => k.Post_To_Id).ToList();

                List<Chromosome> matching_parents_2 = parent_2_list.Where(k => connecting_nodes.Contains(k.Route.First())).ToList();

                if (matching_parents_2.Count > 0)
                {
                    for (var g = 0; g < matching_parents_2.Count; g++)
                    {
                        //GENE PART --------------------------------
                        //Creating the gene sequence
                        int[] gene = new int[p_nb_stops];

                        //Adding parent 1
                        for (var i = 0; i < parent_1_list[w].Route.Count(); i++)
                        {
                            gene[i] = parent_1_list[w].Route[i];
                        }

                        //Adding parent 2
                        for (var i = parent_1_list[w].Route.Count(); i < (matching_parents_2[g].Route.Count() + parent_1_list[w].Route.Count()); i++)
                        {
                            gene[i] = matching_parents_2[g].Route[i - parent_1_list[w].Route.Count()];
                        }

                        //TRANSACTION PART --------------------------------
                        //Creating the list of transactions
                        List<Transaction> result = new List<Transaction>();

                        for (var j = 0; j < parent_1_list[w].Transation_List.Count; j++)
                        {
                            result.Add(parent_1_list[w].Transation_List[j]);
                        }

                        //Case array is so small that crossover will cut but end up with 2 genes on one side and 1 on the other => no transaction => error.
                        if (result.Count > 0)
                        {
                            result.Add(p_transaction_dictionnary.Where(k => k.Post_From_Id == result.Last().Post_To_Id && k.Post_To_Id == matching_parents_2[g].Route.First()).FirstOrDefault());
                        }

                        for (var j = 0; j < matching_parents_2[g].Transation_List.Count; j++)
                        {
                            result.Add(matching_parents_2[g].Transation_List[j]);
                        }

                        //ADDING CHROMOSOME TO CHILDS --------------------------------
                        childs.Add(new Chromosome() { Route = gene, Transation_List = result });
                    }
                }
            }

            return childs;
        }

        #endregion

        #region Mutation

        public static List<Chromosome> Mutation_Process(this List<Chromosome> p_input, Chromosome p_mutated_chromosome, List<Transaction> p_dictionary)
        {
            Chromosome result = new Chromosome();

            //Getting the 2 index of the genes that will be swaped
            int gene_1_id = new Random().Next(1, p_mutated_chromosome.Route.Length);
            int gene_2_id = gene_1_id;

            while (gene_2_id == gene_1_id)
            {
                gene_2_id = new Random().Next(1, p_mutated_chromosome.Route.Length);
            }

            int[] new_gene_sequence = p_mutated_chromosome.Route.ToArray();

            //Swaping the genes
            new_gene_sequence[gene_1_id] = p_mutated_chromosome.Route[gene_2_id];
            new_gene_sequence[gene_2_id] = p_mutated_chromosome.Route[gene_1_id];

            if (new_gene_sequence[gene_1_id] != new_gene_sequence[gene_2_id])
            {
                bool fourth_transaction_exist = false;
                int post_from_transac_1 = new_gene_sequence[gene_1_id - 1];

                Transaction transaction_1 = p_dictionary.Where(k => k.Post_From_Id == post_from_transac_1 && k.Post_To_Id == new_gene_sequence[gene_1_id]).FirstOrDefault();
                Transaction transaction_2 = null;
                Transaction transaction_3 = null;
                Transaction transaction_4 = null;

                if (gene_2_id < new_gene_sequence.Length - 1 && transaction_1 != null)
                {
                    fourth_transaction_exist = true;
                    transaction_4 = p_dictionary.Where(k => k.Post_From_Id == new_gene_sequence[gene_2_id] && k.Post_To_Id == new_gene_sequence[gene_2_id + 1]).FirstOrDefault();
                }

                if ((transaction_1 != null && fourth_transaction_exist == false) || (transaction_1 != null && fourth_transaction_exist && transaction_4 != null))
                {
                    transaction_2 = p_dictionary.Where(k => k.Post_From_Id == new_gene_sequence[gene_1_id] && k.Post_To_Id == new_gene_sequence[gene_1_id + 1]).FirstOrDefault();

                    if (transaction_2 != null)
                    {
                        transaction_3 = p_dictionary.Where(k => k.Post_From_Id == new_gene_sequence[gene_2_id - 1] && k.Post_To_Id == new_gene_sequence[gene_2_id]).FirstOrDefault();

                        if (transaction_3 != null)
                        {
                            //Recalculating transaction list
                            result.Route = new_gene_sequence;
                            result.Transation_List = p_mutated_chromosome.Transation_List.ToList();

                            result.Transation_List[gene_1_id - 1] = transaction_1;
                            result.Transation_List[gene_1_id] = transaction_2;
                            result.Transation_List[gene_2_id - 1] = transaction_3;

                            if (fourth_transaction_exist)
                            {
                                result.Transation_List[gene_2_id] = transaction_4;
                            }

                            if (result.Transation_List.Count == result.Route.Length - 1) //Useless? Indirectly controlled with all the "If"
                            {
                                p_input.Add(result);
                            }
                        }
                    }
                }
            }

            return p_input;
        }

        #endregion
    }
}
