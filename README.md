# I_Fly_GA
Genetic algorithm to calculate trade routes in Star Citizen.
----------------------------------------------------------------------
This algorithm allows players to calculate long trade routes (6+ selling/buying points). Using a more efficient logic compared to the recursive algorithms, this will be a must have once the game will have more posts/planets/systems and less 30k :)
This main logic of this algorithm is described in an article that can be found here : https://www.linkedin.com/pulse/from-star-citizen-genetic-algorithms-route-process-traders-marcilly/?trackingId=m0Rij0NlBsvJ%2BegJJVYfgQ%3D%3D. However, the program slightly evolved since then and is more efficient/dynamic compared to the way I described it in the article.

NOTE: the mutation process was disabled. The performance was highly impacted for no real impact on the final profit. To re-enable it, modify the mutation probability variable in the function Generation_Process.

How to use the program?
-------------------------------
It was not meant to be implemented in a larger one and should be used as an external calculation engine.
The program is expecting the following parameters to work properly:
- Starting post: The post where to start from (object Post)
- Player: the object contains the initial investment + cargo capacity (object Player)
- A transaction dictionary: basically, every possible transaction in the game between 2 points. This might look enormous, but it's calculated quite quickly it's just a permutation problem. (Object List). Please see section “How to generate a transaction dictionary?” for more information.
- Quick Run: To say whether the algorithm should stop at the first iteration or restart using the previous iteration best child (Object: Boolean)
- Max Runtime: in case you want to have multiple runs and define a point of time at which the calculation should stop. Example: 2000ms maximum to calculate the road). This highly influence the result.
- Number of stops: The amount of stops you're willing to make. For example, you want to have at least 15 steps of selling/buying on your journey (Object: Integer)

The Genetic Algorithm Extensions class
-------------------------------------------------
This file is important in the algorithm logic and will contain a set of functions and constants that will highly influence the result of the GA run. 

There are 2 important constants:
-	deeper_ga_stops_level_1: This integer indicates the number of stops at which the route is considered a “medium” route. It is set to 9.
-	deeper_ga_stops_level_2: This integer indicates the number of stops at which the route is considered a “long” route. It is set to 12.

The main functions are:
-	Multi_Population_Trigger: This function will return a Boolean. The goal is to ensure the best result for “long” routes. Concretely this means that if the user is making the calculation for a long route, the best chromosome from the previous run will be re-injected in the initial population of the next run. 
-	Population_Seeding_Calculation: This function will return an integer indicating how much chromosomes the program should initially create. Currently, the math behind this function is simply: p_nb_stops * 1000. It’s an arbitrary choice however under 20 stops this initial population was enough to work with.
-	Generation_Convergence_Calculation: This function returns an integer. Its role is to define the amount of repetition of the max profit before the program stops creating new generations. For example, for a short route it is equal to the number of stops. Let’s imagine we want to make 5 stops. So if the maximum profit on the first generation of chromosomes is 10000 and that after 5 generations the maximum profit is still 10000, then the program will stop there and consider it found the best route.
-	Fitness_Calculation: This function returns a Double and gives the level of fitness a chromosome should have to be kept for the next generation. This function is called right after the initial population is generated. Currently the values are 0.75 for a short route (meaning only the top 25% of the chromosomes will be kept for the next generation), 0.5 for the medium routes and 0.25 for the long routes.

How to interpret the results?
-----------------------------------
The results are communicated back to the calling program through a Run_Result class that can be easily modified to be adapted to your system. Personally, I was using full C# programs.

How to generate a transaction dictionary?
---------------------------------------------------
The transaction dictionary is a list of transactions. The goal was to pre-calculate all the possible transactions before making any calculation. This method saved a lot of time. To generate the dictionary, the method I used is the following: 
1.	Create the “post” universe: this means I first list all the posts/rest areas the player could go through (in my context the user could exclude rest areas, pirate posts for example).
2.	Create the “resources” universe: here I would list all the resources and store them in a list (as previously the user could exclude the vice and stims from that list).
3.	Retrieve all the purchase prices and all the sales prices and store them into 2 different lists.

I wrote it the following way in my context :

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

Once the masterdata are prepared, For each post in the post universe:
1.	Retrieve all the purchase prices for the given post
2.	Retrieve all the sales prices in the locations where the user can sell what he can buy in the current post.
3.	For all the post existing in the list mentioned previously, create each transaction and add it to the final list.
4.	Repeat the operation for every post in the post universe.

If you have questions feel free to mail me at : kamil.marcilly@gmail.com
