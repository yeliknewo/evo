using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RLWorld
{
	private const int NUM_OBJECTS = 6;
	private const int NUM_ACTIONS = 8;
	private const int WALL_TRIALS = 100;
	private const double INIT_VALS = 0.0;

	private int bx, by, mx, my, cx, cy, chx, chy;

	private int catScore = 0, mouseScore = 0;
	private int cheeseReward = 50, deathPenalty = 100;
	private int maxMouseScore = 0;

	List<int> state;
	double waitingReward;
	bool[,] walls;

	public RLWorld(int x, int y, int numWalls)
	{
		bx = x;
		by = y;
		MakeWalls(x, y, numWalls);

		ResetState();
	}

	public List<int> GetDimension()
	{
		List<int> retDim = new List<int>(NUM_OBJECTS + 1);

		int i;

		for (i = 0; i < NUM_OBJECTS;i += 2)
		{
			retDim.Add(bx);
			retDim.Add(by);
		}
		retDim.Add(NUM_ACTIONS);

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

		MoveCat();
		waitingReward = CalcReward();

		if ((mx == chx) && (my == chy))
		{
			d = GetRandomPos();
			chx = d[0];
			chx = d[1];
		}

		//Debug.Log("RLWorld: bx: " + bx + ", by: " + by + ", mx: " + mx + ", my: " + my + ", cx: " + cx + ", cy: " + cy + ", chx: " + chx + ", chy: " + chy + ", hx: " + hx + ", hy: " + hy);

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
		if (mouseScore > maxMouseScore)
		{
			maxMouseScore = mouseScore;
			Debug.Log("New Max Mouse Score: " + maxMouseScore);
		}
		catScore = 0;
		mouseScore = 0;
		SetRandomPos();
		return GetState();
	}

	public double GetInitValues()
	{
		return INIT_VALS;
	}

	private int[] GetNewPos(int x, int y, int tx, int ty)
	{
		int ax = x;
		int ay = y;

		if (tx == x)
		{
			ax = x;
		}
		else
		{
			ax += (tx - x) / Mathf.Abs(tx - x);
		}
		if (ty == y)
		{
			ay = y;
		}
		else
		{
			ay += (ty - y) / Mathf.Abs(ty - y);
		}

		if (IsLegal(ax, ay))
		{
			return new int[] { ax, ay };
		}

		while (true)
		{
			ax = x;
			ay = y;
			ax += Random.Range(-1, 1);
			ay += Random.Range(-1, 1);

			if (IsLegal(ax, ay))
			{
				return new int[] { ax, ay };
			}
		}
	}

	private void MoveCat()
	{
		int[] d = GetNewPos(cx, cy, mx, my);
		cx = d[0];
		cy = d[1];
	}

	private void MoveMouse()
	{
		int[] d = GetNewPos(mx, my, chx, chy);
		mx = d[0];
		my = d[1];
	}

	private int MouseAction()
	{
		int[] d = GetNewPos(mx, my, chx, chy);
		return GetAction(d[0] - mx, d[1] - my);
	}

	private int GetAction(int x, int y)
	{
		int[,] vals = { { 7, 0, 1 }, { 6, 0, 2 }, { 5, 4, 3 } };
		if ((x < -1) || (x > 1) || (y < -1) || (y > 1) || ((y == 0) && (x == 0)))
		{
			return -1;
		}
		int retVal = vals[y + 1, x + 1];
		return retVal;
	}

	private int[] GetCoords(int action)
	{
		int ax = mx;
		int ay = my;

		switch (action)
		{
			case 0: ay = my - 1; break;
			case 1: ay = my - 1; ax = mx + 1; break;
			case 2: ax = mx + 1; break;
			case 3: ay = my + 1; ax = mx + 1; break;
			case 4: ay = my + 1; break;
			case 5: ay = my + 1; ax = mx - 1; break;
			case 6: ax = mx - 1; break;
			case 7: ay = my - 1; ax = mx - 1; break;
		}
		return new int[] { ax, ay };
	}

	private List<int> GetState()
	{
		state = new List<int>();
		state.Add(mx);
		state.Add(my);
		state.Add(cx);
		state.Add(cy);
		state.Add(chx);
		state.Add(chy);
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

	private void MakeWalls(int xDim, int yDim, int numWalls)
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
		Debug.Log("Walls");
		for(int x = 0; x< walls.GetLength(0); x++)
		{
			for(int y = 0; y< walls.GetLength(1); y++)
			{
				Debug.Log("X: " + x + ", Y: " + y + ", ?: " + walls[x, y]);
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
				c[i, j] = walls[i, j];
			}
		}

		bool found = false;
		for (int i = 0; i < c.GetLength(0); i++)
		{
			for (int j = 0; j < c.GetLength(1); j++)
			{
				if (!c[i, j])
				{
					FillNeighbors(c, i, j);
					found = true;
					goto search;
				}
			}
		}
	search:
		if (!found)
		{
			return false;
		}

		for (int i = 0; i < c.GetLength(0); i++)
		{
			for (int j = 0; j < c.GetLength(1); j++)
			{
				if (!c[i, j])
				{
					return false;
				}
			}
		}
		return true;
	}

	private void FillNeighbors(bool[,] c, int x, int y)
	{
		c[x, y] = true;

		for (int i = x - 1; i <= x + 1; i++)
		{
			for (int j = y - 1; j <= y + 1; j++)
			{
				if ((i >= 0) && (i < c.GetLength(0)) && (j >= 0) && (j < c.GetLength(1)) && (!c[i, j]))
				{
					FillNeighbors(c, i, j);
				}
			}
		}
	}
}

