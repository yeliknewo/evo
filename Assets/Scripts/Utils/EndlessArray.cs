using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessArray<T>
{
	Dictionary<int, T> data;
	List<int> dimSizes;

	public EndlessArray(List<int> dimSizes)
	{
		this.dimSizes = dimSizes;
		data = new Dictionary<int, T>();
	}

	public T GetAt(List<int> indices)
	{
		int index = GetIndex(indices);
		if (data.ContainsKey(index))
		{
			return data[index];
		}
		else
		{
			return default(T);
		}
	}

	public void SetAt(List<int> indices, T val)
	{
		int index = GetIndex(indices);
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
		for (int i = 0; i < indices.Count; i++)
		{
			index += indices[i] * (int)Mathf.Pow(dimSizes[i], i);
		}
		return index;
	}
}
