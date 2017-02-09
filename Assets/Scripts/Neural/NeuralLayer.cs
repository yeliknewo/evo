using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Neural1
{
	public class NeuralLayer
	{
		private List<Neuron> neurons;

		public NeuralLayer(List<List<double>> weights)
		{
			neurons = new List<Neuron>();
			for (int i = 0; i < weights.Count; i++)
			{
				neurons.Add(new Neuron(weights[i]));
			}
		}

		public NeuralLayer(int inputCount, int outputCount)
		{
			neurons = new List<Neuron>();
			for (int i = 0; i < outputCount; i++)
			{
				neurons.Add(new Neuron(inputCount));
			}
		}

		public List<double> Fire(List<double> inputs)
		{
			//if (inputs.count != neurons.count)
			//{
			//	debug.logerror("invalid number of inputs, in: " + inputs.count + ", neurons: " + neurons.count);
			//}

			List<double> outputs = new List<double>();

			foreach (Neuron neuron in neurons)
			{
				outputs.Add(neuron.Fire(inputs));
			}
			//Debug.Log("In: " + inputs.Count + ", Neurons: " + neurons.Count + ", Out: " + outputs.Count);

			return outputs;
		}

		public List<List<double>> GetWeights()
		{
			List<List<double>> result = new List<List<double>>();

			foreach (Neuron neuron in neurons)
			{
				result.Add(neuron.GetWeights());
			}

			return result;
		}

		public List<double> GetBiases()
		{
			List<double> result = new List<double>();

			foreach (Neuron neuron in neurons)
			{
				result.Add(neuron.GetBias());
			}

			return result;
		}

		public List<List<double>> GetWeightsAndBias()
		{
			List<List<double>> result = new List<List<double>>();

			foreach (Neuron neuron in neurons)
			{
				result.Add(neuron.GetWeightsAndBias());
			}

			return result;
		}

		public void SetWeightsAndBias(List<List<double>> weightsAndBias)
		{
			for (int i = 0; i < weightsAndBias.Count; i++)
			{
				neurons[i].SetWeightsAndBias(weightsAndBias[i]);
			}
		}

		public int GetBiasCount()
		{
			int sum = 0;

			foreach (Neuron neuron in neurons)
			{
				sum += neuron.GetBiasCount();
			}

			return sum;
		}

		public int GetWeightCount()
		{
			int sum = 0;

			foreach (Neuron neuron in neurons)
			{
				sum += neuron.GetWeightCount();
			}

			return sum;
		}
	}

}
