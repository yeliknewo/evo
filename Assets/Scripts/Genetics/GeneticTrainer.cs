using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Neural1
{
	public class GeneticTrainer
	{
		private List<NeuralNetwork> nextGeneration;
		private List<Species> currentGeneration;

		public GeneticTrainer(List<NeuralNetwork> firstGeneration)
		{
			nextGeneration = firstGeneration;
			currentGeneration = new List<Species>();
		}

		public void Train(List<double> rewards)
		{
			if (rewards.Count != nextGeneration.Count)
			{
				Debug.LogError("Rewards and Next Generation aren't correct Size");
			}

			//Debug.Log("Next Gen Size: " + nextGeneration.Count);
			for (int i = 0; i < rewards.Count; i++)
			{
				currentGeneration.Add(new Species(nextGeneration[i], rewards[i]));
			}
			nextGeneration.Clear();

			currentGeneration.Sort((x, y) => y.GetFitness().CompareTo(x.GetFitness()));

			int keepers = 2;

			for (int i = 0; i < currentGeneration.Count / keepers; i++)
			{
				for (int j = 0; j < keepers; j++)
				{
					nextGeneration.Add(currentGeneration[j].Cross(currentGeneration[i]));
				}
			}
			for (int i = 0; i < currentGeneration.Count % keepers; i++)
			{
				nextGeneration.Add(currentGeneration[Random.Range(0, currentGeneration.Count)].Cross(currentGeneration[Random.Range(0, currentGeneration.Count)]));
			}

			//Debug.Log("Next Gen Size: " + nextGeneration.Count);
			currentGeneration.Clear();
		}

		public List<NeuralNetwork> GetNextGeneration()
		{
			return nextGeneration;
		}
	}
}
