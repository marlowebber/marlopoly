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

    public Person(string name, ref Random r)
    {
        this.name = name;
        this.hungry = r.NextSingle();
        this.thirsty = r.NextSingle();
        this.hedgy = r.NextSingle();
        this.icon = (char)r.Next(255);

        for (int i = 0; i < Content.n_characteristics; i++)
        {
            this.personality.Add(i, r.NextSingle());
            this.ideals.Add(i, r.NextSingle());
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


    public void bodily_functions()
    {
        this.hungry = Utilities.clamp( this.hungry + eats_per_turn, 0.0f, 2.0f );
        this.thirsty = Utilities.clamp( this.thirsty + drinks_per_turn, 0.0f, 2.0f );
        this.hedgy = Utilities.clamp( this.hedgy + smokes_per_turn, 0.0f, 2.0f );
    }

    public void adjust_needs(int need, float amount, float priority)
    {
        if (! this.needs_quantities.ContainsKey(need))
        {
            this.needs_quantities.Add(need, 0.0f);
        }
        if (! this.needs_priorities.ContainsKey(need))
        {
            this.needs_priorities.Add(need, 0.0f);
        }
        this.needs_quantities[ need ] += amount;
        this.needs_priorities[ need ] += priority;
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
                this.adjust_needs(resource ,  debt[resource] , this.likes[debtor] );
            }
        }

        if (! this.is_location)
        {
            if (this.hungry > 1.0f)
            {
                this.adjust_needs((int)Content.Items.Chips , 1.0f,(int)this.hungry );
            }
            if (this.thirsty > 1.0f)
            {
                this.adjust_needs((int)Content.Items.Beer  , 1.0f,(int)this.hungry );
            }
            if (this.hedgy > 1.0f)
            {
                this.adjust_needs((int)Content.Items.Smokes  , 1.0f,(int)this.hungry );
            }
        }


    }




  
}



class World
{
    int time = 0;

    int population_size = 0;

    int world_size = 0;

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

        


    }





   public void camera()
    {
        int viewport_x = 160;
        int viewport_y = 16;

        for (int y = 0; y < viewport_y; ++y)
        {
            string row = "";
            for (int x = 0; x < viewport_x; ++x)
            {
                char here = '_';
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



    public void setup(ref Random r)
    {
        this.people.Clear();
        this.population_size = 0;
        foreach (string name in Content.person_names)
        {
            this.people.Add(new Person(name, ref r));
            this.population_size++;
        }


        for(int j= 0; j < this.population_size; j++ )
        {
            this.people[j].position.X = r.NextSingle() * this.world_size;
            this.people[j].position.Y = r.NextSingle() * this.world_size;

            
            this.people[j].adjust_owned(  (int) r.Next(0,Content.n_items), r.NextSingle() * 10.0f  );
            this.people[j].adjust_owned(  (int)Content.Items.Cash, r.NextSingle() * 20.0f );



        }

        int i = 0;
        foreach (string name in Content.location_names)
        {
            this.people[i].name = name;
            this.people[i].is_location = true;
            i++;
        }
    }

    public void trade(int a, int b )
    {

        this.introduce(a,b);

        int b_gives = this.people[a].greatest_need();
        float b_gives_amount = this.people[a].greatest_need_quantity();

        bool b_can_deal = true;


        // if one character doesn't know how much the item is worth, they accept the face value given by the other person,
        // or if they both don't know, they assume it's just worth 1.
        if (!this.people[a].prices.ContainsKey(b_gives))
        {
            if (this.people[b].prices.ContainsKey(b_gives))
            {
                this.people[a].prices.Add(b_gives, this.people[b].prices[b_gives]);
            }
            else
            {
                this.people[a].prices.Add(b_gives, 1.0f);
                this.people[b].prices.Add(b_gives, 1.0f);
            }
        }
        if (!this.people[b].prices.ContainsKey(b_gives))
        {
            if (this.people[a].prices.ContainsKey(b_gives))
            {
                this.people[b].prices.Add(b_gives, this.people[a].prices[b_gives]);
            }
        }









        if (this.people[b].owns.ContainsKey(b_gives))
        {
            // scale the amount to the quantity that the giving party has.
            if ((b_gives_amount > 0.0f) && (b_gives_amount > this.people[b].owns[b_gives]))
            {
                b_gives_amount = this.people[b].owns[b_gives];
            }
            else if ((b_gives_amount < 0.0f) && (b_gives_amount < this.people[a].owns[b_gives])) 
            {
                b_gives_amount = this.people[a].owns[b_gives];
            }
        }
        else
        {
            b_can_deal = false;
        }

        if (b_gives_amount == 0.0f) { b_can_deal = false; }


        if (!b_can_deal)
        {
            // if b doesn't have the thing, and b was a's source, refer a to b's source.
            bool a_has_source = this.people[a].sources.ContainsKey(b_gives);
            bool b_has_source = this.people[b].sources.ContainsKey(b_gives);

            if (!a_has_source && b_has_source) 
            {
                this.people[a].sources.Add(b_gives, this.people[b].sources[b_gives]);
            }            
            else if (a_has_source && b_has_source)
            {
                if (this.people[b].sources[b_gives] != a)
                {
                    this.people[a].sources[b_gives] = this.people[b].sources[b_gives];
                }
            }
            else if (a_has_source && !b_has_source)
            {
                // presumably you just tried to get this item from b, and b didn't have it.
                // now b doesn't have a source, you should admit you have no source either.
                this.people[a].sources.Remove(b_gives);
            }
           

            return;
        }

        // calculate how much each party thinks the offer is worth.
		float a_opinion_b_gift = b_gives_amount * this.people[a].prices[b_gives];
		float b_opinion_b_gift = b_gives_amount * this.people[b].prices[b_gives];

        // counter-offers are prepared.
        Dictionary<int, float> counter_offers = new Dictionary<int, float>();
        foreach (int trade_good in this.people[a].owns.Keys)
        {
            if (trade_good != b_gives)
            {
                if (this.people[b].prices.ContainsKey(trade_good))
                {
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
                        else
                        {
                            return;
                        }
                    }

                    counter_offers.Add(trade_good, counter_offer_quantity);
                }
            }
        }

        // a selects a counter offer.
        int a_gives = -1;
        float best_offer_value = 0.0f;
        foreach (int trade_good in counter_offers.Keys)
        {
            float counter_offer_value_to_a = counter_offers[trade_good] * this.people[a].prices[trade_good];
            if (counter_offer_value_to_a > best_offer_value)
            {   
                best_offer_value = counter_offer_value_to_a;
                a_gives = trade_good;
            }
        }

        if (a_gives != -1)
        {
            // the goods are exchanged.
            this.people[a].adjust_owned(b_gives,  b_gives_amount );
            this.people[b].adjust_owned(b_gives,  -b_gives_amount );
            this.people[a].adjust_owned(a_gives,  -counter_offers[a_gives] );
            this.people[b].adjust_owned(a_gives,  counter_offers[a_gives] );
            
            // reputation adjustments are made based on how good of a deal each party thinks they got.
            float a_opinion_a_gift = counter_offers[a_gives] * this.people[a].prices[b_gives];
            float b_opinion_a_gift = counter_offers[a_gives] * this.people[b].prices[b_gives];

            float rep_adjust_a = Utilities.fast_sigmoid(    a_opinion_b_gift - a_opinion_a_gift );
            float rep_adjust_b = Utilities.fast_sigmoid(    b_opinion_a_gift - b_opinion_b_gift );

            this.people[a].adjust_likes(b, rep_adjust_a);
            this.people[b].adjust_likes(a, rep_adjust_b);

            Console.WriteLine( $"{this.people[a].name} traded {counter_offers[a_gives]} {Content.item_names[a_gives]} to {this.people[b].name} for {b_gives} {Content.item_names[b_gives]}" );
            this.people[a].traded_this_turn = true;
            this.people[b].traded_this_turn = true;


            this.people[a].compile_needs();  
            this.people[b].compile_needs();  

        }
    }



    public void gossip(int a, int b, ref Random r)
    {


        this.introduce(a,b);


        const float const_gossip = 0.25f;

        // 1. small talk.
        // a says something that falls somewhere on their scale of personality characteristic.
        // b has a reputation adjustment based on the difference between their ideals and what a said.
        int topic = r.Next(0,Content.n_characteristics);
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

    }


      public void update_position_based_on_needs(int a, ref Random r)
    {
        Vector2 destination = this.people[a].position;
        bool go = false;

        int greatest_need = this.people[a].greatest_need();

        if (this.people[a].sources.ContainsKey(greatest_need))
        {
            int source = this.people[a].sources[greatest_need];
            go = true;
            destination = this.people[source].position;
        }
        else
        {
            this.people[a].sources.Add(greatest_need, r.Next(0,this.population_size ) );
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


    public void person_turn(int a, ref Random r)
    {

            this.people[a].compile_needs(); 

            if (print_player)
            {
                if (a == this.player)
                {
                    this.print_character_status(a);
                }
            }


            this.people[a].chatted_this_turn = false;
            this.people[a].traded_this_turn = false;




            for (int b = 0; b < this.population_size; b++  ) 
            {
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

                     
                        this.trade(a, b);
                        
                        if (! this.people[a].is_location )
                        {
                            this.gossip(a, b, ref r);

                           
                        }

                    } 
                }
            }

            if (! this.people[a].is_location )
            {
                this.people[a].bodily_functions();        
                this.update_position_based_on_needs(a, ref r);
            }

    }



    public void update(ref Random r)
    {


        for (int a = 0; a < this.population_size; a++  ) 
        {


            this.person_turn(a, ref r);
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
        this.world.setup(ref this.random);
        for (int i = 0; i < 1000; i++)
        {
            this.world.update(ref this.random);
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