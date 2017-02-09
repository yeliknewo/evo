using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Neural1
{
	public class NeuralNetwork
	{
		private List<NeuralLayer> layers;

		public NeuralNetwork(List<List<List<double>>> weightsAndBiases)
		{
			layers = new List<NeuralLayer>();

			foreach (List<List<double>> layerData in weightsAndBiases)
			{
				layers.Add(new NeuralLayer(layerData));
			}
		}

		public NeuralNetwork(int firstInputCount, List<int> layerSizes)
		{
			layers = new List<NeuralLayer>();

			int currentInputCount = firstInputCount;

			foreach (int layerSize in layerSizes)
			{
				//Debug.Log("Current Input Count: " + currentInputCount + ", Layer Size: " + layerSize);

				layers.Add(new NeuralLayer(currentInputCount, layerSize));
				currentInputCount = layerSize;
			}
		}

		public List<double> Fire(List<double> inputs)
		{
			foreach (NeuralLayer layer in layers)
			{
				inputs = layer.Fire(inputs);
			}

			return inputs;
		}

		public List<List<List<double>>> GetWeights()
		{
			List<List<List<double>>> result = new List<List<List<double>>>();

			foreach (NeuralLayer layer in layers)
			{
				result.Add(layer.GetWeights());
			}

			return result;
		}

		public List<List<double>> GetBiases()
		{
			List<List<double>> result = new List<List<double>>();

			foreach (NeuralLayer layer in layers)
			{
				result.Add(layer.GetBiases());
			}

			return result;
		}

		public List<List<List<double>>> GetWeightsAndBiases()
		{
			List<List<List<double>>> result = new List<List<List<double>>>();

			foreach (NeuralLayer layer in layers)
			{
				result.Add(layer.GetWeightsAndBias());
			}

			return result;
		}

		public void SetWeightsAndBiases(List<List<List<double>>> weightsAndBiases)
		{
			for (int i = 0; i < weightsAndBiases.Count; i++)
			{
				layers[i].SetWeightsAndBias(weightsAndBiases[i]);
			}
		}

		public int GetBiasCount()
		{
			int sum = 0;

			foreach (NeuralLayer layer in layers)
			{
				sum += layer.GetBiasCount();
			}

			return sum;
		}

		public int GetWeightCount()
		{
			int sum = 0;

			foreach (NeuralLayer layer in layers)
			{
				sum += layer.GetWeightCount();
			}

			return sum;
		}
	}

}
