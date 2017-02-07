using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Neuron
{
	private List<double> weights;
	private double bias;

	public Neuron(List<double> weightsAndBias)
	{
		SetWeightsAndBias(weightsAndBias);
	}

	public Neuron(int inputCount)
	{
		weights = new List<double>();
		for (int i = 0; i < inputCount; i++)
		{
			weights.Add(Random.Range(-1.0f, 1.0f));
		}
		bias = Random.Range(-1.0f, 1.0f);
	}

	private double Sigmoid(float input)
	{
		return 2.0 / (1 + Mathf.Exp(-input));
	}

	public double Fire(List<double> inputs)
	{
		//if (inputs.Count != weights.Count)
		//{
		//	Debug.LogError("Invalid Number of Inputs, In: " + inputs.Count + ", Weights: " + weights.Count);
		//}

		double sum = bias;
		for (int i = 0; i < inputs.Count; i++)
		{
			sum += inputs[i] * this.weights[i];
		}
		return Sigmoid((float)sum);
	}

	private void SetWeights(List<double> weights)
	{
		this.weights = weights;
	}

	private void SetBias(double bias)
	{
		this.bias = bias;
	}

	public void SetWeightsAndBias(List<double> weightsAndBias)
	{
		this.weights = weightsAndBias.GetRange(0, weightsAndBias.Count - 1);
		this.bias = weightsAndBias[weightsAndBias.Count - 1];
	}

	private List<double> GetWeights()
	{
		return weights;
	}

	private double GetBias()
	{
		return bias;
	}

	public List<double> GetWeightsAndBias()
	{
		List<double> result = new List<double>();
		result.AddRange(GetWeights().ToArray());
		result.Add(bias);
		return result;
	}
}
