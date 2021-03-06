﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QLearningNotSure;

public class QNeural3Controller : MonoBehaviour
{
	private QLearner learner;
	private List<Action> actions;
	private World world;

	[SerializeField]
	private int eValue;
	[SerializeField]
	private double alpha;
	[SerializeField]
	private double gamma;
	[SerializeField]
	private double eta;
	[SerializeField]
	private double mom;
	[SerializeField]
	private List<int> hiddenSizes;
	[SerializeField]
	private int numInputs;
	[SerializeField]
	private int seed;
	[SerializeField]
	private float mapSize;
	[SerializeField]
	private float winDistance;
	[SerializeField]
	private int winReward;
	[SerializeField]
	private int stepReward;
	[SerializeField]
	private int loseReward;
	[SerializeField]
	private int inverseDeltaTime;
	[SerializeField]
	private int epochsPerEra;
	[SerializeField]
	private int eras;
	[SerializeField]
	private float counterStep;
	[SerializeField]
	private int erasPerRun;
	[SerializeField]
	private bool runDemo;

	private int internalEras;

	private int currentEpoch;

	private float counter;

	private State nextState;

	[SerializeField]
	private GameObject goalPrefab;
	[SerializeField]
	private GameObject playerPrefab;

	void Start()
	{
		world = new World(mapSize, winDistance, winReward, stepReward, loseReward);
		nextState = GetRandomState();
		actions = new List<Action>();
		for (int x = -1; x <= 1; x++)
		{
			for (int y = -1; y <= 1; y++)
			{
				if (x != 0 || y != 0)
				{
					Debug.Log("Action: (" + x + "," + y + ")");
					actions.Add(new Action(new Vector2(x, y) / inverseDeltaTime, Vector2.zero));
				}
			}
		}
		learner = new QLearner(eValue, alpha, gamma, eta, actions, hiddenSizes, numInputs, seed);

		GameObject goal = Instantiate(goalPrefab);
		goal.transform.position = nextState.GetGoalPos();
		goalPrefab = goal;
		GameObject player = Instantiate(playerPrefab);
		player.transform.position = nextState.GetPlayerPos();
		playerPrefab = player;
	}

	State GetRandomState()
	{
		return new State(Random.insideUnitCircle * mapSize, Vector2.zero, mapSize);
	}

	void Update()
	{
		if (runDemo)
		{
			if (internalEras < erasPerRun)
			{
				for (int epochs = 0; epochs < epochsPerEra; epochs++)
				{
					learner.RunEpoch(world, GetRandomState(), eValue, alpha, gamma, eta, mom);
				}
				eras++;
				internalEras++;
			}
			while (internalEras >= erasPerRun && counter <= Time.time)
			{
				counter = Time.time + counterStep;
				nextState = learner.RunStep(world, nextState, eValue, alpha, gamma, eta, mom);
				playerPrefab.transform.position = nextState.GetPlayerPos();
				goalPrefab.transform.position = nextState.GetGoalPos();
				if (world.IsTerminal(nextState))
				{
					nextState = GetRandomState();
					internalEras = 0;
				}
			}
		}
		else
		{
			for(int epochs = 0; epochs < epochsPerEra; epochs++)
			{
				learner.RunEpoch(world, GetRandomState(), eValue, alpha, gamma, eta, mom);
			}
			eras++;
		}
	}
}
