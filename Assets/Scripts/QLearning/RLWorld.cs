using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RLWorld : MonoBehaviour
{
	private const int NUM_OBJECTS = 6;
	private const int NUM_ACTIONS = 8;
	private const int WALL_TRIALS = 100;
	private const double INIT_VALS = 0.0;

	private int bx, by, mx, my, cx, cy, chx, chy, hx, hy;
	private bool gotCheese = false;

	private int catScore = 0, mouseScore = 0;
	private int cheeseReward = 50, deathPenalty = 100;

	List<int> state;
	double waitingReward;
	bool[,] walls;

	public RLWorld(int x, int y, int numWalls)
	{
		bx = x;
		by = y;
		makeWalls(x, y, numWalls);

		ResetState();
	}

	public List<int> GetDimension()
	{
		List<int> retDim = new List<int>(NUM_OBJECTS + 1);

		int i;

		for (i = 0; i < NUM_OBJECTS;)
		{
			retDim[i++] = bx;
			retDim[i++] = by;
		}
		retDim[i] = NUM_ACTIONS;

		return retDim;
	}

	public List<int> GetNextState(int action)
	{
		int[] d = GetCoords(action);

		int ax = d[0], ay = d[1];

		if (IsLegal(ax, ay))
		{
			mx = ax;
			my = ay;
		}
		else
		{
			//illegal action
		}

		moveCat();
		waitingReward = calcReward();

		if ((mx == chx) && (my == chy))
		{
			d = GetRandomPos();
			chx = d[0];
			chx = d[1];
		}

		if ((mx == cx) && (my == cy))
		{
			d = GetRandomPos();
			mx = d[0];
			my = d[1];
		}

		return GetState();
	}

	public double GetReward(int action)
	{
		return GetReward();
	}

	private double GetReward()
	{
		return waitingReward;
	}

	public bool IsValidAction(int action)
	{
		int[] d = GetCoords(action);
		return IsLegal(d[0], d[1]);
	}

	public bool IsOver()
	{
		return EndGame();
	}

	public List<int> ResetState()
	{
		catScore = 0;
		mouseScore = 0;
		SetRandomPos();
		return GetState();
	}

	public double GetInitValues()
	{
		return INIT_VALS;
	}

	private List<int> GetState()
	{
		state = new List<int>(NUM_OBJECTS);
		state[0] = mx;
		state[1] = my;
		state[2] = cx;
		state[3] = cy;
		state[4] = chx;
		state[5] = chy;
		return state;
	}

	private double CalcReward()
	{
		double newReward = 0;

		if ((mx == chx) && (my == chy))
		{
			mouseScore++;
			newReward += cheeseReward;
		}
		if ((cx == mx) && (cy == my))
		{
			catScore++;
			newReward -= deathPenalty;
		}
		if ((mx == hx) && (my == hy) && (gotCheese))
		{
			newReward += 100;
		}

		return newReward;
	}

	private void SetRandomPos()
	{
		int[] d = GetRandomPos();
		cx = d[0];
		cy = d[1];
		d = GetRandomPos();
		mx = d[0];
		my = d[1];
		d = GetRandomPos();
		chx = d[0];
		chy = d[1];
		d = GetRandomPos();
		hx = d[0];
		hy = d[1];
	}

	private bool IsLegal(int x, int y)
	{
		return ((x >= 0) && (x < bx) && (y >= 0) && (y < by)) && (!walls[x, y]);
	}

	private bool EndGame()
	{
		return ((cx == mx) && (cy == my));
	}

	int[] GetRandomPos()
	{
		int nx, ny;
		nx = Random.Range(0, bx);
		ny = Random.Range(0, by);
		for (int trials = 0; (!IsLegal(nx, ny)) && (trials < WALL_TRIALS); trials++)
		{
			nx = Random.Range(0, bx);
			ny = Random.Range(0, by);
		}
		return new int[] { nx, ny };
	}

	private void MakeWAlls(int xDim, int yDim, int numWalls)
	{
		walls = new bool[xDim, yDim];

		for (int t = 0; t < WALL_TRIALS; t++)
		{
			for (int i = 0; i < walls.GetLength(0); i++)
			{
				for (int j = 0; j < walls.GetLength(1); j++)
				{
					walls[i, j] = false;
				}
			}

			float xMid = xDim / 2f;
			float yMid = yDim / 2f;

			for (int i = 0; i < numWalls; i++)
			{
				int[] d = GetRandomPos();

				double dx2 = Mathf.Pow(xMid - d[0], 2);
				double dy2 = Mathf.Pow(yMid - d[1], 2);

				double dropperc = Mathf.Sqrt((float)(dx2 + dy2) / (xMid * xMid + yMid * yMid));
				if (Random.value < dropperc)
				{
					i--;
					continue;
				}

				walls[d[0], d[1]] = true;
			}
			if (IsValidWallSet(walls))
			{
				break;
			}
		}
	}

	private bool IsValidWallSet(bool[,] walls)
	{
		bool[,] c;
		c = new bool[walls.GetLength(0), walls.GetLength(1)];

		for (int i = 0; i < walls.GetLength(0); i++)
		{
			for (int j = 0; j < walls.GetLength(1); j++)
			{
				c[i, j] = w[i, j];
			}
		}

		bool found = false;
		for (int i = 0; i < c.GetLength(0); i++)
		{
			for (int j = 0; j < c.GetLength(1); j++)
			{
				if(!c[i, j])
				{
					FillNeighbors(c, i, j);
					found = true;
					goto search;
				}
			}
		}
		search:
		if(!found)
		{
			return false;
		}

		for(int i = 0; i< c.GetLength(0); i++)
		{
			for(int j = 0; j < c.GetLength(1);j++)
			{

			}
		}
	}
}
