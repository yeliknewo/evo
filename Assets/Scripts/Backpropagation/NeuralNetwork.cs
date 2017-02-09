using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Neural2
{
	public class NeuralNetwork
	{
		private List<int> sizes;
		private List<double> biases;
		private List<int> biasLayerIndexCache;

		private List<double> weights;
		private List<int> weightLayerIndexCache;
		private List<int> weightLayerNumWeights;

		public NeuralNetwork(List<int> sizes, int numInputs)
		{
			this.sizes = sizes;

			{
				biases = new List<double>();
				biasLayerIndexCache = new List<int>();
				int currentIndex = 0;
				for (int layerIndex = 0; layerIndex < sizes.Count; layerIndex++)
				{
					for (int neuronIndex = 0; neuronIndex < sizes[layerIndex]; neuronIndex++)
					{
						biases.Add(Random.value);
					}
					biasLayerIndexCache.Add(currentIndex);
					currentIndex += sizes[layerIndex];
				}
			}

			{
				weights = new List<double>();
				weightLayerIndexCache = new List<int>();
				weightLayerNumWeights = new List<int>();
				int numWeights = numInputs;
				int currentIndex = 0;
				for (int layerIndex = 0; layerIndex < sizes.Count; layerIndex++)
				{
					for (int neuronIndex = 0; neuronIndex < sizes[layerIndex]; neuronIndex++)
					{
						for (int weightIndex = 0; weightIndex < numWeights; weightIndex++)
						{
							weights.Add(Random.value);
						}
					}
					weightLayerIndexCache.Add(currentIndex);
					currentIndex += sizes[layerIndex] * numWeights;
					weightLayerNumWeights.Add(numWeights);
					numWeights = sizes[layerIndex];
				}
			}
		}

		public void UpdateBatch(List<List<double>> inputsBatch, List<List<double>> targetOutputBatch, double eta)
		{
			Debug.Assert(inputsBatch.Count == targetOutputBatch.Count, "Inputs and Outputs are different sizes");

			List<double> nablaB = new List<double>();

			foreach (double bias in biases)
			{
				nablaB.Add(0);
			}

			List<double> nablaW = new List<double>();

			foreach (double weight in weights)
			{
				nablaW.Add(0);
			}

			{
				List<double> deltaNablaB = new List<double>();
				List<double> deltaNablaW = new List<double>();
				for (int batchIndex = 0; batchIndex < inputsBatch.Count; batchIndex++)
				{
					Backpropagate(inputsBatch[batchIndex], targetOutputBatch[batchIndex], deltaNablaB, deltaNablaW);
					for (int nablaBIndex = 0; nablaBIndex < deltaNablaB.Count; nablaBIndex++)
					{
						nablaB[nablaBIndex] += deltaNablaB[nablaBIndex];
					}
					for (int nablaWIndex = 0; nablaWIndex < deltaNablaW.Count; nablaWIndex++)
					{
						nablaW[nablaWIndex] += deltaNablaW[nablaWIndex];
					}
				}
			}

			for (int biasesIndex = 0; biasesIndex < biases.Count; biasesIndex++)
			{
				biases[biasesIndex] -= (eta / inputsBatch.Count) * nablaB[biasesIndex];
			}

			for (int weightsCount = 0; weightsCount < weights.Count; weightsCount++)
			{
				weights[weightsCount] -= (eta / inputsBatch.Count) * nablaW[weightsCount];
			}
		}

		private void Backpropagate(List<double> inputs, List<double> targetOutput, List<double> deltaNablaB, List<double> deltaNablaW)
		{
			deltaNablaB.Clear();
			for (int biasesIndex = 0; biasesIndex < biases.Count; biasesIndex++)
			{
				deltaNablaB.Add(0);
			}

			deltaNablaW.Clear();
			for (int weightsIndex = 0; weightsIndex < weights.Count; weightsIndex++)
			{
				deltaNablaW.Add(0);
			}

			List<double> activation = new List<double>();
			activation.AddRange(inputs);
			List<List<double>> activations = new List<List<double>>();
			activations.Add(activation);

			List<List<double>> zs = new List<List<double>>();

			for (int layerIndex = 0; layerIndex < sizes.Count; layerIndex++)
			{
				int layerSize = sizes[layerIndex];

				List<double> localBiases = new List<double>();
				for (int neuronIndex = 0; neuronIndex < layerSize; neuronIndex++)
				{
					localBiases.Add(biases[neuronIndex]);
				}
			}

			for (int layerIndex = 0; layerIndex < sizes.Count; layerIndex++)
			{
				List<double> zLayer = new List<double>();
				for (int neuronIndex = 0; neuronIndex < sizes[layerIndex]; neuronIndex++)
				{
					List<double> weights = GetWeights(layerIndex, neuronIndex);
					double bias = GetBias(layerIndex, neuronIndex);
					double z = Dot(weights, activation) + bias;
					zLayer.Add(z);
				}
				zs.Add(zLayer);
				activation = SigmoidList(zLayer);
				activations.Add(activation);
			}

			List<double> delta = CostDerivative(activations[activations.Count - 1], targetOutput);
			for (int deltaIndex = 0; deltaIndex < delta.Count; deltaIndex++)
			{
				delta[deltaIndex] *= SigmoidPrime(zs[zs.Count - 1][deltaIndex]);
			}
			deltaNablaB.AddRange(delta);
			for (int deltaIndex = 0; deltaIndex < delta.Count; deltaIndex++)
			{
				deltaNablaW.Add(Dot(delta, activations[activations.Count - 2]));
			}

			for(int layerIndex = 2; layerIndex < sizes.Count; layerIndex++)
			{
				List<double> z = zs[zs.Count - layerIndex];
				List<double> sp = SigmoidPrimeList(z);
				delta = Dot(GetWeights(sizes.Count - layerIndex + 1), delta);
			}
		}

		private double Dot(List<double> a, List<double> b)
		{
			Debug.Assert(a.Count == b.Count, "A and B are different Sizes");

			double sum = 0;

			for (int vectorIndex = 0; vectorIndex < a.Count; vectorIndex++)
			{
				sum += a[vectorIndex] * b[vectorIndex];
			}

			return sum;
		}

		private double SigmoidPrime(double input)
		{
			return Sigmoid(input) * (1 - Sigmoid(input));
		}

		private List<double> SigmoidPrimeList(List<double> inputs)
		{
			List<double> result = new List<double>();

			foreach (double input in inputs)
			{
				result.Add(SigmoidPrime(input));
			}

			return result;
		}

		private List<double> CostDerivative(List<double> outputActivations, List<double> targetOutput)
		{
			Debug.Assert(outputActivations.Count == targetOutput.Count, "OutputActivations and TargetOutput are different sizes");

			List<double> result = new List<double>();
			for (int outputIndex = 0; outputIndex < targetOutput.Count; outputIndex++)
			{
				result.Add(outputActivations[outputIndex] - targetOutput[outputIndex]);
			}
			return result;
		}

		private List<double> SigmoidList(List<double> inputs)
		{
			List<double> result = new List<double>();
			foreach (double input in inputs)
			{
				result.Add(Sigmoid(input));
			}
			return result;
		}

		private double Sigmoid(double input)
		{
			return 1.0 / (1.0 + Mathf.Exp((float)-input));
		}

		private double GetBias(int layer, int neuron)
		{
			return biases[biasLayerIndexCache[layer] + neuron];
		}

		private List<double> GetWeights(int layer, int neuron)
		{
			List<double> result = new List<double>();

			for (int weightIndex = 0; weightIndex < weightLayerNumWeights[layer]; weightIndex++)
			{
				result.Add(weightLayerIndexCache[weightIndex] + neuron * weightLayerNumWeights[weightIndex]);
			}

			return result;
		}

		//public void backpropagate(list<double> inputs, list<double> outputs, double eta)
		//{
		//	list<list<list<double>>> weights = network.getweights();
		//	list<list<double>> biases = network.getbiases();

		//	list<double> nablaw = new list<double>();
		//	for (int i = 0; i < weights.count; i++)
		//	{
		//		nablaw.add(0);
		//	}

		//	list<double> nablab = new list<double>();
		//	for (int i = 0; i < biases.count; i++)
		//	{
		//		nablab.add(0);
		//	}

		//	list<double> deltanablaw = new list<double>();
		//	list<double> deltanablab = new list<double>();

		//	backprop(network, inputs, outputs, deltanablaw, deltanablab);

		//	for (int i = 0; i < deltanablaw.count; i++)
		//	{
		//		nablaw[i] += deltanablaw[i];
		//	}

		//	for (int i = 0; i < deltanablab.count; i++)
		//	{
		//		nablab[i] += deltanablab[i];
		//	}

		//	for (int i = 0; i < weights.count; i++)
		//	{
		//		weights[i] -= eta * nablaw[i];
		//	}
		//}

		//private void backprop(list<double> inputs, list<double> outputs, list<double> deltanablaw, list<double> deltanablab)
		//{

		//}

	}

}
