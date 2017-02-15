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
	private int eras;
	[SerializeField]
	private float counterStep;
	[SerializeField]
	private int erasPerRun;
	[SerializeField]
	private GameObject background;

	private int internalEras;

	private QLearner learner;
	private World world;

	private State nextState;

	private float counter;

	[SerializeField]
	private GameObject playerPrefab;
	[SerializeField]
	private double startingQ;

	void Start()
	{
		world = new World(mapSize);
		List<Action> actions = new List<Action>();
		actions.Add(new Action(Vector2.up));
		actions.Add(new Action(Vector2.down));
		actions.Add(new Action(Vector2.left));
		actions.Add(new Action(Vector2.right));
		List<int> tableDimensions = new List<int>();
		tableDimensions.Add(mapSize);
		tableDimensions.Add(mapSize);
		//tableDimensions.Add(mapSize);
		//tableDimensions.Add(mapSize);
		tableDimensions.Add(actions.Count);
		learner = new QLearner(actions, tableDimensions, startingQ);
		nextState = GetRandomState();
		playerPrefab = Instantiate(playerPrefab);
		counter = Time.time;
		background.transform.position = Vector2.one * mapSize / 2;
		FindObjectOfType<Camera>().transform.position = (Vector3)(Vector2.one * mapSize / 2) + Vector3.forward * FindObjectOfType<Camera>().transform.position.z;
		FindObjectOfType<Camera>().orthographicSize = mapSize / 2.0f;
	}

	private State GetRandomState()
	{
		return new State(new Vector2(Random.Range(0, mapSize), Random.Range(0, mapSize)), Vector2.one * mapSize / 2);//new Vector2(Random.Range(0, mapSize), Random.Range(0, mapSize)));
	}

	void Update()
	{
		world.UpdateWorld(winReward, stepReward, loseReward);
		if (internalEras >= erasPerRun)
		{
			if (counter <= Time.time)
			{
				
				counter = Time.time + counterStep;
				nextState = learner.RunStep(world, nextState, eValue, alpha, gamma);
				playerPrefab.transform.position = nextState.GetPlayerPos();
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
				learner.RunEpoch(world, GetRandomState(), eValue, alpha, gamma);
			}
			eras++;
			internalEras++;
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
				float val = (float)((bestQ - minQ) / (maxQ - minQ));
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
}
