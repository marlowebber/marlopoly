using System.Runtime.InteropServices;
using System.Numerics;
using System;




class Person
{

 public   Dictionary<int, float> owns        = new Dictionary<int, float>(); 
 public   Dictionary<int, float> likes       = new Dictionary<int, float>(); 
 public   Dictionary<int, int> sources       = new Dictionary<int, int>(); 
 public   Dictionary<int, float> values      = new Dictionary<int, float>(); 
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

bool is_location = false;

    public Person(string name, ref Random r)
    {
        this.name = name;
        this.hungry = (float)r.NextDouble();
        this.thirsty = (float)r.NextDouble();
        this.hedgy = (float)r.NextDouble();

        for (int i = 0; i < Content.n_characteristics; i++)
        {
            this.personality.Add(i, (float)r.NextDouble());
            this.ideals.Add(i, (float)r.NextDouble());
        }
    }


    public void bodily_functions()
    {
        this.hungry = Utilities.clamp( this.hungry + eats_per_turn, 0.0f, 2.0f );
        this.thirsty = Utilities.clamp( this.thirsty + drinks_per_turn, 0.0f, 2.0f );
        this.hedgy = Utilities.clamp( this.hedgy + smokes_per_turn, 0.0f, 2.0f );
    }

    public void add_need(int need, float amount, float priority)
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
                this.add_need(resource ,  debt[resource] , this.likes[debtor] );
            }
        }

        if (this.hungry > 1.0f)
        {
            this.add_need((int)Content.Items.Chips , 1.0f,(int)this.hungry );
        }
        if (this.thirsty > 1.0f)
        {
            this.add_need((int)Content.Items.Beer  , 1.0f,(int)this.hungry );
        }
        if (this.hedgy > 1.0f)
        {
            this.add_need((int)Content.Items.Smokes  , 1.0f,(int)this.hungry );
        }


    }




  
}



class World
{
    int time = 0;

    int population_size = 0;

    List<Person> people = new List<Person>();

    public void setup(ref Random r)
    {
        this.people.Clear();
        this.population_size = 0;
        foreach (string name in Content.person_names)
        {
            // Console.WriteLine(name);
            this.people.Add(new Person(name, ref r));
            this.population_size++;
        }
        int memow= 88;
    }

    public void trade(int a, int b, int b_gives, float b_gives_amount )
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


        if (b_gives_amount == 0)
        {
            // if b doesn't have the thing, and b was a's source, refer a to b's source.
            if (this.people[a].sources[b_gives] == b && this.people[b].sources[b_gives]!= a )
            {
                this.people[a].sources[b_gives] = this.people[b].sources[b_gives];
            }
        }

        // calculate how much each party thinks the offer is worth.
		float a_opinion_b_gift = b_gives_amount * this.people[a].values[b_gives];
		float b_opinion_b_gift = b_gives_amount * this.people[b].values[b_gives];

        // counter-offers are prepared.
        Dictionary<int, float> counter_offers = new Dictionary<int, float>();
        foreach (int trade_good in this.people[a].owns.Keys)
        {
            if (trade_good != b_gives)
            {
                float counter_offer_quantity = b_opinion_b_gift / this.people[b].values[trade_good];

                // scale the amount to the quantity that the giving party has.
                if (counter_offer_quantity > 0.0f)
                {
                    counter_offer_quantity = Utilities.clamp(counter_offer_quantity, 0.0f, this.people[a].owns[trade_good]);
                }
                else
                {
                    counter_offer_quantity = Utilities.clamp(counter_offer_quantity, 0.0f, this.people[b].owns[trade_good]);
                }

                counter_offers.Add(trade_good, counter_offer_quantity);
            }
        }

        // a selects a counter offer.
        int a_gives = -1;
        float best_offer_value = 0.0f;
        foreach (int trade_good in counter_offers.Keys)
        {
            float counter_offer_value_to_a = counter_offers[trade_good] * this.people[a].values[trade_good];
            if (counter_offer_value_to_a > best_offer_value)
            {   
                best_offer_value = counter_offer_value_to_a;
                a_gives = trade_good;
            }
        }

        if (a_gives != -1)
        {
            // the goods are exchanged.
            this.people[a].owns[b_gives] += b_gives_amount;
            this.people[b].owns[b_gives]    -= b_gives_amount;
            
            this.people[a].owns[a_gives] -= counter_offers[a_gives];
            this.people[b].owns[b_gives]    += counter_offers[a_gives];

            // reputation adjustments are made based on how good of a deal each party thinks they got.
            float a_opinion_a_gift = counter_offers[a_gives] * this.people[a].values[b_gives];
            float b_opinion_a_gift = counter_offers[a_gives] * this.people[b].values[b_gives];

            float rep_adjust_a = Utilities.fast_sigmoid(    a_opinion_b_gift - a_opinion_a_gift );
            float rep_adjust_b = Utilities.fast_sigmoid(    b_opinion_a_gift - b_opinion_b_gift );

            this.people[a].likes[b] += rep_adjust_a;
            this.people[b].likes[a] += rep_adjust_b;
        }
    }



    public void gossip(int a, int b, ref Random r)
    {
        const float const_gossip = 0.25f;

        // 1. small talk.
        // a says something that falls somewhere on their scale of personality characteristic.
        // b has a reputation adjustment based on the difference between their ideals and what a said.
        int topic = r.Next(0,Content.n_characteristics);
        this.people[b].likes[a] += (1.0f - ((this.people[a].personality[topic] - this.people[b].ideals[topic]) / Content.n_characteristics )) * const_gossip ;

        // 2. talk about a person
        // https://stackoverflow.com/questions/10685142/c-sharp-dictionaries-intersect
        Dictionary<int, float> mutual_friends =  
        this.people[a].likes.Where(x => this.people[b].likes.ContainsKey(x.Key))
                .ToDictionary(x => x.Key, x => x.Value + this.people[b].likes[x.Key]);

        // c should be a randomly chosen mutual friend.
        int ci = r.Next(0, mutual_friends.Count() );
        int i = 0;
        int c = -1;
        foreach (int friend in mutual_friends.Keys)
        {
            if (i == ci)
            {
                c = friend;
                break;
            }
            i++;
        }

        if (c != -1)
        {
            this.people[a].likes[b] += this.people[b].likes[c] * this.people[a].likes[c] * const_gossip;
            this.people[b].likes[a] += this.people[a].likes[c] * this.people[b].likes[c] * const_gossip;

            this.people[a].likes[c] += this.people[b].likes[c] * this.people[a].likes[b] * const_gossip;
            this.people[b].likes[c] += this.people[a].likes[c] * this.people[b].likes[a] * const_gossip;
        }

    }


      public void update_position_based_on_needs(int a, int greatest_need)
    {
        Vector2 destination = this.people[a].position;
        bool go = false;


        if (greatest_need != -1)
        {
            int source = this.people[a].sources[greatest_need];
            if (source != -1)
            {
                go = true;
                destination = this.people[source].position;
            }
            
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

        }

    }



    public void update(ref Random r)
    {
        for (int a = 0; a < this.population_size; a++  ) 
        {
            for (int b = 0; b < this.population_size; b++  ) 
            {
                if (a != b)
                {
                    if (System.Numerics.Vector2.Distance(this.people[a].position, this.people[b].position) < 1.0f)
                    {

                        // if the two don't know each other, well they do now! Add them to each other's dictionaries.
                        if (! this.people[a].likes.ContainsKey(b))
                        {
                            this.people[a].likes.Add(b, 0.0f);
                        }
                        if (! this.people[b].likes.ContainsKey(a))
                        {
                            this.people[b].likes.Add(a, 0.0f);
                        }

                        this.people[a].compile_needs();  

                        this.gossip(a, b, ref r);

                        // choose something out of a's list of needs to trade for.
                        int greatest_need = -1;
                        float greated_need_priority = 0.0f;
                        foreach (int need in this.people[a].needs_priorities.Keys)
                        {
                            if (this.people[a].needs_priorities[need] > greated_need_priority)
                            {
                                greated_need_priority = Utilities.abs( this.people[a].needs_priorities[need] );
                                greatest_need = need;
                            }
                        }
                        
                        if (greatest_need != -1)
                        {
                            this.trade(a, b, greatest_need, this.people[a].needs_quantities[greatest_need]);
                            this.update_position_based_on_needs(a, greatest_need);
                        }

                        this.people[a].bodily_functions();
                    } 
                }
            }
        }
        this.time++;
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

    public void run()
    {
        this.world.setup(ref this.random);
        for (int i = 0; i < 1000; i++)
        {
            this.world.update(ref this.random);
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