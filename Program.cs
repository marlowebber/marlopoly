using System.Runtime.InteropServices;
using System.Numerics;
using System;




class Person
{

 public   Dictionary<int, float> owns        = new Dictionary<int, float>(); 
 public   Dictionary<int, float> likes       = new Dictionary<int, float>(); 
 public   Dictionary<int, int> sources       = new Dictionary<int, int>(); 
 public   Dictionary<int, float> prices      = new Dictionary<int, float>(); 
 public   Dictionary<int, float> needs_quantities       = new Dictionary<int, float>(); 
 public   Dictionary<int, float> needs_priorities       = new Dictionary<int, float>(); 
 public   Dictionary<int, float> personality = new Dictionary<int, float>(); 
 
 public   Dictionary<int, float> ideals      = new Dictionary<int, float>(); 
 public   Dictionary<int, Dictionary<int, float> > agreements = new Dictionary<int, Dictionary<int, float> >(); 
 public   Vector2 position;
 public   bool traded_this_turn;
 public   bool chatted_this_turn;
 public   string name = new string("");


public float hungry = 0.0f;
public float thirsty = 0.0f;
public float hedgy = 0.0f;

public const float eats_per_turn = 0.01f;
public const float drinks_per_turn = 0.01f;
public const float smokes_per_turn = 0.01f;

public bool is_location = false;

public char icon;

    public Person(string name)
    {
        this.name = name;
        this.hungry = Utilities.rng_float();
        this.thirsty = Utilities.rng_float();
        this.hedgy = Utilities.rng_float();
        this.icon = Utilities.rng_char();

        for (int i = 0; i < Content.n_characteristics; i++)
        {
            this.personality.Add(i, Utilities.rng_float());
            this.ideals.Add(i, Utilities.rng_float());
        }
    }


    public void adjust_owned(int item, float amount)
    {
        if (this.owns.ContainsKey(item))
        {
            this.owns[item] += amount;
        }
        else
        {
            this.owns.Add(item, amount);
        }
    }

    public void adjust_prices(int item, float amount)
    {

        if (this.owns.ContainsKey(item))
        {
            this.owns[item] += amount;
        }
        else
        {
            this.owns.Add(item, amount);
        }
    }

    public void adjust_likes(int item, float amount)
    {
        if (this.likes.ContainsKey(item))
        {
            this.likes[item] += amount;
        }
        else
        {
            this.likes.Add(item, amount);
        }

        this.likes[item] = Utilities.clamp(this.likes[item], -1.0f, 1.0f);
    }





    public int best_friend()
    {
        int bf = -1;
        float bf_like = 0.0f;
        foreach (int peep in this.likes.Keys)
        {
            if (this.likes[peep] > bf_like)
            {
                bf = peep;
                bf_like = this.likes[peep];
            }
        }
        return bf;
    }



    public void bodily_functions()
    {
        this.hungry = Utilities.clamp( this.hungry + eats_per_turn, -1.0f, 2.0f );
        this.thirsty = Utilities.clamp( this.thirsty + drinks_per_turn, -1.0f, 2.0f );
        this.hedgy = Utilities.clamp( this.hedgy + smokes_per_turn, -1.0f, 2.0f );

        if (this.hungry > 0.0f)
        {
            if (this.owns.ContainsKey((int)Content.Items.Chips))
            {
                if (this.owns[(int)Content.Items.Chips] >= (this.quantity_owed_to_others((int)Content.Items.Chips) + 1.0f ))
                {
                    this.owns[(int)Content.Items.Chips] -= 1.0f;
                    this.hungry -= 1.0f;
                }
            }
        }


        if (this.hedgy > 0.0f)
        {
            if (this.owns.ContainsKey((int)Content.Items.Smokes))
            {
                if (this.owns[(int)Content.Items.Smokes] >= (this.quantity_owed_to_others((int)Content.Items.Smokes) + 1.0f ))
                {
                    this.owns[(int)Content.Items.Smokes] -= 1.0f;
                    this.hedgy -= 1.0f;
                }
            }
        }


        if (this.thirsty > 0.0f)
        {
            if (this.owns.ContainsKey((int)Content.Items.Beer))
            {
                if (this.owns[(int)Content.Items.Beer] >= (this.quantity_owed_to_others((int)Content.Items.Beer) + 1.0f ))
                {
                    this.owns[(int)Content.Items.Beer] -= 1.0f;
                    this.thirsty -= 1.0f;
                }
            }
        }

    }

    public void adjust_needs(int need, float amount, float priority)
    {
        if (! this.needs_quantities.ContainsKey(need))
        {
            this.needs_quantities.Add(need, amount);
        }
        else
        {
            this.needs_quantities[ need ] += amount;
        }

        if (! this.needs_priorities.ContainsKey(need))
        {
            this.needs_priorities.Add(need, priority);
        }
         else
        {
            this.needs_priorities[ need ] += priority;
        }
    }

    public int greatest_need()
    {
        int greatest_need = -1;
        float greatest_need_priority = 0.0f;

        foreach (int need in this.needs_priorities.Keys)
        {
            if (this.needs_priorities[need] > greatest_need_priority)
            {
                greatest_need_priority = this.needs_priorities[need];
                greatest_need = need;
            }
        }
        return greatest_need;

    }

    public float greatest_need_quantity()
    {
        int greatest_need = this.greatest_need();
        if (greatest_need >=0 )
        {
            return this.needs_quantities[greatest_need];
        }
        return 0.0f;
    }


    public float quantity_owed_to_others(int item)
    {
        float amount = 0.0f;
        foreach (int debtor in this.agreements.Keys)
        {
            if (this.agreements[debtor].ContainsKey(item))
            {
                amount += this.agreements[debtor][item];
            }
        }  
        return amount;
    }


    public void compile_needs()
    {
        // rebuild the list of needs from various agreements and bodily needs.
        this.needs_quantities.Clear();
        this.needs_priorities.Clear();

        foreach (int debtor in this.agreements.Keys)
        {
            Dictionary<int, float> debt = this.agreements[debtor];
            foreach (int resource in debt.Keys)
            {
               
                float value_of_debt_to_a = debt[resource] * this.prices[resource] * this.likes[debtor];
                
                this.adjust_needs(resource ,  debt[resource] , value_of_debt_to_a );

            }
        }
       
        if (! this.is_location)
        {
            if (this.hungry > 0.0f)
            {
                this.adjust_needs((int)Content.Items.Chips , 1.0f,this.hungry );
            }
            if (this.thirsty > 0.0f)
            {
                this.adjust_needs((int)Content.Items.Beer  , 1.0f, this.thirsty );
            }
            if (this.hedgy > 0.0f)
            {
                this.adjust_needs((int)Content.Items.Smokes  , 1.0f, this.hedgy );
            }
        }
      

        // everyone wants 1,000,000 dollars
        // this.adjust_needs((int)Content.Items.Cash  , 1000000.0f, 1.0f);
        


    }




  
}





class World
{
    int time = 0;

    int population_size = 0;

    int world_size = 80;

    int player = 36;

    List<Person> people = new List<Person>();

    public World(int world_size)
    {
        this.world_size = world_size;
    }




    bool print_player = true;

    public void print_character_status(int character)
    {

        Console.WriteLine( this.people[character].name + " turn " + this.time );

        Console.WriteLine("Hungry: " + this.people[character].hungry.ToString());
        Console.WriteLine("Hedgy: " + this.people[character].hedgy.ToString());
        Console.WriteLine("Thirsty: " + this.people[character].thirsty.ToString());

           string prices = "Prices: ";
        foreach (int item in this.people[character].prices.Keys)
        {
            prices += Content.item_names[item] + " " + this.people[character].prices[item].ToString() + ", ";
        }
        Console.WriteLine(prices);

        string owns = "Owns: ";
        foreach (int item in this.people[character].owns.Keys)
        {
            owns += Content.item_names[item] + " " + this.people[character].owns[item].ToString() + ", ";
        }
        Console.WriteLine(owns);

        string needs = "Needs amount: ";
        foreach (int item in this.people[character].needs_quantities.Keys)
        {
            needs += Content.item_names[item] + " " + this.people[character].needs_quantities[item].ToString() + ", ";
        }
        Console.WriteLine(needs);

        string needsp = "Needs priority: ";
        foreach (int item in this.people[character].needs_quantities.Keys)
        {
            needsp += Content.item_names[item] +" " +  this.people[character].needs_priorities[item].ToString() + ", ";
        }
        Console.WriteLine(needsp);

        string likes = "Likes: ";
        foreach (int item in this.people[character].likes.Keys)
        {
            likes += this.people[item].name + " " + this.people[character].likes[item].ToString() + ", ";
        }
        Console.WriteLine(likes);

        
        string sources = "Sources: ";
        foreach (int item in this.people[character].sources.Keys)
        {
            int source = this.people[character].sources[item];
            sources += Content.item_names[item] + " " + this.people[source].name + ", ";
        }
        Console.WriteLine(sources);

        
        string owes = "Owes: ";
        foreach (int debtor in this.people[character].agreements.Keys)
        {
            owes += this.people[debtor].name + " ";
            foreach ( System.Collections.Generic.KeyValuePair<int, float> agreement in this.people[character].agreements[debtor])
            {
                owes += agreement.Value.ToString() + " " + Content.item_names[ agreement.Key ];
            }

            owes += ", ";
        } 
        Console.WriteLine(owes);

    }





   public void camera()
    {
        int viewport_x = 80;
        int viewport_y = 24;

        for (int vy = 0; vy < viewport_y; ++vy)
        {
            string row = "";
            for (int vx = 0; vx < viewport_x; ++vx)
            {

                int x = (int)this.people[this.player].position.X -(viewport_x/2) + vx;
                int y = (int)this.people[this.player].position.Y -(viewport_y/2) + vy; 


                char here = '_';

                // draw a crosshair over the player position
                if (
                   ( y == (((int)this.people[ this.player ] .position.Y) + 1)  
                    || y == (((int)this.people[ this.player ] .position.Y) - 1)  )
                    && x == (int)this.people[ this.player ] .position.X 
                    ) 
                {
                    here = '|';
                }
                if (
                    (x == (((int)this.people[ this.player ] .position.X) + 1)  
                    || x == (((int)this.people[ this.player ] .position.X) - 1)  )
                    && y == (int)this.people[ this.player ] .position.Y 

                    ) 
                {
                    here = '-';
                }


                for (int k = 0; k < population_size; ++k)
                {
                    if ((int)this.people[k].position.X == x && (int)this.people[k].position.Y == y )
                    {
                        here = this.people[k].icon;



                        if (this.people[k].chatted_this_turn)
                        {
                            here = '?';
                        }
                        if (this.people[k].traded_this_turn)
                        {
                            here = '$';
                        }





                        break;
                    }
                }
                row += here;
            }
            Console.WriteLine(row);
        }
    }



    public void setup()
    {
        this.people.Clear();
        this.population_size = 0;
        foreach (string name in Content.person_names)
        {
            this.people.Add(new Person(name));
            this.population_size++;
        }


        for(int j= 0; j < this.population_size; j++ )
        {
            this.people[j].position.X = Utilities.rng_float() * this.world_size;
            this.people[j].position.Y = Utilities.rng_float() * this.world_size;

            
            this.people[j].adjust_owned(  (int) Utilities.rng_int(Content.n_items), Utilities.rng_float()* 10.0f  );
            this.people[j].adjust_owned(  (int)Content.Items.Cash, Utilities.rng_float() * 20.0f );



        }

        int i = 0;
        foreach (string name in Content.location_names)
        {
            this.people[i].name = name;
            this.people[i].is_location = true;
            i++;
        }


        for(int j= 0; j < this.population_size; j++ )
        {
            if (Utilities.rng_bool())
            {
                this.people[j].sources.Add( (int) Content.Items.Beer  , 1 ); // the valley
            }
            else
            {
                this.people[j].sources.Add( (int) Content.Items.Beer  , 0 ); // the lord raglan hotel
            }

            int random_source = Utilities.rng_int(4);

            switch(random_source)
            {
                case 0:
                {
                    this.people[j].sources.Add( (int) Content.Items.Chips  , 3 ); // the vending machine
                    break;
                }
                 case 1:
                {
                    this.people[j].sources.Add( (int) Content.Items.Spin  , 2 ); // Nite Owl
                    break;
                }
                 case 2:
                {
                    this.people[j].sources.Add( (int) Content.Items.Papers  , 2 ); // Nite Owl
                    break;
                }
                 case 3:
                {
                    this.people[j].sources.Add( (int) Content.Items.Filters  , 2 ); // Nite Owl
                    break;
                }
            }

            for(int k= 0; k < Content.n_items; k++ )
            {
                this.people[j].adjust_prices(k, 0.5f + Utilities.rng_float() );
            }
            

        }

        this.adjust_agreement(this.player, this.player-1, (int)Content.Items.Cash, 100.0f);

        this.people[this.player-1].adjust_likes(this.player, 0.5f);
        this.people[this.player].adjust_likes(this.player-1, 0.5f);
    }




    public void adjust_agreement(int a, int b, int item, float quantity)
    {
        // A will owe X amount of stuff to B.
        // A understands that they owe B, and B understands that A owes them.

        // a negative value agreement means you can have that thing from that person.

        // a positive value agreement means you have to give that thing to that person.

        this.introduce(a,b);
        this.exchange_price_information(a, b, item);

        bool exists_a = this.people[a].agreements.ContainsKey(b);
        bool exists_b = this.people[b].agreements.ContainsKey(a);

        if (exists_a && ! exists_b)
        {
            // if one party has a copy of the agreement and the other doesn't, mirror that person's part of the agreement to make the other person's one.
            Dictionary<int, float> for_b = this.people[a].agreements[b];
            foreach (int owed_thing in for_b.Keys )
            {   
                for_b[owed_thing] *= -1;
            }
            this.people[a].agreements.Add( b ,for_b  );
        }
        else if (!exists_a && exists_b)
        {
            Dictionary<int, float> for_a = this.people[b].agreements[a];
            foreach (int owed_thing in for_a.Keys )
            {   
                for_a[owed_thing] *= -1;
            }
            this.people[a].agreements.Add( a ,for_a  );
        }
        else if (!exists_a && !exists_b)
        {
            Dictionary<int, float> debt = new Dictionary<int, float>();
            Dictionary<int, float> credit = new Dictionary<int, float>();
            debt.Add( item, quantity );
            credit.Add( item, -quantity );
            this.people[a].agreements.Add( b ,debt  );
            this.people[b].agreements.Add( a ,credit  );
        }
        else if (exists_a && exists_b)
        {
            // they both agree on an existing agreement; adjust the quantity by the indicated amount.
            this.people[a].agreements[b][item] += quantity;
            this.people[b].agreements[a][item] -= quantity;

            if (a == this.player || b == this.player)
            {
                if (quantity < 0.0f)
                {
      Console.WriteLine(this.people[a].name + " gave " + this.people[b].name + " " + Utilities.abs(quantity).ToString() + " " + Content.item_names[item] + ", as agreed.");
           
                }
                else
                {
      Console.WriteLine(this.people[b].name + " gave " + this.people[a].name + " " + quantity.ToString() + " " + Content.item_names[item]+ ", as agreed."  );
           
                }
           }

            if (this.people[a].agreements[b][item] ==0.0f || this.people[b].agreements[a][item] == 0.0f)
            {
                this.people[a].agreements[b].Remove(item);
                this.people[b].agreements[a].Remove(item);
            } 
        }

        this.people[a].compile_needs();
        this.people[b].compile_needs();

    }


    public void exchange_price_information(int a, int b, int item)
    {
         // if one character doesn't know how much the item is worth, they accept the face value given by the other person,
        // or if they both don't know, they assume it's just worth 1.

        bool a_knows = false;
        bool b_knows = false;

        if (this.people[a].prices.ContainsKey(item))
        {
            a_knows = true;
        }
        if (this.people[b].prices.ContainsKey(item))
        {
            b_knows = true;
        }

        if (!a_knows && !b_knows)
        {
            float noise_amp = 0.001f;
            float noise_a = Utilities.rng_float() * noise_amp;
            float noise_b = Utilities.rng_float() * noise_amp;

            this.people[a].prices.Add(item, 1.0f + noise_a);
            this.people[b].prices.Add(item, 1.0f + noise_b);
        }
        else if (a_knows && !b_knows)
        {
            this.people[b].prices.Add(item, this.people[a].prices[item]);
        } 
        else if (b_knows && !a_knows)
        {
            this.people[a].prices.Add(item, this.people[b].prices[item]);
        }



   
        
    }



    public void refer_to_source(int a, int b, int item)
    {
            // A asks B where to get something. What does B say?

            // if b doesn't have the thing, and b was a's source, refer a to b's source.
            bool a_has_source = this.people[a].sources.ContainsKey(item);
            bool b_has_source = this.people[b].sources.ContainsKey(item);

            if (!a_has_source && b_has_source) 
            {
                this.people[a].sources.Add(item, this.people[b].sources[item]);
            }            
            else if (a_has_source && b_has_source)
            {
                if (this.people[b].sources[item] != a)
                {
                    this.people[a].sources[item] = this.people[b].sources[item];
                }
            }
            else if (a_has_source && !b_has_source)
            {
                // presumably you just tried to get this item from b, and b didn't have it.
                // now b doesn't have a source, you should admit you have no source either.
                if (this.people[a].sources[item] == b)
                {
                    this.people[a].sources.Remove(item);
                }
            }
    }






    public void trade(int a, int b, int b_gives, float b_gives_amount)
    {   

         foreach (int good in this.people[a].owns.Keys)
        {
            this.exchange_price_information(a, b, good);
        }
         foreach (int good in this.people[b].owns.Keys)
        {     
            this.exchange_price_information(a, b, good);
        }



        float p  = this.people[b].prices[b_gives] * b_gives_amount;

        if (a == this.player || b == this.player)
        {
            Console.WriteLine(this.people[a].name + " wants " + this.people[b].name + " to give them " + b_gives_amount.ToString() + " " + Content.item_names[b_gives].ToString()
            + ", which to " + this.people[b].name + " is worth " +  p.ToString() );
            Console.ReadKey(true);
        }
        Dictionary<int, float> a_tradeable_quantities = new Dictionary<int, float>();
        // Dictionary<int, float> b_tradeable_quantities = new Dictionary<int, float>();
        
        Dictionary<int, float> a_tradeable_values_to_a = new Dictionary<int, float>();
        // Dictionary<int, float> b_tradeable_values_to_a = new Dictionary<int, float>();
        Dictionary<int, float> a_tradeable_values_to_b = new Dictionary<int, float>();
        // Dictionary<int, float> b_tradeable_values_to_b = new Dictionary<int, float>();


        // Dictionary<int, float> b_tradeable_profitability_to_a = new Dictionary<int, float>();
        Dictionary<int, float> a_tradeable_profitability_to_b = new Dictionary<int, float>();


        float amount_b_is_willing_to_give = 0.0f;

        if (this.people[b].owns.ContainsKey(b_gives) )
        {
           amount_b_is_willing_to_give =   this.people[b].owns[b_gives] ;
        }

        if (this.people[b].needs_quantities.ContainsKey(b_gives) )
        {
            amount_b_is_willing_to_give = this.people[b].owns[b_gives] - this.people[b].needs_quantities[b_gives];
        }



       
        if (amount_b_is_willing_to_give <= 0.0f)
        {
            if (a == this.player || b == this.player)
            {
                Console.WriteLine( this.people[b].name + " doesn't have any " + Content.item_names[b_gives] + " to spare." );
            }
            if (this.people[a].sources.ContainsKey(b_gives) )
            {
               if ( this.people[a].sources[b_gives] == b)
               {
                    this.refer_to_source(a, b, b_gives);
               }
            } 
            return;
        }
        else
        {
            if (a == this.player || b == this.player)
            {
                Console.WriteLine( this.people[b].name + " has " + amount_b_is_willing_to_give.ToString() + " "  + Content.item_names[b_gives] + " to trade." );
            }
        }

      

        foreach (int good in this.people[a].owns.Keys)
        {
            // this.exchange_price_information(a, b, good);





            float tradeable_volume = this.people[a].owns[good];
            if (this.people[a].needs_quantities.ContainsKey(good) )
            {
                tradeable_volume -= this.people[a].needs_quantities[good];
            }


            if (tradeable_volume > 0.0f)
            {
                 // update B's source information.
                if (!this.people[b].sources.ContainsKey(good))
                {
                    this.people[b].sources.Add(good, a);   
                }
                int b_source = this.people[b].sources[good];


                if (!this.people[b_source].prices.ContainsKey(good))
                {
                    this.exchange_price_information(b_source, b, good);
                }

                if (this.people[a].prices[good] < this.people[b_source].prices[good])
                {
                    this.people[b].sources[good] = a;
                }

            }

            // the tradeable volume is reduced to the size that the other party actually wants.
            // if (this.people[b].needs_quantities.ContainsKey(good))
            // {

                float profitability = this.people[b].prices[good]  - this.people[a].prices[good]  ;

                if (profitability > 0.0f && tradeable_volume > 0.0f)
                {
                    // if (this.people[b].needs_quantities[good] < tradeable_volume)
                    // {
                    //     tradeable_volume = this.people[b].needs_quantities[good];
                    // }

                    a_tradeable_quantities.Add(good, tradeable_volume);
                    a_tradeable_values_to_a.Add(good, tradeable_volume * this.people[a].prices[good]);
                    a_tradeable_values_to_b.Add(good, tradeable_volume * this.people[b].prices[good]);
                    a_tradeable_profitability_to_b.Add(good, profitability );

                    // if (a == this.player || b == this.player)
                    // {
                    //     Console.WriteLine("Counter offer: up to " + a_tradeable_quantities[good].ToString() + " " + Content.item_names[good]);
                    // }
                }

            // }
        }

        // foreach (int good in this.people[b].owns.Keys)
        // {
        //     if (good != b_gives) { continue; }
            
        //     // this.exchange_price_information(a, b, good);

        //     float tradeable_volume = this.people[b].owns[good];
        //     if (this.people[b].needs_quantities.ContainsKey(good) )
        //     {
        //         tradeable_volume -= this.people[b].needs_quantities[good];
        //     }

        //     if (tradeable_volume > b_gives_amount)
        //     {
        //         tradeable_volume = b_gives_amount;
        //     }


        //     // if (this.people[a].needs_quantities.ContainsKey(good))
        //     // {
        //         float profitability =  this.people[a].prices[good]  - this.people[b].prices[good] ;
        //         if (profitability > 0.0f)
        //         {
        //             // if (this.people[a].needs_quantities[good] < tradeable_volume)
        //             // {
        //             //     tradeable_volume = this.people[a].needs_quantities[good];
        //             // }



        //             b_tradeable_quantities.Add(good, tradeable_volume);

        //             b_tradeable_values_to_a.Add(good, tradeable_volume * this.people[a].prices[good]);
        //             b_tradeable_values_to_b.Add(good, tradeable_volume * this.people[b].prices[good]);

        //             b_tradeable_profitability_to_a.Add(good, profitability );
        //         }
        //     // }
        // }

        if (a == this.player || b == this.player)
        {
            // foreach (int good in b_tradeable_profitability_to_a.Keys)
            // {
            //     Console.WriteLine( this.people[b].name + " has " + Content.item_names[good] + " that " + this.people[a].name + " wants." );
            // }
            foreach (int good in a_tradeable_profitability_to_b.Keys)
            {
                float egque = this.people[a].prices[good] * a_tradeable_quantities[good];
                Console.WriteLine( this.people[a].name + " has " + a_tradeable_quantities[good].ToString() + " " + Content.item_names[good] + " that " + this.people[b].name + " wants"  
                + ", which to " +  this.people[a].name + " is worth " + egque.ToString()
                 );
            }

        }


        // each party tries to capitalize as much as possible on the other party's goods while losing as little as possible themselves.
        // at this point, all things in the 'profitability' list are considered profitable, so they would like to exchange all of them if possible.

        // float a_spending_limit = p;//0.0f; // spending limit is just the sum of the other's profitability list, 
        float b_spending_limit = p;//0.0f; // clipped to the total value of the least-valuable of the two profitability lists.
                                       // this can be stated as, "they let the other have as much as they think they're getting".
        


        float a_tradeable_total = 0.0f;

        foreach (int item in a_tradeable_values_to_a.Keys)
        {
            a_tradeable_total += a_tradeable_values_to_a[item];
        }

        if (b_spending_limit > a_tradeable_total)
        {
            b_spending_limit = a_tradeable_total;
        }






        // if a can't trade anything to b in return, refer to b's source
        if (a_tradeable_total <= 0.0f)
        {
            if (a == this.player || b == this.player)
            {
                Console.WriteLine( this.people[a].name + " doesn't have anything that " + this.people[b].name + " wants." );
            }
            if (this.people[a].sources.ContainsKey(b_gives) )
            {
               if ( this.people[a].sources[b_gives] == b)
               {
                    this.refer_to_source(a, b, b_gives);
               }
            } 
            return;
        }


        // foreach (int good in b_tradeable_profitability_to_a.Keys)
        // {
        //     b_spending_limit += b_tradeable_profitability_to_a[good];
        // }

        // foreach (int good in a_tradeable_profitability_to_b.Keys)
        // {
        //     // a_spending_limit += a_tradeable_profitability_to_b[good];
        // }

        // if (a_spending_limit > b_spending_limit)
        // {
        //     a_spending_limit = b_spending_limit;
        // }
        // else
        // {
        //     b_spending_limit = a_spending_limit;
        // }

        // if (a_spending_limit == 0.0f || b_spending_limit == 0.0f) { return; }
        // if (a == this.player || b == this.player)
        // {
        //     Console.WriteLine("They are both willing to exchange goods worth " + final_spending_limit.ToString());
        // }


        float b_gives_value_to_b = b_gives_amount * this.people[b].prices[b_gives];
        if (b_gives_value_to_b > b_spending_limit)
        {
            b_gives_amount *= (b_spending_limit / b_gives_value_to_b);
        }


        float a_received_value = b_gives_amount * this.people[a].prices[b_gives];



        // while (true)
        // {
        //     // A is getting the goods in order from most to least profitable.
        //     int most_profitable = -1;
        //     float highest_profitability = 0.0f;

        //     foreach (int good in b_tradeable_profitability_to_a.Keys)
        //     {
        //         if (b_tradeable_profitability_to_a[good] > highest_profitability)
        //         {
        //             highest_profitability = b_tradeable_profitability_to_a[good];
        //             most_profitable = good;
        //         }
        //     }

        //     if (most_profitable != -1)
        //     {
        //         float value_of_this_trade = b_tradeable_values_to_b[most_profitable];
        //         float quantity_of_this_trade = b_tradeable_quantities[most_profitable];
                
        //         // if (value_of_this_trade > a_spending_limit)
        //         // {
        //         //     float ratio_you_can_have = a_spending_limit / value_of_this_trade;
        //         //     quantity_of_this_trade *= ratio_you_can_have;
        //         //     value_of_this_trade    *= ratio_you_can_have;
        //         // }

        //         // exchange the goods here.
        //         this.people[b].owns[most_profitable] -= quantity_of_this_trade;
        //         this.people[a].owns[most_profitable] += quantity_of_this_trade;

        //         // a_spending_limit -= value_of_this_trade;
        //         b_tradeable_profitability_to_a.Remove(most_profitable);

        //         if (a == this.player || b == this.player)
        //         {
        //             Console.WriteLine( this.people[a].name +  " got " + quantity_of_this_trade.ToString() + " " + Content.item_names[most_profitable] );
        //         }
        //     }

        //     if (most_profitable == -1 
        //     // || a_spending_limit <= 0.0f
        //     )
        //     {
        //         break;
        //     }
        // }



        // this.people


        float b_received_value = 0.0f;
        float a_gave_value = 0.0f;

        while (true)
        {
            // B is getting the goods in order from most to least profitable.
            int most_profitable = -1;
            float highest_profitability = 0.0f;

            foreach (int good in a_tradeable_profitability_to_b.Keys)
            {
                if (a_tradeable_profitability_to_b[good] > highest_profitability)
                {
                    highest_profitability = a_tradeable_profitability_to_b[good];
                    most_profitable = good;
                }
            }

            if (most_profitable != -1)
            {
                float value_of_this_trade_to_a = a_tradeable_values_to_a[most_profitable];
                float value_of_this_trade_to_b = a_tradeable_values_to_b[most_profitable];

                float quantity_of_this_trade = a_tradeable_quantities[most_profitable];
                
                if (value_of_this_trade_to_a > b_spending_limit)
                {
                    float ratio_you_can_have = b_spending_limit / value_of_this_trade_to_a;
                    quantity_of_this_trade *= ratio_you_can_have;
                    value_of_this_trade_to_a    *= ratio_you_can_have;
                    value_of_this_trade_to_b    *= ratio_you_can_have;
                }

                // exchange the goods here.
                this.people[a].owns[most_profitable] -= quantity_of_this_trade;
                this.people[b].owns[most_profitable] += quantity_of_this_trade;

                b_spending_limit -= value_of_this_trade_to_a;
                a_gave_value     += value_of_this_trade_to_a;

                b_received_value += value_of_this_trade_to_b;
                
                a_tradeable_profitability_to_b.Remove(most_profitable);

                // update A's source information.
                if (!this.people[a].sources.ContainsKey(b_gives))
                {
                    this.people[a].sources.Add(b_gives, b);   
                }
                int a_source = this.people[a].sources[b_gives];
                if (!this.people[a_source].prices.ContainsKey(b_gives))
                {
                    this.exchange_price_information(a_source, b, b_gives);
                }
                if (this.people[b].prices[b_gives] < this.people[a_source].prices[b_gives])
                {
                    this.people[a].sources[b_gives] = b;
                }



                if (a == this.player || b == this.player)
                {
                    Console.WriteLine( this.people[b].name +  " got " + quantity_of_this_trade.ToString() + " " + Content.item_names[most_profitable] );
                }
            }

            if (most_profitable == -1 || b_spending_limit <= 0.0f)
            {
                break;
            }
        }



                this.people[a].owns[b_gives] += b_gives_amount;
                this.people[b].owns[b_gives] -= b_gives_amount;

                if (a == this.player || b == this.player)
                {
                    Console.WriteLine( this.people[a].name +  " got " + b_gives_amount.ToString() + " " + Content.item_names[b_gives] );
                }
                

        float b_gave_value = b_gives_amount * this.people[b].prices[b_gives]; 

        



            float rep_bonus_a_about_b = a_received_value - a_gave_value;
            float rep_bonus_b_about_a = b_received_value - b_gave_value;


            if (a == this.player || b == this.player)
                {

                    Console.WriteLine("In total, " + this.people[b].name + " thinks they received " + b_received_value.ToString() + " and that they gave " + b_gave_value.ToString() + " worth of stuff, leading to a rep bonus of " + rep_bonus_b_about_a.ToString());
                    Console.WriteLine("In total, " + this.people[a].name + " thinks they received " + a_received_value.ToString() + " and that they gave " + a_gave_value.ToString() + " worth of stuff, leading to a rep bonus of " + rep_bonus_a_about_b.ToString());

                }


            this.people[a].likes[b] += rep_bonus_a_about_b;
            this.people[b].likes[a] += rep_bonus_b_about_a;


           if (a == this.player || b == this.player)
        {
            Console.ReadKey();
        }


    }







    public void trade_old(int a, int b, int b_gives, float b_gives_amount )
    {

         if (b_gives == -1)
        {
            if (a == this.player || b == this.player)
            {
                Console.WriteLine( "Trade failed: wants a -1.");
            }
            return;
        }
    



        const float trade_coeff = 0.025f;

        int a_gives = -1;
        float a_gives_amount = 0.0f;

        if (a == this.player || b == this.player)
        {
            Console.WriteLine( this.people[a].name + " asks " + this.people[b].name + " if they have " + b_gives_amount.ToString() + " " + Content.item_names[b_gives]);
        }

  
     
     
        this.exchange_price_information(a, b, b_gives);


        bool b_has_the_item = false;
        bool counter_offers_prepared = false;
        bool counter_offer_chosen = false;

        Dictionary<int, float> counter_offers = new Dictionary<int, float>();

        float a_opinion_b_gift = 0.0f;
        float b_opinion_b_gift = 0.0f;
    
        if (this.people[b].owns.ContainsKey(b_gives))
        {
            // scale the amount to the quantity that the giving party has.
            float b_has_available = this.people[b].owns[b_gives];
            if (this.people[b].needs_quantities.ContainsKey(b_gives))
            {
                b_has_available = this.people[b].owns[b_gives] - this.people[b].needs_quantities[b_gives];
            }


            if (b_gives_amount > b_has_available)
            {
                b_gives_amount = b_has_available;
                b_has_the_item = true;
            }
            
           
        }


        if (b_has_the_item)
        {


           

            // counter-offers are prepared.
            foreach (int trade_good in this.people[a].owns.Keys)
            {
                if (trade_good != b_gives)
                {
                    if (! this.people[b].prices.ContainsKey(trade_good))
                    {
                        this.exchange_price_information(a, b, trade_good);
                    }

                    float counter_offer_quantity = b_opinion_b_gift / this.people[b].prices[trade_good];

                    // scale the amount to the quantity that the giving party has.
                    if (counter_offer_quantity > 0.0f)
                    {
                        counter_offer_quantity = Utilities.clamp(counter_offer_quantity, 0.0f, this.people[a].owns[trade_good]);


                    }
                    else
                    {
                        if (this.people[b].owns.ContainsKey(trade_good))
                        {
                            counter_offer_quantity = Utilities.clamp(counter_offer_quantity, 0.0f, this.people[b].owns[trade_good]);
                        }
                    }

                    counter_offers.Add(trade_good, counter_offer_quantity);
                    counter_offers_prepared = true;


                  
                    
                }
            }
        }

        // a selects a counter offer.
        if (counter_offers_prepared)
        {
            float best_offer_value = 0.0f;
            foreach (int trade_good in counter_offers.Keys)
            {

                this.exchange_price_information(a, b, trade_good);

                float counter_offer_value_to_b = counter_offers[trade_good] * this.people[b].prices[trade_good];

                  if (a == this.player || b == this.player)
                    {
                        Console.WriteLine( this.people[a].name + " offers " + counter_offers[trade_good].ToString() + " " + Content.item_names[trade_good] + " in return, "
                        + "which to " + this.people[b].name + " is worth " + counter_offer_value_to_b.ToString()
                         );
                    }

                if (counter_offer_value_to_b > best_offer_value)
                {   
                    best_offer_value = counter_offer_value_to_b;
                    a_gives = trade_good;
                    counter_offer_chosen = true;
                }
            }
        }









 if (a == this.player || b == this.player)
            {
                Console.WriteLine( this.people[b].name + " will trade up to " + b_gives_amount.ToString() + " " + Content.item_names[b_gives] );
            }

            // calculate how much each party thinks the offer is worth.
            a_opinion_b_gift = b_gives_amount * this.people[a].prices[b_gives];
            b_opinion_b_gift = b_gives_amount * this.people[b].prices[b_gives];


            if (a == this.player || b == this.player)
            {
                Console.WriteLine( this.people[a].name + " thinks that " + b_gives_amount.ToString() +" " +  Content.item_names[b_gives]  + " is worth " + a_opinion_b_gift.ToString() );
                Console.WriteLine( this.people[b].name + " thinks that " + b_gives_amount.ToString() +" " +  Content.item_names[b_gives]  + " is worth " + b_opinion_b_gift.ToString() );
            }








        float price_hike_coeff = 0.1f;

        if (  b_has_the_item &&  counter_offers_prepared && counter_offer_chosen )
        {

            // b_gives_amount *= ratio;

            Console.WriteLine(  this.people[b].name + " accepts the offer of " + Content.item_names[b_gives] + ". " 
            + "They trade " + b_gives_amount.ToString() + Content.item_names[b_gives] + " for it."
            );


            a_gives_amount = counter_offers[a_gives] ;
             
            // the goods are exchanged.
            this.people[a].adjust_owned(b_gives,  b_gives_amount );
            this.people[b].adjust_owned(b_gives,  -b_gives_amount );
            this.people[a].adjust_owned(a_gives,  -a_gives_amount);
            this.people[b].adjust_owned(a_gives,  a_gives_amount );
            
            // reputation adjustments are made based on how good of a deal each party thinks they got.
            float a_opinion_a_gift = counter_offers[a_gives] * this.people[a].prices[b_gives];
            float b_opinion_a_gift = counter_offers[a_gives] * this.people[b].prices[b_gives];

            float rep_adjust_a = Utilities.fast_sigmoid(    (a_opinion_b_gift - a_opinion_a_gift) * trade_coeff  );
            float rep_adjust_b = Utilities.fast_sigmoid(    (b_opinion_a_gift - b_opinion_b_gift) * trade_coeff  );

            this.people[a].adjust_likes(b, rep_adjust_a);
            this.people[b].adjust_likes(a, rep_adjust_b);

            // Console.WriteLine( $"{this.people[a].name} traded {counter_offers[a_gives]} {Content.item_names[a_gives]} to {this.people[b].name} for {b_gives_amount} {Content.item_names[b_gives]}" );
            this.people[a].traded_this_turn = true;
            this.people[b].traded_this_turn = true;


            this.people[a].compile_needs();  
            this.people[b].compile_needs();  

            if (a == this.player || b == this.player)
            {
                Console.WriteLine(" A deal was reached: " + this.people[a].name + " received " + b_gives_amount.ToString() + " " +  Content.item_names[b_gives] 
                  + ", and " + this.people[b].name + " received "  + counter_offers[a_gives] .ToString() + " " +  Content.item_names[a_gives] );
            }


            // if the trade was successful, raise your price a bit.
            float inc = 1.0f + price_hike_coeff;
            this.people[a].adjust_prices( a_gives, this.people[a].prices[a_gives] * inc  ); 
            this.people[a].adjust_prices( b_gives, this.people[a].prices[b_gives] * inc  ); 

            this.people[b].adjust_prices( a_gives, this.people[b].prices[a_gives] * inc  ); 
            this.people[b].adjust_prices( b_gives, this.people[b].prices[b_gives] * inc  ); 

        }
        else
        {




            this.refer_to_source(a, b, b_gives);

            if (!b_has_the_item)
            {
                if (a == this.player || b == this.player)
                {
                    Console.WriteLine(" A deal could not be reached: " + this.people[b].name
                     + " doesn't have the item that " + this.people[a].name + " wants.");
                }
                return;
            }



            if (!counter_offers_prepared)
            {
                if (a == this.player || b == this.player)
                {
                    Console.WriteLine(" A deal could not be reached: " + this.people[a].name + " doesn't have anything to trade.");
                }
                return;
            }



            if (!counter_offer_chosen)
            {

                // if the trade was possible but rejected, lower your price a bit.

                float inc = 1.0f - price_hike_coeff;
                this.people[a].adjust_prices( b_gives, this.people[a].prices[b_gives] * inc  ); 
                this.people[b].adjust_prices( b_gives, this.people[b].prices[b_gives] * inc  ); 


                if (a == this.player || b == this.player)
                {
                    Console.WriteLine(" A deal could not be reached: " + this.people[b].name+ " doesn't want any of the counter-offers.");
                }

                return;
            }





        }
       

    


    }




    public void gossip(int a, int b)
    {


        this.introduce(a,b);


        const float const_gossip = 0.025f;

        // 1. small talk.
        // a says something that falls somewhere on their scale of personality characteristic.
        // b has a reputation adjustment based on the difference between their ideals and what a said.
        int topic =  Utilities.rng_int( Content.n_characteristics);
        float rep_adjust_b =   Utilities.fast_sigmoid(((1.0f - (this.people[a].personality[topic] - this.people[b].ideals[topic] ))) * const_gossip );
        this.people[b].adjust_likes(a, rep_adjust_b);

        // 2. talk about a person
        int c = -1;
        foreach (int friend_of_a in this.people[a].likes.Keys)
        {
            foreach (int friend_of_b in this.people[b].likes.Keys)
            {
                if (friend_of_a == friend_of_b)
                {
                    c = friend_of_a;
                    break;
                }
            }

        }

        if (c != -1)
        {
            float rep_adjust_a_b     = Utilities.fast_sigmoid( this.people[b].likes[c] * this.people[a].likes[c] * const_gossip);
            float rep_adjust_b_a     = Utilities.fast_sigmoid( this.people[a].likes[c] * this.people[b].likes[c] * const_gossip);
            float rep_adjust_a_c     = Utilities.fast_sigmoid( this.people[b].likes[c] * this.people[a].likes[b] * const_gossip);
            float rep_adjust_b_c     = Utilities.fast_sigmoid( this.people[a].likes[c] * this.people[b].likes[a] * const_gossip);

            this.people[a].adjust_likes(b, rep_adjust_a_b);
            this.people[a].adjust_likes(c, rep_adjust_a_c);

            this.people[b].adjust_likes(a, rep_adjust_b_a);
            this.people[b].adjust_likes(c, rep_adjust_b_c);
        }
        this.people[a].chatted_this_turn = true;
        this.people[b].chatted_this_turn = true;


        // 3. exchange source information
        foreach (int item in this.people[a].sources.Keys)
        {
            if (! this.people[b].sources.ContainsKey(item))
            {
                this.people[b].sources.Add(item, this.people[a].sources[item]);
            }
            else
            {
                int source_a = this.people[a].sources[item];
                int source_b = this.people[b].sources[item];

                if (this.people[source_a].prices.ContainsKey(item) && this.people[source_b].prices.ContainsKey(item))
                {
                    if (this.people[source_a].prices[item] > this.people[source_b].prices[item])
                    {
                        this.people[a].sources[item] = source_b;
                    }
                    else
                    {
                        this.people[b].sources[item] = source_a;
                    }
                }
            }
        }
    }


      public void update_position_based_on_needs(int a)
    {
        Vector2 destination = this.people[a].position;
        bool go = false;

        int greatest_need = this.people[a].greatest_need();
        float greatest_need_priority = 0.0f;
        if (this.people[a].needs_priorities.ContainsKey(greatest_need))
        {
            greatest_need_priority = this.people[a].needs_priorities[greatest_need];
        }
        int bf = this.people[a].best_friend();

        bool can_pay_debt = false;
        float biggest_payment_importance = 0.0f;
        int biggest_creditor = -1;

        foreach( int creditor in this.people[a].agreements.Keys)
        {
            foreach (int item in this.people[a].agreements[creditor].Keys)
            {
                if (this.people[a].owns.ContainsKey(item))
                {
                    if (this.people[a].owns[item] > 0.0f)
                    {
                        if (this.people[a].likes.ContainsKey(creditor))
                        {
                            if (this.people[a].likes[creditor] > biggest_payment_importance)
                            {
                                
                                biggest_payment_importance = this.people[a].likes[creditor];
                                can_pay_debt = true;
                                biggest_creditor = creditor;

                            }
                        }
                    }
                }   
            }
        }




        
        if (a == this.player && this.print_player && greatest_need != -1)
        {
            Console.WriteLine("Greatest Need: " +  Content.item_names[ greatest_need ]);
        }



        if (can_pay_debt)
        {
            go = true;
            destination = this.people[biggest_creditor].position;

                if (a == this.player && this.print_player)
            {
                Console.WriteLine("    Going to see " + this.people[biggest_creditor].name + " to repay debt. ");
            }
        }



        else if (this.people[a].sources.ContainsKey(greatest_need))
        {
            int source = this.people[a].sources[greatest_need];
            go = true;
            destination = this.people[source].position;

              if (a == this.player && this.print_player)
            {
                Console.WriteLine("    Moving to source: " + this.people[source].name);
            }
        }
        else if (greatest_need == -1 && bf >= 0 )
        {
            // you don't need anything; go hang out with a friend
            destination = this.people[bf].position;
            go = true;
            if (a == this.player && this.print_player)
            {
                Console.WriteLine("    Hanging out with " + this.people[bf].name);
            }
        }
        else if (greatest_need == -1 && bf == -1 )
        {
            // you don't need anything; but you don't have any friends either.
            // vibrate around randomly until you bump into something.
            if (a == this.player && this.print_player)
            {
                Console.WriteLine("    Wandering");
            }
            destination.X += Utilities.rng_float() - 0.5f;
            destination.Y += Utilities.rng_float() - 0.5f;
            go = true;
        }
        else
        {
            int source = Utilities.rng_int(this.population_size);
            if (a == this.player && this.print_player)
            {
                Console.WriteLine("    Asking a random person: " + this.people[source].name);
            }
            this.people[a].sources.Add(greatest_need,  source );
        }

        if (go)
        {

            float angle = (float)Math.Atan2(destination.Y - this.people[a].position.Y,destination.X - this.people[a].position.X );

            const float speed = 1.0f;

            float x_move = (float)Math.Cos(angle) * speed;
            float y_move = (float)Math.Sin(angle) * speed;

            if (destination.X - this.people[a].position.X < x_move)
            {
                this.people[a].position.X = destination.X;
            }
            else
            {
                this.people[a].position.X += x_move;
            }

            if (destination.Y - this.people[a].position.Y < y_move)
            {
                this.people[a].position.Y = destination.Y;
            }
            else
            {
                this.people[a].position.Y += y_move;
            }

             this.people[a].position.X  = Utilities.clamp( this.people[a].position.X , 0, this.world_size);
             this.people[a].position.Y  = Utilities.clamp( this.people[a].position.Y , 0, this.world_size);

        }

    }


    public void introduce(int a, int b)
    {

                        // if the two don't know each other, well they do now! Add them to each other's dictionaries.
                        if (! this.people[a].likes.ContainsKey(b))
                        {
                            this.people[a].adjust_likes(b, 0.0f);
                        }
                        if (! this.people[b].likes.ContainsKey(a))
                        {
                            this.people[b].adjust_likes(a, 0.0f);
                        }

    }


    public void settle_agreements(int a, int b)
    {

        if (this.people[a].agreements.ContainsKey(b))
        {
            foreach (int item in this.people[a].agreements[b].Keys)
            {

                // if (this.people[a].agreements[b][item] > 0.0f) // if you are the person who owes, do the repaying and such.
                // {
                    float amount_to_give = this.people[a].agreements[b][item];

                    // if (amount_to_give > 0.0f)
                    // {
                       
                    //     if ( amount_to_give > this.people[a].owns[item])
                    //     {
                    //         amount_to_give =  this.people[a].owns[item];
                    //     }
                    // }
                    // else
                    
                     if (amount_to_give > 0.0f)
                    {
                    if ( amount_to_give > this.people[a].owns[item])
                        {
                            amount_to_give =  this.people[a].owns[item];
                        }
                    }

                        // amount_to_give *= -1;

                        // a minus sign here indicates repayment, whereas a positive would increase the amount of debt.
                        this.adjust_agreement(a, b, item, -amount_to_give);

                        this.people[b].owns[item] += amount_to_give;
                        this.people[a].owns[item] -= amount_to_give;
                        
                    // }
                // }
            }

            if (this.people[a].agreements[b].Count() == 0 )
            {
                this.people[a].agreements.Remove(b);
            }
        }


    }


    public void person_turn(int a)
    {

            this.people[a].compile_needs(); 

            if (this.print_player)
            {
                if (a == this.player)
                {
                    this.print_character_status(a);
                }
            }


            this.people[a].chatted_this_turn = false;
            this.people[a].traded_this_turn = false;



            int random_start = Utilities.rng_int( this.population_size);
            for (int bhh = 0; bhh < this.population_size; bhh++  ) 
            {
                int b = (bhh + random_start) % this.population_size;
                if (a != b)
                {
                    if (System.Numerics.Vector2.Distance(this.people[a].position, this.people[b].position) < 1.0f)
                    {


                        if (this.print_player)
                        {
                            if (a == this.player || b == this.player)
                            {
                                Console.WriteLine($"{this.people[a].name} met with {this.people[b].name}");
                            }
                        }



                        this.introduce(a,b);



                        this.settle_agreements(a, b);





                        int b_gives = this.people[a].greatest_need();
                      
                        if ( b_gives != -1)
                        {
                            float b_gives_amount = this.people[a].greatest_need_quantity();
                            this.trade(a,b, b_gives, b_gives_amount);
                        }
                        // else
                        // {
                        //     float b_gives_amount = this.people[a].greatest_need_quantity();
                        //     this.trade_for_specific_item(a, b, b_gives, b_gives_amount);
                        // }
                    
                        
                        if (! this.people[a].is_location )
                        {
                            this.gossip(a, b);

                           
                        }

                        break;
                    } 
                }
            }

            if (! this.people[a].is_location )
            {
                this.people[a].bodily_functions();        
                this.update_position_based_on_needs(a);
            }

    }



    public void update()
    {


        for (int a = 0; a < this.population_size; a++  ) 
        {


            this.person_turn(a);
        }

        this.time++;
    }
}





class Game
{



    private Random random = new Random();


    private World world = new World(80);



    public void detect_OS()
    {
        // https://ubuntu.com/blog/creating-cross-platform-applications-with-net-on-ubuntu-on-wsl
        Console.WriteLine("Marlopoly version 0.0");
        Console.WriteLine($"Hello {System.Environment.GetEnvironmentVariable("USER")}");
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Console.WriteLine("We're on Linux!");
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Console.WriteLine("We're on Windows!");
        }
        Console.WriteLine("Version {0}", Environment.OSVersion.Version);
    }

    public void run()
    {
        this.world.setup();
        for (int i = 0; i < 1000; i++)
        {
            this.world.update();
            Console.WriteLine("Turn {0}", i);

            this.world.camera();
        }
    }



 
    
}


class Marlopoly
{

       static  public void Main(string[] args)
    {
        Game game = new Game();
        game.detect_OS();
        game.run();
    }


}