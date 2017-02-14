using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tabular1;

public class Tabular1Controller : MonoBehaviour
{
	[SerializeField]
	private int mapSize;
	[SerializeField]
	private int winReward;
	[SerializeField]
	private int stepReward;
	[SerializeField]
	private int loseReward;
	[SerializeField]
	private int eValue;
	[SerializeField]
	private double alpha;
	[SerializeField]
	private double gamma;
	[SerializeField]
	private int epochsPerEra;
	[SerializeField]
	private int maxMoves;
	[SerializeField]
	private int eras;
	[SerializeField]
	private float counterStep;
	[SerializeField]
	private int erasPerRun;
	[SerializeField]
	private GameObject background;

	private QLearner learner;
	private World world;

	private State startingState;

	private State nextState;

	private float counter;

	[SerializeField]
	private GameObject playerPrefab;
	[SerializeField]
	private double startingQ;

	void Start()
	{
		world = new World(mapSize, maxMoves, winReward, stepReward, loseReward);
		List<Action> actions = new List<Action>();
		actions.Add(new Action(Vector2.up));
		actions.Add(new Action(Vector2.down));
		actions.Add(new Action(Vector2.left));
		actions.Add(new Action(Vector2.right));
		List<int> tableDimensions = new List<int>();
		tableDimensions.Add(mapSize);
		tableDimensions.Add(mapSize);
		tableDimensions.Add(maxMoves);
		tableDimensions.Add(actions.Count);
		learner = new QLearner(alpha, gamma, actions, tableDimensions, startingQ);
		startingState = new State(Vector2.one * (mapSize - 1), 0);
		nextState = startingState;
		playerPrefab = Instantiate(playerPrefab);
		counter = Time.time;
		background.transform.position = Vector2.one * mapSize / 2;
		FindObjectOfType<Camera>().transform.position = (Vector3)(Vector2.one * mapSize / 2) + Vector3.forward * FindObjectOfType<Camera>().transform.position.z;
		FindObjectOfType<Camera>().orthographicSize = mapSize / 2.0f;
	}

	void Update()
	{
		if (eras % erasPerRun == 0)
		{
			if (counter <= Time.time)
			{
				UpdateBackground();
				counter = Time.time + counterStep;
				State state = nextState;
				Action action = learner.GetNextAction(state);
				int actionIndex = learner.GetNextActionIndex(state);
				StateReward stateReward = world.GetNextStateReward(state, action);
				nextState = stateReward.GetState();
				int reward = stateReward.GetReward();
				double qValue = learner.GetQValue(state, actionIndex);
				double maxQValue = learner.GetMaxQValue(nextState);
				double newQValue = learner.GetNewQValue(maxQValue, qValue, alpha, gamma, reward);
				learner.UpdateQValues(state, actionIndex, newQValue);
				playerPrefab.transform.position = nextState.GetPlayerPos();
				if (world.IsTerminal(nextState))
				{
					startingState = new State(new Vector2(Random.Range(0, mapSize), Random.Range(0, mapSize)), 0);
					nextState = startingState;
					eras++;
				}
			}
		}
		else
		{
			//for (int epochs = 0; epochs < epochsPerEra; epochs++)
			//{
			//	learner.RunEpoch(world, startingState, eValue);
			//}
			eras++;
		}
	}

	private void UpdateBackground()
	{
		SpriteRenderer renderer = background.GetComponent<SpriteRenderer>();
		Texture2D texture = new Texture2D(mapSize + 1, mapSize + 1);
		double maxQ = double.MinValue;
		double minQ = double.MaxValue;
		for (int x = 0; x <= mapSize; x++)
		{
			for (int y = 0; y <= mapSize; y++)
			{
				double upperQ = learner.GetMaxQValue(new State(new Vector2(x, y), 0));
				double lowerQ = learner.GetMaxQValue(new State(new Vector2(x, y), 0));
				if (maxQ < upperQ)
				{
					maxQ = upperQ;
				}
				if (minQ > lowerQ)
				{
					minQ = lowerQ;
				}
			}
		}
		for (int x = 0; x <= mapSize; x++)
		{
			for (int y = 0; y <= mapSize; y++)
			{
				Color color;
				double bestQ = learner.GetMaxQValue(new State(new Vector2(x, y), 0));
				//if (bestQ < (maxQ + minQ) / 2)
				//{
				//	float val = (float)(bestQ / minQ);
				//	color = Color.Lerp(Color.grey, Color.red, val);
				//}
				//else
				{
					float val = (float)(bestQ / maxQ);
					color = Color.Lerp(Color.red, Color.green, val);
				}
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
}
