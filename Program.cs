using System.Runtime.InteropServices;
using System.Numerics;
using System;



class Source
{
    public int type = (int)Content.source_types.PERSON;
    public int value = 0;
}







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
            this.owns[item] = amount;
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



   
    public void adjust_needs(int need, float amount, float priority)
    {
        if (! this.needs_quantities.ContainsKey(need))
        {
            this.needs_quantities.Add(need, amount);
        }
        else
        {
            this.needs_quantities[ need ] = amount;
        }

        if (! this.needs_priorities.ContainsKey(need))
        {
            this.needs_priorities.Add(need, priority);
        }
         else
        {
            this.needs_priorities[ need ] = priority;
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

                if (this.needs_quantities.ContainsKey(need))
                {
                    if (this.needs_quantities[need] > 0.0f)  // doesn't really make sense to have negative needs
                    {
                            greatest_need_priority = this.needs_priorities[need];
                            greatest_need = need;
                    }
                }
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

                if (value_of_debt_to_a > 0.0f)
                {
                    // it makes sense to owe a negative amount of stuff, it just means the other person owes you
                    // but it doesn't make sense to need a negative amount of stuff, like you need to get rid of it i guess?
                    this.adjust_needs(resource ,  debt[resource] , value_of_debt_to_a );
                }
                

            }
        }

        if (this.is_location)
        {
            this.compile_location_needs();
        }
        else
        {
            this.compile_person_needs();
        }
       
   

    }

    void compile_person_needs()
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

    void compile_location_needs()
    {

        this.needs_priorities[  (int)Content.Items.Cash  ] = 1.0f;
        this.needs_quantities[  (int)Content.Items.Cash  ] = 1000000.0f;

        this.owns[(int)Content.Items.Cash] = 100.0f;
   
        if (this.name == Content.location_names[0]  )
        {
            this.owns[(int)Content.Items.Beer] = 100.0f;
        }
        else  if (this.name == Content.location_names[1]  )
        {
            this.owns[(int)Content.Items.Beer] = 100.0f;
        }
        else  if (this.name == Content.location_names[2]  )
        {
            this.owns[(int)Content.Items.Spin] = 100.0f;
            this.owns[(int)Content.Items.Filters] = 100.0f;
            this.owns[(int)Content.Items.Papers] = 100.0f;
            this.owns[(int)Content.Items.Lighter] = 100.0f;
        }
        else  if (this.name == Content.location_names[3]  )
        {
            this.owns[(int)Content.Items.Chips] = 100.0f;
        }


    }





  
}




class Square
{

}



class World
{
    int time = 0;

    int population_size = 0;

    int player = 36;

    List<Person> people = new List<Person>();

    Square[] map = new Square[Content.world_size];

    public World()
    {
        
    }



     public void bodily_functions(int a)
    {
        this.people[a].hungry = Utilities.clamp( this.people[a].hungry + Content.eats_per_turn, -1.0f, 2.0f );
        this.people[a].thirsty = Utilities.clamp( this.people[a].thirsty + Content.drinks_per_turn, -1.0f, 2.0f );
        this.people[a].hedgy = Utilities.clamp( this.people[a].hedgy + Content.smokes_per_turn, -1.0f, 2.0f );

        if (this.people[a].hungry > 0.0f)
        {
            if (this.people[a].owns.ContainsKey((int)Content.Items.Chips))
            {
                if (this.people[a].owns[(int)Content.Items.Chips] >= (this.people[a].quantity_owed_to_others((int)Content.Items.Chips) + 1.0f ))
                {
                    this.people[a].owns[(int)Content.Items.Chips] -= 1.0f;
                    this.people[a].hungry -= 1.0f;
                }
            }
        }


        if (this.people[a].hedgy > 0.0f)
        {
            if (this.people[a].owns.ContainsKey((int)Content.Items.Smokes))
            {
                if (this.people[a].owns[(int)Content.Items.Smokes] >= (this.people[a].quantity_owed_to_others((int)Content.Items.Smokes) + 1.0f ))
                {
                    this.people[a].owns[(int)Content.Items.Smokes] -= 1.0f;
                    this.people[a].hedgy -= 1.0f;
                }
            }
        }


        if (this.people[a].thirsty > 0.0f)
        {
            if (this.people[a].owns.ContainsKey((int)Content.Items.Beer))
            {
                if (this.people[a].owns[(int)Content.Items.Beer] >= (this.people[a].quantity_owed_to_others((int)Content.Items.Beer) + 1.0f ))
                {
                    this.people[a].owns[(int)Content.Items.Beer] -= 1.0f;
                    this.people[a].thirsty -= 1.0f;
                }
            }
        }

    }

    



    public void print_character_status(int character)
    {


        Console.WriteLine("--------------------------------------------------------");
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

        
        string owes = "Owes(+)/Owed(-): ";
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

        Console.WriteLine("--------------------------------------------------------");

    }


    public void clear_screen()
    {
        for (int i = 0; i < 1000; ++i)
        {
            Console.WriteLine("");
        }
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

                // outside of the world edges is clear.
                if (x < 0 || x > Content.world_size || y < 0 || y > Content.world_size)
                {
                    here = ' ';
                }
                // draw a border around the edge of the world.
                else if (x == 0 || x == Content.world_size )
                {
                    here = '|';
                }
                else if ( y == 0 || y == Content.world_size)
                {
                    here = '-';
                }


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


                for (int k = 0; k < this.population_size; ++k)
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

                if (Char.IsWhiteSpace(here))
                {
                    here = ' ';
                }

                row += here;
            }
            Console.WriteLine(row);
        }


Console.WriteLine("Nearby:" );
        for (int i = 0; i < this.population_size; i++)
        {
            if (this.touching(this.player, i))
            {
                Console.WriteLine(  this.people[i].name );
            }
        }



    }



    public int select_person()
    {
        int result = -1;

        Dictionary<char, int> nearby = new Dictionary<char, int> ();

        Console.WriteLine("select a nearby person:");

        char index = 'a';
        for (int i = 0; i < this.population_size; i++)
        {
            if (this.touching(this.player, i))
            {
                nearby.Add(index, i);
                Console.WriteLine( "[" + index.ToString() + "] " + this.people[i].name);

                if (index == 'z')
                {
                    break;
                }

                index++;
            }
        }

        ConsoleKeyInfo cki = Console.ReadKey();
        
        if (nearby.ContainsKey(cki.KeyChar))
        {
            return nearby[cki.KeyChar];
        }

        return result;
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
            this.people[j].position.X = Utilities.rng_float() * Content.world_size;
            this.people[j].position.Y = Utilities.rng_float() * Content.world_size;

            
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

        this.adjust_agreement(this.player-1, this.player, (int)Content.Items.Cash, 100.0f);

        this.people[this.player-1].adjust_likes(this.player, 0.5f);
        this.people[this.player].adjust_likes(this.player-1, 0.5f);


    // print some intro text.
    Console.WriteLine("What will you do? [o] list options");
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

            if (a_has_source)
            {
                if (this.people[a].sources[item] == a)
                {
                    a_has_source = false;
                    this.people[a].sources.Remove(item);
                }
            }

              if (b_has_source)
            {
                if (this.people[b].sources[item] == b)
                {
                    b_has_source = false;
                    this.people[b].sources.Remove(item);
                }
            }

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


        this.introduce(a,b);


         if (a == this.player || b == this.player)
        {
            Console.WriteLine( this.people[a].name + " wants to trade with " + this.people[b].name + " for " + b_gives_amount.ToString() + " " + Content.item_names[b_gives] );
        }


        Dictionary<int, float> a_tradeable_quantities = new Dictionary<int, float>();
        Dictionary<int, float> a_tradeable_values_to_a = new Dictionary<int, float>();
        Dictionary<int, float> a_tradeable_values_to_b = new Dictionary<int, float>();
        Dictionary<int, float> a_tradeable_profitability_to_b = new Dictionary<int, float>();
        float amount_b_is_willing_to_give = 0.0f;

        // both parties appraise the goods by noticing how the other is acting.
        foreach (int good in this.people[a].owns.Keys)
        {
            this.exchange_price_information(a, b, good);
        }
        foreach (int good in this.people[b].owns.Keys)
        {     
            this.exchange_price_information(a, b, good);
        }

        // check how much of the requested item B is able to trade.
        if (this.people[b].owns.ContainsKey(b_gives) )
        {
           amount_b_is_willing_to_give =   this.people[b].owns[b_gives] ;
            if (this.people[b].needs_quantities.ContainsKey(b_gives) )
            {
                if (this.people[b].needs_quantities[b_gives] > 0.0f)
                {
                    // if this happens when the needed quantity is negative, it allows them to trade stuff they're owed, but don't actually have!
                    // needless to say, this is an economic disaster.
                    amount_b_is_willing_to_give = this.people[b].owns[b_gives] - this.people[b].needs_quantities[b_gives];
                }
            }


            // A preexisting arrangement to exchange that item means it cannot be traded to this person until the agreement is resolved.
            if (this.people[b].agreements.ContainsKey(a))
            {
                if (this.people[b].agreements[a].ContainsKey(b_gives))
                {
                    amount_b_is_willing_to_give = 0.0f;
                }
            }

            if (this.people[a].agreements.ContainsKey(b))
            {
                if (this.people[a].agreements[b].ContainsKey(b_gives))
                {
                    amount_b_is_willing_to_give = 0.0f;
                }
            }
        }


        if (amount_b_is_willing_to_give > 0.0f)
        {
            if (a == this.player || b == this.player)
            {
                Console.WriteLine( this.people[b].name + " has " + amount_b_is_willing_to_give.ToString() + " "  + Content.item_names[b_gives] + " to trade." );
            }
 
        }
        else
        {
            // if B doesn't have enough to trade, fail out, but don't cause a reputation or friendship penalty.
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

        // A prepares a list of offers that B can choose.
        foreach (int good in this.people[a].owns.Keys)
        {
     
            // limit by how much A has on hand.
            float tradeable_volume = this.people[a].owns[good];
            if (this.people[a].needs_quantities.ContainsKey(good) )
            {
                tradeable_volume -= this.people[a].needs_quantities[good];
            }

            // update B's source information now you know that A has goods.
            if (tradeable_volume > 0.0f)
            {
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

            // the desirability of A's offering to B is calculated.
            float profitability = this.people[b].prices[good]  - this.people[a].prices[good]  ;
            if (profitability > 0.0f && tradeable_volume > 0.0f)
            {
                a_tradeable_quantities.Add(good, tradeable_volume);
                a_tradeable_values_to_a.Add(good, tradeable_volume * this.people[a].prices[good]);
                a_tradeable_values_to_b.Add(good, tradeable_volume * this.people[b].prices[good]);
                a_tradeable_profitability_to_b.Add(good, profitability );
            }
        }

        // each party tries to capitalize as much as possible on the other party's goods while losing as little as possible themselves.
        // they evaluate the goods they have received, and allow the other to take that much from their own goods.,
        // at this point, all things in the 'profitability' list are considered profitable, so they would like to exchange as much of them as possible.

        float b_spending_limit = this.people[b].prices[b_gives] * amount_b_is_willing_to_give ;

        float a_tradeable_total = 0.0f;

        foreach (int item in a_tradeable_values_to_a.Keys)
        {
            a_tradeable_total += a_tradeable_values_to_a[item];
        }

        if (b_spending_limit > a_tradeable_total)
        {
            b_spending_limit = a_tradeable_total;
        }

        // if A can't trade anything to B in return, refer to B's source
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

        float b_gives_value_to_b = amount_b_is_willing_to_give * this.people[b].prices[b_gives];
        if (b_gives_value_to_b > b_spending_limit)
        {
            amount_b_is_willing_to_give *= (b_spending_limit / b_gives_value_to_b);
        }
        float a_received_value = amount_b_is_willing_to_give * this.people[a].prices[b_gives];

        float b_received_value = 0.0f;
        float a_gave_value = 0.0f;

        List<int> goods_that_didnt_sell = new List<int>();

        foreach(int item in this.people[a].owns.Keys)
        {
            goods_that_didnt_sell.Add(item);
        }

        int count = 0;
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

                goods_that_didnt_sell.Remove(most_profitable);
                
                a_tradeable_profitability_to_b.Remove(most_profitable);

                // update A's source information.
                if (!this.people[a].sources.ContainsKey(most_profitable))
                {
                    this.people[a].sources.Add(most_profitable, b);   
                }
                int a_source = this.people[a].sources[most_profitable];
                if (!this.people[a_source].prices.ContainsKey(most_profitable))
                {
                    this.exchange_price_information(a_source, b, most_profitable);
                }
                if (this.people[b].prices[most_profitable] < this.people[a_source].prices[most_profitable])
                {
                    this.people[a].sources[most_profitable] = b;
                }


                this.people[a].adjust_prices(most_profitable, this.people[a].prices[most_profitable] * 1.1f);

                if (a == this.player || b == this.player)
                {
                    Console.WriteLine( this.people[b].name +  " got " + quantity_of_this_trade.ToString() + " " + Content.item_names[most_profitable] );
                }
            }

            if (most_profitable == -1 || b_spending_limit <= 0.0f || count > Content.n_items)
            {
                break;
            }
            count++;
        }






        


                this.people[a].owns[b_gives] += amount_b_is_willing_to_give;
                this.people[b].owns[b_gives] -= amount_b_is_willing_to_give;

                if (a == this.player || b == this.player)
                {
                    Console.WriteLine( this.people[a].name +  " got " + amount_b_is_willing_to_give.ToString() + " " + Content.item_names[b_gives] );
                }
                

            float b_gave_value = amount_b_is_willing_to_give * this.people[b].prices[b_gives]; 

        



         

       



            if (a_received_value > 0.0f && b_received_value > 0.0f)
            {
             

                float rep_bonus_a_about_b = a_received_value - a_gave_value;
                float rep_bonus_b_about_a = b_received_value - b_gave_value;
                this.people[a].likes[b] += rep_bonus_a_about_b;
                this.people[b].likes[a] += rep_bonus_b_about_a;


                if (a == this.player || b == this.player)
                {

                    Console.WriteLine("In total, " + this.people[b].name + " thinks they received " + b_received_value.ToString() + " and that they gave " + b_gave_value.ToString() + " worth of stuff, leading to a rep bonus of " + rep_bonus_b_about_a.ToString());
                    Console.WriteLine("In total, " + this.people[a].name + " thinks they received " + a_received_value.ToString() + " and that they gave " + a_gave_value.ToString() + " worth of stuff, leading to a rep bonus of " + rep_bonus_a_about_b.ToString());

                }


                this.people[b].adjust_prices(b_gives, this.people[b].prices[b_gives] * 1.1f);
            

            }

                 foreach(int item in goods_that_didnt_sell)
            {
                this.people[a].adjust_prices(item, this.people[a].prices[item] * 0.9f);
            }






    }







    public void gossip(int a, int b)
    {


        this.introduce(a,b);


        const float const_gossip = 0.025f;

        if (a == this.player || b == this.player)
        {
            Console.WriteLine(this.people[a].name + " met with " + this.people[b].name);
            Console.WriteLine( "they like each other " + this.people[a].likes[b].ToString() + " and " + this.people[b].likes[a].ToString() + " respectively.");
        }

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


            if (a == this.player || b == this.player)
            {



                Console.WriteLine("they talked about " + this.people[c].name);
                Console.WriteLine(  this.people[a].name + " likes " + this.people[c].name + " " + this.people[a].likes[c].ToString()
               + " and " +  this.people[b].name + " likes " + this.people[c].name + " " + this.people[b].likes[c].ToString());

               Console.WriteLine("this made " + this.people[a].name + " like " + this.people[b].name + " " + rep_adjust_a_b.ToString() + " more,"
               + " and like "+ this.people[c].name + " " + rep_adjust_a_c.ToString() + " more");


               Console.WriteLine("this made " + this.people[b].name + " like " + this.people[a].name + " " + rep_adjust_b_a.ToString() + " more,"
               + " and like "+ this.people[c].name + " " + rep_adjust_b_c.ToString() + " more");


            }

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


    public bool touching(int a, int b)
    {
         if (System.Numerics.Vector2.Distance(this.people[a].position, this.people[b].position) < 1.5f)
        {
            return true;
        }
        return false;

    }


      public void npc_ai(int a)
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

                            float repayment_importance = this.people[a].prices[item] * this.people[a].agreements[creditor][item];

                            if (repayment_importance > 0.0f)
                            {
                                
                                biggest_payment_importance = repayment_importance;
                                can_pay_debt = true;
                                biggest_creditor = creditor;

                            }
                        }
                    }
                }   
            }
        }




        if (can_pay_debt)
        {
            go = true;
            destination = this.people[biggest_creditor].position;

                if (a == this.player )
            {
                Console.WriteLine("    Going to see " + this.people[biggest_creditor].name + " to repay debt. ");
            }
        }



        else if (this.people[a].sources.ContainsKey(greatest_need))
        {
            int source = this.people[a].sources[greatest_need];
            go = true;
            destination = this.people[source].position;

              if (a == this.player)
            {
                Console.WriteLine("    Moving to source: " + this.people[source].name);
            }
        }
        else if (greatest_need == -1 && bf >= 0 )
        {
            // you don't need anything; go hang out with a friend
            destination = this.people[bf].position;
            go = true;
            if (a == this.player)
            {
                Console.WriteLine("    Hanging out with " + this.people[bf].name);
            }
        }
        else if (greatest_need == -1 && bf == -1 )
        {
            // you don't need anything; but you don't have any friends either.
            // vibrate around randomly until you bump into something.
            if (a == this.player)
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
            if (a == this.player)
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

   
                this.people[a].position.X += x_move;
     
                this.people[a].position.Y += y_move;

             this.people[a].position.X  = Utilities.clamp( this.people[a].position.X , 0, Content.world_size);
             this.people[a].position.Y  = Utilities.clamp( this.people[a].position.Y , 0, Content.world_size);

        }
















            int random_start = Utilities.rng_int( this.population_size);
            for (int bhh = 0; bhh < this.population_size; bhh++  ) 
            {
                int b = (bhh + random_start) % this.population_size;
                if (a != b)
                {
                    if (this.touching(a,b))
                    {
                        if (a <0 || b < 0) { continue; }
                        if (a >= this.population_size || b >= this.population_size) { continue; }

                        if (a == this.player || b == this.player)
                        {
                            Console.WriteLine($"{this.people[a].name} met with {this.people[b].name}");
                        }
                       

                        this.introduce(a,b);


                        this.settle_agreements(a, b);


                        if (! this.people[a].is_location )
                        {
                            this.gossip(a, b);   
                        }
                     

                        int b_gives = this.people[a].greatest_need();
                      
                        if ( b_gives != -1)
                        {
                            float b_gives_amount = this.people[a].greatest_need_quantity();
                            this.trade(a,b, b_gives, b_gives_amount);
                        }
                    
                        
                        

                        break;
                    } 
                }
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

                        if (amount_to_give > 0.0f)
                        {

                                // amount_to_give *= -1;

                                // a minus sign here indicates repayment, whereas a positive would increase the amount of debt.
                                this.adjust_agreement(a, b, item, -amount_to_give);

                                this.people[b].owns[item] += amount_to_give;
                                this.people[a].owns[item] -= amount_to_give;

                        }
                    

                    
                    }
                        
                    // }
                // }
            }

            if (this.people[a].agreements[b].Count() == 0 )
            {
                this.people[a].agreements.Remove(b);
            }
        }


    }



    public void player_trade()
    {


        int b = this.select_person();

      
        int b_gives = this.people[this.player].greatest_need();
        
        if ( b_gives != -1)
        {
            float b_gives_amount = this.people[this.player].greatest_need_quantity();
            this.trade(this.player,b, b_gives, b_gives_amount);
        }



    }

    public void player_talk()
    {

        int b = this.select_person();

        if (b != -1)
        {
            this.gossip(this.player, b);
        }


    }



    public void player_look()
    {

       

            int random_start = Utilities.rng_int( this.population_size);
            for (int bhh = 0; bhh < this.population_size; bhh++  ) 
            {
                int b = (bhh + random_start) % this.population_size;
                if (this.player != b)
                {
                    if (this.touching(this.player,b))
                    {
                        Console.WriteLine("Here is " + this.people[b].name);

                    }
                }
            
        }
    }



    // ConsoleKeyInfo cki;


    ConsoleKeyInfo user_input()
    {
        Console.WriteLine("->");
        return Console.ReadKey(true);
    }

    void wait_for_user()
    {
        Console.WriteLine("[any] proceed");
        while(true)
        {
            if (Console.ReadKey(true).KeyChar == ' ')
            {
                return;
            }
        }
    }


    public void person_turn(int a)
    {

        

         
                if (a == this.player)
                {

                    bool wait = false;
                   ConsoleKeyInfo  cki = this.user_input();
                    
                    switch(cki.Key)
                    {


                        case ConsoleKey.C:
                        {


                            this.player_talk();

                            wait = true;
                            break;
                        }


                        case ConsoleKey.L:
                        {
                            this.player_look();
                            wait = true;

                            break;
                        }

                          case ConsoleKey.T:
                        {
                            this.player_trade();
                            wait = true;

                            break;
                        }

                        case ConsoleKey.O:
                        {

                            
                            Console.WriteLine("[q] Print character status");
                            Console.WriteLine("[up, down, left, right] move");
                            Console.WriteLine("[c] chat");
                            Console.WriteLine("[t] trade");

                            Console.WriteLine("[space] wait 1 turn");

                            wait = true;
                            break;
                        }

                        case ConsoleKey.Q:
                        {
                            this.print_character_status(a);

                            wait = true;
                            break;
                        }


                        case ConsoleKey.Spacebar:
                        {
                            break;
                        }


                        case ConsoleKey.LeftArrow:
                        {
                            this.people[a].position.X -= 1.0f;
                            break;
                        }
                        case ConsoleKey.RightArrow:
                        {
                            this.people[a].position.X += 1.0f;
                            break;
                        }
                        case ConsoleKey.UpArrow:
                        {
                            this.people[a].position.Y -= 1.0f;
                            break;
                        }
                        case ConsoleKey.DownArrow:
                        {
                            this.people[a].position.Y += 1.0f;
                            break;
                        }


                    }


                    if (wait)
                    {
                        this.wait_for_user();
                    }
                    
            

            }
            else
            {
    
                this.npc_ai(a);
            }


      
     

         
                this.people[a].bodily_functions();    
            

    }



    public void update()
    {

        this.time++;

        for (int a = 0; a < this.population_size; a++  ) 
        {

             this.people[a].chatted_this_turn = false;
            this.people[a].traded_this_turn = false;




            this.people[a].compile_needs(); 



           
             if (! this.people[a].is_location )
            {

                this.person_turn(a);

            }
        }




    }
}





class Game
{



    private Random random = new Random();


    private World world = new World();



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


    public void world_turn()
    {
            this.world.clear_screen();
            this.world.camera();
            this.world.update();
    }

    public void run()
    {
        this.world.setup();
        for (int i = 0; i < 1000; i++)
        {
            this.world_turn();
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