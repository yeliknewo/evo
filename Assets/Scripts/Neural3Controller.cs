using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Neural3;

public class Neural3Controller : MonoBehaviour
{
	private NeuralNetwork network;
	[SerializeField]
	private List<int> sizes;
	[SerializeField]
	private int numInputs;

	[SerializeField]
	private List<double> inputs;
	[SerializeField]
	private List<double> targetOutputs;
	[SerializeField]
	private double eta;
	[SerializeField]
	private int seed;

	void Start()
	{
		network = new NeuralNetwork(sizes, numInputs, seed);
	}

	void Update()
	{
		List<List<double>> inputBatch = new List<List<double>>();
		inputBatch.Add(inputs);
		List<List<double>> targetOutputBatch = new List<List<double>>();
		targetOutputBatch.Add(targetOutputs);
		network.UpdateBatch(inputBatch, targetOutputBatch, eta);
		List<double> result = network.FeedForward(inputs);
		foreach(double val in result)
		{
			Debug.Log(val);
		}
	}
}
