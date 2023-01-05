using System.Runtime.InteropServices;
using System.Numerics;
using System;


static class Utilities
{


public static int rng_int(int input)
{
	Random r = new Random();
	return r.Next(input);
}

public static float rng_float()
{

	Random r = new Random();
	return r.NextSingle();
}

public static char rng_char()
{

	Random r = new Random();

	return (char) (  32 + r.Next(94) );
}

public static bool rng_bool()
{

	Random r = new Random();

	bool result = false;
	if (r.Next(2) > 0)
	{
		result = true;
	}
	return result;


}


public static float abs(float input)
{
    if (input > 0.0f)
    {
        return input;
    }
    return input * -1.0f;
}

public static float clamp (float input, float min, float max)
{
	if (input < min)
	{
		return min;
	}
	else if (input > max)
	{
		return max;
	}
	return input;
}

public static float fast_sigmoid(float input)
{
	// https://stackoverflow.com/questions/10732027/fast-sigmoid-algorithm
	float a = (input / (1 + abs(input)));
	return  a;
}

}

