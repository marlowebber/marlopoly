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

    public void bodily_functions(ref Random r)
    {

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
            this.needs[ (int)Items.Chips ] += 1.0f;
        }
        if (this.thirsty > 1.0f)
        {
            this.needs[ (int)Items.Beer ] += 1.0f;
        }
        if (this.hedgy > 1.0f)
        {
            this.needs[ (int)Items.Smokes ] += 1.0f;
        }


    }

    public void trade_with(Person partner)
    {

    }

    public void gossip_with(Person partner)
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

    List<Person> population = new List<Person>();

    public void setup()
    {
        for (int i = 0; i < population_size; i++)
        {
            population.Append(new Person());
        }
    }



    public void update(ref Random r)
    {
        foreach (Person p in population) 
        {
            foreach (Person q in population) 
            {
                if (p != q)
                {
                    if (System.Numerics.Vector2.Distance(p.position, q.position) < 1.0f)
                    {
                        p.trade_with(q);
                        p.gossip_with(q);
                    }
                   p.compile_needs();
                   p.bodily_functions(ref r);   
                   p.update_position_based_on_needs();
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



