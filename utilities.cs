static class Utilities
{

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

