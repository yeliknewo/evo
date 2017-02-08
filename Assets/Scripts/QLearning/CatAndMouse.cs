using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatAndMouse : MonoBehaviour
{
	private QLearner learner;
	private RLWorld world;

	void Start()
	{
		world = new RLWorld(10, 10, 20);
		learner = new QLearner(world);
	}

	private void Update()
	{
		Debug.Log("Starting Update");
		learner.RunEpoch();
		Debug.Log("Ending Update");
	}
}
