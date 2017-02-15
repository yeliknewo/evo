using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessArray<T>
{
	Dictionary<int, T> data;
	List<int> dimSizes;
	private T startingVal;
	private T minVal;

	public EndlessArray(List<int> dimSizes, T startingVal, T minVal)
	{
		this.dimSizes = dimSizes;
		this.startingVal = startingVal;
		this.minVal = minVal;
		data = new Dictionary<int, T>();
	}

	public T GetAt(List<int> indices)
	{
		int index = GetIndex(indices);
		if (index == -1)
		{
			return minVal;
		}
		if (data.ContainsKey(index))
		{
			return data[index];
		}
		else
		{
			return startingVal;
		}
	}

	public void SetAt(List<int> indices, T val)
	{
		int index = GetIndex(indices);
		if (index == -1)
		{
			return;
		}
		if (data.ContainsKey(index))
		{
			data[index] = val;
		}
		else
		{
			data.Add(index, val);
		}
	}

	private int GetIndex(List<int> indices)
	{
		int index = 0;
		int pow = 0;
		for (int i = 0; i < indices.Count; i++)
		{
			if (indices[i] >= dimSizes[i] || indices[i] < 0)
			{
				return -1;
			}
			pow += (int)Mathf.Pow(dimSizes[i] * 10, i);
			index += indices[i] * pow;
		}
		return index;
	}
}
