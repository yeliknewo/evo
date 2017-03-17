using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QLearningNeural4;

public class Neural4Controller : MonoBehaviour
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
	private int hiddenSize;
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
	[SerializeField]
	private GameObject background;

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
		learner = new QLearner(actions, hiddenSize, numInputs, seed);

		GameObject goal = Instantiate(goalPrefab);
		goal.transform.position = nextState.GetGoalPos();
		goalPrefab = goal;
		GameObject player = Instantiate(playerPrefab);
		player.transform.position = nextState.GetPlayerPos();
		playerPrefab = player;
		background.transform.position = (Vector3)(Vector2.one * mapSize / 2) + Vector3.forward;
		FindObjectOfType<Camera>().transform.position = (Vector3)(Vector2.one * mapSize / 2) + Vector3.forward * FindObjectOfType<Camera>().transform.position.z;
		FindObjectOfType<Camera>().orthographicSize = mapSize / 2.0f;
	}

	private void UpdateBackground()
	{
		SpriteRenderer renderer = background.GetComponent<SpriteRenderer>();
		Texture2D texture = new Texture2D((int)mapSize + 1, (int)mapSize + 1);
		double maxQ = double.MinValue;
		double minQ = double.MaxValue;
		for (int x = 0; x <= mapSize; x++)
		{
			for (int y = 0; y <= mapSize; y++)
			{
				double q = learner.GetMaxQValue(nextState.MovePlayerTo(new Vector2(x, y)));
				if (maxQ < q)
				{
					maxQ = q;
				}
				if (minQ > q)
				{
					minQ = q;
				}
			}
		}
		for (int x = 0; x <= mapSize; x++)
		{
			for (int y = 0; y <= mapSize; y++)
			{
				double bestQ = learner.GetMaxQValue(nextState.MovePlayerTo(new Vector2(x, y)));
				float val;
				if (maxQ == minQ) {
					val = 0.5f;
				}
				else
				{
					val = (float)((bestQ - minQ) / (maxQ - minQ));
				}
				Color color = Color.Lerp(Color.red, Color.green, val);
				texture.SetPixel(x, y, color);
			}
		}
		texture.filterMode = FilterMode.Point;
		texture.wrapMode = TextureWrapMode.Clamp;
		texture.Apply();
		Rect rect = new Rect(Vector2.zero, Vector2.one * mapSize);
		Vector2 pivot = Vector2.one / 2;
		Sprite sprite = Sprite.Create(texture, rect, pivot, 1);
		renderer.sprite = sprite;
	}

	State GetRandomState()
	{
		return new State(new Vector2(Random.Range(0.0f, mapSize), Random.Range(0.0f, mapSize)), Vector2.one * mapSize / 2.0f, mapSize);
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
				UpdateBackground();
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
					UpdateBackground();
				}
			}
		}
		else
		{
			for (int epochs = 0; epochs < epochsPerEra; epochs++)
			{
				learner.RunEpoch(world, GetRandomState(), eValue, alpha, gamma, eta, mom);
			}
			eras++;
			UpdateBackground();
		}
	}
}
