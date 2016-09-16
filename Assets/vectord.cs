using System.Collections.Generic;
using System.Diagnostics;

public class VectorD : List<double>
{
    public static VectorD operator +(VectorD a, VectorD b)
{

    Debug.Assert(a.Count == b.Count);
   
	VectorD result = new VectorD();

	for(int i = 0; i<a.Count; ++i)
	{
		result.Add(a[i] + b[i]);
	}

	return result;
}
    public static VectorD operator *(double a, VectorD b)
{
    VectorD result = new VectorD();

    for (int i = 0; i < b.Count; ++i)
    {
        result.Add(a * b[i]);
    }

    return result;
}

    public static VectorD operator *(VectorD a, double b)
{
    return b * a;
}

public static VectorD operator /(VectorD a, double b)
{
    VectorD result = new VectorD();

    for (int i = 0; i < a.Count; ++i)
    {
        result.Add(a[i] / b);
    }

    return result;
}
}
