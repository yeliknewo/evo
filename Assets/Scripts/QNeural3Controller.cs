using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QLearningNotSure;

public class QNeural3Controller : MonoBehaviour
{
	private QLearner learner;
	private List<Action> actions;
	private State startingState;
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
	private int epochPerEra;
	[SerializeField]
	private int currentEra;

	private int currentEpoch;
	private bool training;

	[SerializeField]
	private GameObject goalPrefab;
	[SerializeField]
	private GameObject playerPrefab;

	void Start()
	{
		world = new World(mapSize, winDistance, winReward, stepReward, loseReward);
		startingState = GetRandomState();
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
		training = true;

		GameObject goal = Instantiate(goalPrefab);
		goal.transform.position = startingState.GetGoalPos();
		goalPrefab = goal;
		GameObject player = Instantiate(playerPrefab);
		player.transform.position = startingState.GetPlayerPos();
		playerPrefab = player;
	}

	State GetRandomState()
	{
		return new State(Random.insideUnitCircle * mapSize, Vector2.zero, mapSize);
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			training = !training;
			startingState = GetRandomState();
		}
		if (training)
		{
			while (currentEpoch < epochPerEra)
			{
				learner.RunEpoch(world, startingState);
				currentEpoch++;
			}

			Debug.Log("Era Done");
			currentEpoch = 0;
			currentEra++;
		}
		else
		{
			if(world.IsTerminal(startingState))
			{
				startingState = GetRandomState();
			}
			Action action = learner.GetNextAction(startingState);
			startingState = world.GetNextState(startingState, action);
			playerPrefab.transform.position = startingState.GetPlayerPos();
			goalPrefab.transform.position = startingState.GetGoalPos();
		}
	}
}
