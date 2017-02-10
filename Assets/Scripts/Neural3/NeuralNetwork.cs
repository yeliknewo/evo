using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Neural3
{
	public class NeuralNetwork
	{
		private int firstInputSize;
		private List<int> sizes;
		private List<List<double>> biases;
		private List<List<List<double>>> weights;

		public NeuralNetwork(List<int> sizes, int numInputs, int seed)
		{
			this.sizes = sizes;

			Random.InitState(seed);

			biases = new List<List<double>>();
			for (int layerIndex = 0; layerIndex < sizes.Count; layerIndex++)
			{
				List<double> biasesLayer = new List<double>();
				for (int neuronIndex = 0; neuronIndex < sizes[layerIndex]; neuronIndex++)
				{
					biasesLayer.Add(Random.Range(-1.0f, 1.0f));
				}
				biases.Add(biasesLayer);
			}

			weights = new List<List<List<double>>>();
			int numWeights = numInputs;
			for (int layerIndex = 0; layerIndex < sizes.Count; layerIndex++)
			{
				List<List<double>> weightsLayer = new List<List<double>>();
				for (int neuronIndex = 0; neuronIndex < sizes[layerIndex]; neuronIndex++)
				{
					List<double> weightsNeuron = new List<double>();
					for (int weightIndex = 0; weightIndex < numWeights; weightIndex++)
					{
						weightsNeuron.Add(Random.Range(-1.0f, 1.0f));
					}
					weightsLayer.Add(weightsNeuron);
				}
				weights.Add(weightsLayer);
				numWeights = sizes[layerIndex];
			}
		}

		public List<double> FeedForward(List<double> inputs)
		{
			for (int layerIndex = 0; layerIndex < biases.Count; layerIndex++)
			{
				List<double> z = DotMV2V(weights[layerIndex], inputs);

				for (int neuronIndex = 0; neuronIndex < biases[layerIndex].Count; neuronIndex++)
				{
					z[neuronIndex] += biases[layerIndex][neuronIndex];
				}
				inputs = SigmoidList(z);
			}
			return inputs;
		}

		public void UpdateBatch(List<List<double>> inputBatch, List<List<double>> targetOutputBatch, double eta)
		{
			Debug.Assert(inputBatch.Count == targetOutputBatch.Count, "Inputs and Outputs are different sizes");

			List<List<double>> nablaB = new List<List<double>>();

			foreach (List<double> biasesLayer in biases)
			{
				List<double> nablaBLayer = new List<double>();
				foreach (double bias in biasesLayer)
				{
					nablaBLayer.Add(0);
				}
				nablaB.Add(nablaBLayer);
			}

			List<List<List<double>>> nablaW = new List<List<List<double>>>();

			foreach (List<List<double>> weightsLayer in weights)
			{
				List<List<double>> nablaWLayer = new List<List<double>>();
				foreach (List<double> weightsNeuron in weightsLayer)
				{
					List<double> nablaWNeuron = new List<double>();
					foreach (double weight in weightsNeuron)
					{
						nablaWNeuron.Add(0);
					}
					nablaWLayer.Add(nablaWNeuron);
				}
				nablaW.Add(nablaWLayer);
			}

			{
				List<List<double>> deltaNablaB = new List<List<double>>();
				for (int layerIndex = 0; layerIndex < nablaB.Count; layerIndex++)
				{
					deltaNablaB.Add(new List<double>());
				}
				List<List<List<double>>> deltaNablaW = new List<List<List<double>>>();
				for (int layerIndex = 0; layerIndex < nablaW.Count; layerIndex++)
				{
					List<List<double>> deltaNablaWLayer = new List<List<double>>();
					for (int i = 0; i < nablaW[layerIndex].Count; i++)
					{
						deltaNablaWLayer.Add(new List<double>());
					}
					deltaNablaW.Add(deltaNablaWLayer);
				}
				for (int batchIndex = 0; batchIndex < inputBatch.Count; batchIndex++)
				{
					for (int layerIndex = 0; layerIndex < nablaB.Count; layerIndex++)
					{
						deltaNablaB[layerIndex].Clear();
						for (int neuronIndex = 0; neuronIndex < nablaB[layerIndex].Count; neuronIndex++)
						{
							deltaNablaB[layerIndex].Add(0);
						}
					}
					for (int layerIndex = 0; layerIndex < nablaW.Count; layerIndex++)
					{
						for (int neuronIndex = 0; neuronIndex < nablaW[layerIndex].Count; neuronIndex++)
						{
							deltaNablaW[layerIndex][neuronIndex].Clear();
							for (int weightIndex = 0; weightIndex < nablaW[layerIndex][neuronIndex].Count; weightIndex++)
							{
								deltaNablaW[layerIndex][neuronIndex].Add(0);
							}
						}
					}
					Backpropagate(inputBatch[batchIndex], targetOutputBatch[batchIndex], deltaNablaB, deltaNablaW);
					Debug.Assert(nablaB.Count == deltaNablaB.Count, "NablaB and DeltaNablaB are different sizes");
					for (int layerIndex = 0; layerIndex < nablaB.Count; layerIndex++)
					{
						Debug.Assert(nablaB[layerIndex].Count == deltaNablaB[layerIndex].Count, "NablaB Layer and DeltaNablaB Layer are different sizes");
						for (int neuronIndex = 0; neuronIndex < nablaB[layerIndex].Count; neuronIndex++)
						{
							nablaB[layerIndex][neuronIndex] += deltaNablaB[layerIndex][neuronIndex];
						}
					}
					Debug.Assert(nablaW.Count == deltaNablaW.Count, "NablaW and DeltaNablaW are different sizes");
					for (int layerIndex = 0; layerIndex < nablaW.Count; layerIndex++)
					{
						Debug.Assert(nablaW[layerIndex].Count == deltaNablaW[layerIndex].Count, "NablaW Layer and DeltaNablaW Layer are different sizes, NablaW Layer Count: " + nablaW[layerIndex].Count + ", DeltaNablaW Layer Count: " + deltaNablaW[layerIndex].Count);
						for (int neuronIndex = 0; neuronIndex < nablaW[layerIndex].Count; neuronIndex++)
						{
							Debug.Assert(nablaW[layerIndex][neuronIndex].Count == deltaNablaW[layerIndex][neuronIndex].Count, "NablaW Neuron and DeltaNablaW Neuron are different sizes");
							for (int weightIndex = 0; weightIndex < nablaW[layerIndex][neuronIndex].Count; weightIndex++)
							{
								nablaW[layerIndex][neuronIndex][weightIndex] += deltaNablaW[layerIndex][neuronIndex][weightIndex];
							}
						}
					}
				}

				for (int layerIndex = 0; layerIndex < biases.Count; layerIndex++)
				{
					for (int neuronIndex = 0; neuronIndex < biases[layerIndex].Count; neuronIndex++)
					{
						biases[layerIndex][neuronIndex] -= (eta / inputBatch.Count) * nablaB[layerIndex][neuronIndex];
					}
				}

				for (int layerIndex = 0; layerIndex < weights.Count; layerIndex++)
				{
					for (int neuronIndex = 0; neuronIndex < weights[layerIndex].Count; neuronIndex++)
					{
						for (int weightIndex = 0; weightIndex < weights[layerIndex][neuronIndex].Count; weightIndex++)
						{
							weights[layerIndex][neuronIndex][weightIndex] -= (eta / inputBatch.Count) * nablaW[layerIndex][neuronIndex][weightIndex];
						}
					}
				}
			}
		}

		private void Backpropagate(List<double> input, List<double> targetOutput, List<List<double>> deltaNablaB, List<List<List<double>>> deltaNablaW)
		{
			List<double> activation = new List<double>();
			activation.AddRange(input);
			List<List<double>> activations = new List<List<double>>();
			activations.Add(activation);

			List<List<double>> zValues = new List<List<double>>();

			for (int layerIndex = 0; layerIndex < biases.Count; layerIndex++)
			{
				List<double> z = DotMV2V(weights[layerIndex], activation);

				for (int neuronIndex = 0; neuronIndex < biases[layerIndex].Count; neuronIndex++)
				{
					z[neuronIndex] += biases[layerIndex][neuronIndex];
				}

				zValues.Add(z);

				activation = SigmoidList(z);
				activations.Add(activation);
			}

			List<double> delta = new List<double>();
			for (int activationIndex = 0; activationIndex < activations[activations.Count - 1].Count; activationIndex++)
			{
				delta.Add((activations[activations.Count - 1][activationIndex] - targetOutput[activationIndex]) * SigmoidPrime(zValues[zValues.Count - 1][activationIndex]));
			}

			{
				for (int neuronIndex = 0; neuronIndex < delta.Count; neuronIndex++)
				{
					deltaNablaB[deltaNablaB.Count - 1][neuronIndex] = delta[neuronIndex];
				}
				List<List<double>> m = DotVV2M(delta, activations[activations.Count - 2]);
				for (int neuronIndex = 0; neuronIndex < deltaNablaW[deltaNablaW.Count - 1].Count; neuronIndex++)
				{
					for (int weightIndex = 0; weightIndex < deltaNablaW[deltaNablaW.Count - 1][neuronIndex].Count; weightIndex++)
					{
						deltaNablaW[deltaNablaW.Count - 1][neuronIndex][weightIndex] = m[neuronIndex][weightIndex];
					}
				}
			}

			for (int layerIndex = 2; layerIndex < sizes.Count; layerIndex++)
			{
				List<double> z = zValues[zValues.Count - layerIndex];
				List<double> sp = SigmoidPrimeList(z);
				delta = DotMTV2V(weights[weights.Count - layerIndex + 1], delta);
				for (int deltaIndex = 0; deltaIndex < delta.Count; deltaIndex++)
				{
					delta[deltaIndex] *= sp[deltaIndex];
				}
				deltaNablaB[deltaNablaB.Count - layerIndex + 1] = delta;
				deltaNablaW[deltaNablaW.Count - layerIndex + 1] = DotVV2M(delta, activations[activations.Count - layerIndex]);
			}
		}

		private static List<double> DotMV2V(List<List<double>> matrix, List<double> vector)
		{
			List<double> result = new List<double>();
			for (int matrixIndex = 0; matrixIndex < matrix.Count; matrixIndex++)
			{
				double sum = 0;
				for (int vectorIndex = 0; vectorIndex < vector.Count; vectorIndex++)
				{
					sum += matrix[matrixIndex][vectorIndex] * vector[vectorIndex];
				}
				result.Add(sum);
			}
			return result;
		}

		private static List<double> DotMTV2V(List<List<double>> matrix, List<double> vector)
		{
			List<double> result = new List<double>();
			for (int vectorIndex = 0; vectorIndex < vector.Count; vectorIndex++)
			{
				double sum = 0;
				for (int matrixIndex = 0; matrixIndex < matrix.Count; matrixIndex++)
				{
					sum += matrix[matrixIndex][vectorIndex] * vector[vectorIndex];
				}
				result.Add(sum);
			}
			return result;
		}

		private static List<List<double>> DotVV2M(List<double> a, List<double> b)
		{
			List<List<double>> result = new List<List<double>>();
			for (int aIndex = 0; aIndex < a.Count; aIndex++)
			{
				List<double> resultLayer = new List<double>();
				for (int bIndex = 0; bIndex < b.Count; bIndex++)
				{
					resultLayer.Add(a[aIndex] * b[bIndex]);
				}
				result.Add(resultLayer);
			}
			return result;
		}

		private static List<double> SigmoidPrimeList(List<double> inputs)
		{
			List<double> result = new List<double>();
			foreach (double input in inputs)
			{
				result.Add(SigmoidPrime(input));
			}
			return result;
		}

		private static double SigmoidPrime(double input)
		{
			return Sigmoid(input) / (1 - Sigmoid(input));
		}

		private static List<double> SigmoidList(List<double> inputs)
		{
			List<double> outputs = new List<double>();
			foreach (double input in inputs)
			{
				outputs.Add(Sigmoid(input));
			}
			return outputs;
		}

		private static double Sigmoid(double input)
		{
			return 1.0 / (1.0 + Mathf.Exp(-(float)input));
		}
	}
}

