using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tabular1
{
	public class QLearner
	{
		private EndlessArray<double> qValues;
		private List<Action> actions;
		private double alpha;
		private double gamma;

		public QLearner(double alpha, double gamma, List<Action> actions, List<int> tableDimensions, double startingQ)
		{
			this.alpha = alpha;
			this.gamma = gamma;
			this.actions = actions;
			qValues = new EndlessArray<double>(tableDimensions, startingQ, -startingQ);
		}

		public void RunEpoch(World world, State state, int eValue)
		{
			while (!world.IsTerminal(state))
			{
				int actionIndex;
				if (Random.Range(0, eValue) == 0)
				{
					actionIndex = Random.Range(0, actions.Count);
				}
				else
				{
					actionIndex = GetNextActionIndex(state);
				}
				StateReward stateReward = world.GetNextStateReward(state, actions[actionIndex]);
				State nextState = stateReward.GetState();
				int reward = stateReward.GetReward();
				double qValue = GetQValue(state, actionIndex);
				double maxQValue = GetMaxQValue(nextState);
				double newQValue = GetNewQValue(maxQValue, qValue, alpha, gamma, reward);
				UpdateQValues(state, actionIndex, newQValue);
				state = nextState;
			}
		}

		public void UpdateQValues(State state, int actionIndex, double newQ)
		{
			List<int> inputs = state.GetInputs();
			inputs.Add(actionIndex);
			qValues.SetAt(inputs, newQ);
		}

		public double GetNewQValue(double maxQ, double q, double alpha, double gamma, int reward)
		{
			return q + alpha * (reward + gamma * maxQ - q);
		}

		public double GetMaxQValue(State state)
		{
			List<int> inputs = state.GetInputs();
			double maxQ = double.MinValue;
			for (int actionIndex = 0; actionIndex < actions.Count; actionIndex++)
			{
				inputs.Add(actionIndex);
				double q = GetQValue(inputs);
				if (maxQ < q)
				{
					maxQ = q;
				}
				inputs.RemoveAt(inputs.Count - 1);
			}
			return maxQ;
		}

		public double GetMinQValue(State state)
		{
			List<int> inputs = state.GetInputs();
			double minQ = double.MaxValue;
			for (int actionIndex = 0; actionIndex < actions.Count; actionIndex++)
			{
				inputs.Add(actionIndex);
				double q = GetQValue(inputs);
				if (minQ > q)
				{
					minQ = q;
				}
				inputs.RemoveAt(inputs.Count - 1);
			}
			return minQ;
		}

		public int GetNextActionIndex(State state)
		{
			double maxQ = double.MinValue;
			int maxIndex = 0;
			for (int i = 0; i < actions.Count; i++)
			{
				double q = GetQValue(state, i);
				if (maxQ < q)
				{
					maxQ = q;
					maxIndex = i;
				}
			}
			return maxIndex;
		}

		public Action GetNextAction(State state)
		{
			return actions[GetNextActionIndex(state)];
		}

		public double GetQValue(State state, int actionIndex)
		{
			List<int> inputs = state.GetInputs();
			inputs.Add(actionIndex);
			return GetQValue(inputs);
		}

		private double GetQValue(List<int> inputs)
		{ 
			return qValues.GetAt(inputs);
		}
	}

	public class Action
	{
		private Vector2 playerPosDelta;

		public Action(Vector2 playerPosDelta)
		{
			this.playerPosDelta = playerPosDelta;
			this.playerPosDelta.x = Mathf.Round(this.playerPosDelta.x);
			this.playerPosDelta.y = Mathf.Round(this.playerPosDelta.y);
		}

		public Vector2 GetPlayerPosDelta()
		{
			return playerPosDelta;
		}
	}

	public class State
	{
		private Vector2 playerPos;
		private int moves;

		public State(Vector2 playerPos, int moves)
		{
			this.playerPos = playerPos;
			this.playerPos.x = Mathf.Round(this.playerPos.x);
			this.playerPos.y = Mathf.Round(this.playerPos.y);
			this.moves = moves;
		}

		public Vector2 GetPlayerPos()
		{
			return playerPos;
		}

		public int GetMoves()
		{
			return moves;
		}

		public List<int> GetInputs()
		{
			List<int> inputs = new List<int>();
			inputs.Add(Mathf.RoundToInt(playerPos.x));
			inputs.Add(Mathf.RoundToInt(playerPos.y));
			inputs.Add(0);
			return inputs;
		}
	}

	public class StateReward
	{
		private State state;
		private int reward;

		public StateReward(State state, int reward)
		{
			this.state = state;
			this.reward = reward;
		}

		public State GetState()
		{
			return state;
		}

		public int GetReward()
		{
			return reward;
		}
	}

	public class World
	{
		private int mapSize;
		private int winReward;
		private int stepReward;
		private int loseReward;
		private int maxMoves;
		private int goalX;
		private int goalY;

		public World(int mapSize, int maxMoves, int winReward, int stepReward, int loseReward)
		{
			this.mapSize = mapSize;
			this.maxMoves = maxMoves;
			this.winReward = winReward;
			this.stepReward = stepReward;
			this.loseReward = loseReward;
			goalX = mapSize / 2;
			goalY = mapSize / 2;
		}

		public StateReward GetNextStateReward(State state, Action action)
		{
			State nextState = GetNextState(state, action);
			int reward = GetReward(nextState);
			return new StateReward(nextState, reward);
		}

		public State GetNextState(State state, Action action)
		{
			Vector2 nextPlayerPos = state.GetPlayerPos() + action.GetPlayerPosDelta();
			return new State(nextPlayerPos, state.GetMoves() + 1);
		}

		private int GetReward(State nextState)
		{
			if (HasWon(nextState))
			{
				return winReward;
			}
			else if (HasLost(nextState))
			{
				return loseReward;
			}
			else
			{
				return stepReward; // * nextState.GetMoves()
			}
		}

		private bool HasWon(State state)
		{
			return state.GetPlayerPos().x == goalX && state.GetPlayerPos().y == goalY;
		}

		private bool HasLost(State state)
		{
			return state.GetPlayerPos().x < 0 || state.GetPlayerPos().x > mapSize || state.GetPlayerPos().y < 0 || state.GetPlayerPos().y > mapSize || state.GetMoves() >= maxMoves;
		}

		public bool IsTerminal(State state)
		{
			return HasWon(state) || HasLost(state);
		}
	}
}

