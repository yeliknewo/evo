using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class NeuralController : MonoBehaviour
{
	private NeuralNetwork network;
	[SerializeField]
	private double fitness;

	[SerializeField]
	private int firstInputCount;
	[SerializeField]
	private List<int> layerSizes;

	[SerializeField]
	private float speed;

	private Vector3 startPos;
	private Quaternion startRotation;
	private Vector3 startVelocity;

	private void Awake()
	{
		FindObjectOfType<CentralNeuralController>().AddNeuralController(this);
	}

	private void Start()
	{
		startPos = transform.position;
		startRotation = transform.rotation;
		startVelocity = GetRigidbody().velocity;
		network = new NeuralNetwork(firstInputCount, layerSizes);
		fitness = 0;
	}

	private void Update()
	{
		fitness += transform.position.y * Time.deltaTime * 10;
		fitness -= (Mathf.Abs(transform.position.x) + Mathf.Abs(transform.position.
			z)) * Time.deltaTime;

		HandleOutputs(network.Fire(GetInputs()));
	}

	private List<double> GetInputs()
	{
		//pos x, pos z
		List<double> inputs = new List<double>();

		inputs.Add(transform.position.x);
		inputs.Add(transform.position.y);
		inputs.Add(transform.position.z);
		inputs.Add(GetRigidbody().velocity.x);
		inputs.Add(GetRigidbody().velocity.y);
		inputs.Add(GetRigidbody().velocity.z);

		return inputs;
	}

	private void HandleOutputs(List<double> outputs)
	{
		float x = (float)outputs[0];
		float y = (float)outputs[1];
		float theta = Mathf.Atan2(y * 2 - 1, x * 2 - 1);
		Vector3 vel = new Vector3(Mathf.Cos(theta), 0, Mathf.Sin(theta)) * speed;
		GetRigidbody().AddForce(vel, ForceMode.Acceleration);
	}

	private Rigidbody GetRigidbody()
	{
		return GetComponent<Rigidbody>();
	}

	public NeuralNetwork GetNetwork()
	{
		return network;
	}

	public void SetNetwork(NeuralNetwork network)
	{
		this.network = network;
		fitness = 0;
		transform.position = startPos;
		transform.rotation = startRotation;
		GetRigidbody().velocity = startVelocity;
	}

	public double GetFitness()
	{
		return fitness;
	}
}
