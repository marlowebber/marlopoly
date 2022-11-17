

static class Content
{

enum Characteristics 
{
  Cute,
  Funny,
  Smart,
  Tough,
  Cool,
}


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




List<string> person_names = new List<string>(new string[]  {

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
	"Swamp Beast",
});





Dictionary<int,Dictionary<int,float>> recipes = new Dictionary<int,Dictionary<int,float>>();


Dictionary<int,float> recipe_smokes = new Dictionary<int,float> 
{
    [(int)Items.Spin] = 1.0f,
    [(int)Items.Papers] = 1.0f,
    [(int)Items.Filters] = 1.0f,
};

recipes.Add( (int)Items.Smokes, recipe_smokes);




}