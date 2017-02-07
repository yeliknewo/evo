using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CentralNeuralController : MonoBehaviour
{
	private List<NeuralController> controllers;
	private GeneticTrainer trainer;

	[SerializeField]
	private float timer;

	[SerializeField]
	private float maxTime;

	public CentralNeuralController()
	{
		controllers = new List<NeuralController>();
	}

	public void AddNeuralController(NeuralController neural)
	{
		controllers.Add(neural);
	}

	private void Awake()
	{
		timer = Time.time + maxTime;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space) || ShouldTrain() || timer < Time.time)
		{
			double maxFitness = double.MinValue;
			foreach (NeuralController controller in controllers)
			{
				if (maxFitness < controller.GetFitness())
				{
					maxFitness = controller.GetFitness();
				}
			}
			Debug.Log("Fitness: " + maxFitness);
			UpdateNetworks();
			timer = Time.time + maxTime;
		}
	}

	private bool ShouldTrain()
	{
		//foreach (NeuralController controller in controllers)
		//{
		//	if (controller.GetFitness() < 0)
		//	{
		//		return true;
		//	}
		//}
		return false;
	}

	public void UpdateNetworks()
	{
		if (trainer == null)
		{
			List<NeuralNetwork> networks = new List<NeuralNetwork>();
			foreach (NeuralController controller in controllers)
			{
				networks.Add(controller.GetNetwork());
			}
			trainer = new GeneticTrainer(networks);
		}

		List<double> rewards = new List<double>();
		foreach (NeuralController controller in controllers)
		{
			rewards.Add(controller.GetFitness());
		}
		trainer.Train(rewards);

		List<NeuralNetwork> next = trainer.GetNextGeneration();

		for (int i = 0; i < next.Count; i++)
		{
			controllers[i].SetNetwork(next[i]);
		}
	}
}
