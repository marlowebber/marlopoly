


class Recipe
{
	Dictionary<int, float> input = new Dictionary<int, float>();
	Dictionary<int, float> output = new Dictionary<int, float>();

	public Recipe(Dictionary<int, float> input , Dictionary<int, float> output )
	{
		this.input = input;
		this.output = output;
	}
}


static class Content
{


	static Dictionary<int, float> ingredients_smokes = new Dictionary<int, float>
	{
		{ (int)Content.Items.Filters, 1.0f },
		{ (int)Content.Items.Papers, 1.0f },
		{ (int)Content.Items.Spin, 1.0f },
	};

	static Dictionary<int, float> products_smokes = new Dictionary<int, float>
	{
		{ (int)Content.Items.Smokes, 1.0f },
	};
	
	static Dictionary<int, Recipe> recipes = new Dictionary<int, Recipe>
	{
		{(int) Content.Items.Smokes, new Recipe(ingredients_smokes, products_smokes) }
	};


public enum Characteristics 
{
  Cute,
  Funny,
  Smart,
  Tough,
  Cool,
};

public const int n_characteristics = 5;

public enum Items 
{
Cash,
  Smokes,
  Filters,
  Papers,
  Spin,
  Beer,
  Chips,
  Lighter,
}

public static readonly List<string> item_names = new List<string> ( new[]
{
"Cash",
"Smokes",
"Filters",
"Papers",
"Spin",
"Beer",
"Chips",
"Lighter",
});

public const int n_items = 8;



public static readonly List<string> person_names = new List<string> ( new[]
{
	"Brandon",
	"Kyle",
	"Keelan",
	"Jonah",
	"Jeremy",
	"Molly",
	"Holly",
	"Kaylah",
	"Mikayla",
	"Bree",
	"Shaun",
	"Jake",
	"Stephen",
	"Mitchell",
	"Bryce",
	"Brittney",
	"Ella",
	"Kaliah",
	"Emily",
	"Rayleigh",
	"River",
	"Morgan",
	"Zen",
	"Ziggy",
	"Ollie",
	"Alex",
	"Alisha",
	"Amber",
	"Ellie",
	"Maddy",
	"Sarah",
	"Michelle",
	"Hayden",
	"Patrick",
	"Luke",
	"Connor",
	"Jarrad",
	"Sam",
	"Lewis",
	"Jay",
	"Krissy",
	"Ruby",
	"Julian",
	"Ben",
	"Jessica",
	"Lauren",
	"Jesse",
	"Ryan",
	"Olivia",
	"Danielle",
	"Jas",
	"Khalla",
	"Philippa",
	"Amy",
	"Lili",
	"Arden",
	"Jayden",
	"Jack",
	"Storm",
	"Zack",
	"Elise",
	"Nicola",
	"Trey",
	"Kai",
	"Justin",
	"Alia",
	"Isaac",
	"Monisha",
	"Josh",
	"Charlie",
	"Nathan",
	"Rebecca",
	"Peter",
	"Elena",
	"Lina",
	"Karlee",
	"Nicky",
	"Dylan",
	"Lindsay",
	"Rowan",
	"Anne",
	"Mittens",
	"CJ",
	"Cooper",
	"Siena",
	"Bianca",
	"Jasmine",
	"Meg",
	"Cat",
	"Swamp Monster",
	"Pastaforelli",
});


public const int n_person_names = 100;


public static readonly List<string> location_names = new List<string> ( new[]
{
	"The Lord Raglan Hotel",
	"The Valley",
	"Nite Owl Convenience",
	"The Vending Machine",
});

public const int n_place_names = 4;



// static readonly Dictionary<int,Dictionary<int,float>> recipes = new Dictionary<int,Dictionary<int,float>>();


// Dictionary<int,float> recipe_smokes = new Dictionary<int,float> 
// {
//     [(int)Items.Spin] = 1.0f,
//     [(int)Items.Papers] = 1.0f,
//     [(int)Items.Filters] = 1.0f,
// };

// recipes.Add( (int)Items.Smokes, recipe_smokes);




}