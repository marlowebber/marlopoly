using System.Runtime.InteropServices;
using System.Numerics;





class Person
{

 public   Dictionary<int, float> owns        = new Dictionary<int, float>(); 
 public   Dictionary<int, float> likes       = new Dictionary<int, float>(); 
 public   Dictionary<int, float> knows       = new Dictionary<int, float>(); 
 public   Dictionary<int, float> values      = new Dictionary<int, float>(); 
 public   Dictionary<int, float> needs       = new Dictionary<int, float>(); 
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

    public void bodily_functions()
    {
        this.hungry = Utilities.clamp( this.hungry + eats_per_turn, 0.0f, 2.0f );
        this.thirsty = Utilities.clamp( this.thirsty + drinks_per_turn, 0.0f, 2.0f );
        this.hedgy = Utilities.clamp( this.hedgy + smokes_per_turn, 0.0f, 2.0f );
    }

    public void compile_needs()
    {
        // rebuild the list of needs from various agreements and bodily needs.
        this.needs.Clear();

        foreach (Dictionary<int, float> debt in this.agreements.Values)
        {
            foreach (int resource in debt.Keys)
            {
                this.needs[resource] += debt[resource];
            }
        }

        if (this.hungry > 1.0f)
        {
            this.needs[ (int)Content.Items.Chips ] += (int)this.hungry;
        }
        if (this.thirsty > 1.0f)
        {
            this.needs[ (int)Content.Items.Beer ] += (int)this.thirsty;
        }
        if (this.hedgy > 1.0f)
        {
            this.needs[ (int)Content.Items.Smokes ] += (int)this.hedgy;
        }


    }



   
    public void gossip_with(ref Person partner)
    {

    }

    public void update_position_based_on_needs()
    {

    }
}



class World
{
    int time = 0;

    int population_size = 100;

    List<Person> people = new List<Person>();

    public void setup()
    {
        for (int i = 0; i < population_size; i++)
        {
            people.Append(new Person());
        }
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





    public void update(ref Random r)
    {
        for (int a = 0; a < population_size; a++  ) 
        {
            for (int b = 0; b < population_size; b++  ) 
            {
                if (a != b)
                {
                    if (System.Numerics.Vector2.Distance(this.people[a].position, this.people[b].position) < 1.0f)
                    {
                        // choose something out of a's list of needs to trade for.
                    }
                    this.people[a].compile_needs();
                    this.people[a].bodily_functions();   
                    this.people[a].update_position_based_on_needs();
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



    private void detect_OS()
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

    private void run()
    {
        for (int i = 0; i < 1000; i++)
        {
            this.world.update(ref this.random);
        }
    }



    public void Main()
    {
        this.detect_OS();
    }

    
}



