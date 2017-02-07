using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Species
{
	private double fitness;
	private List<List<List<double>>> networkData;

	public Species(NeuralNetwork network, double fitness)
	{
		this.fitness = fitness;
		this.networkData = network.GetWeightsAndBiases();
	}

	public double GetFitness()
	{
		return fitness;
	}

	public NeuralNetwork Cross(Species other)
	{
		List<List<List<double>>> result = networkData;
		List<List<List<double>>> otherWeights = other.networkData;

		for (int x = 0; x < result.Count; x++)
		{
			for (int y = 0; y < result[x].Count; y++)
			{
				for (int z = 0; z < result[x][y].Count; z++)
				{
					double val = Random.value;
					if (val < 0.05)
					{
						result[x][y][z] += Random.Range(-1.0f, 1.0f);
					}
					else if (val < 0.5)
					{
						result[x][y][z] = otherWeights[x][y][z];
					}
				}
			}
		}

		return new NeuralNetwork(result);
	}
}
