using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tabular1
{
	public class QLearner
	{
		private EndlessArray<double> qValues;
		private List<Action> actions;

		public QLearner(List<Action> actions, List<int> tableDimensions, double startingQ)
		{
			this.actions = actions;
			qValues = new EndlessArray<double>(tableDimensions, startingQ, -startingQ);
		}

		public State RunStep(World world, State state, int eValue, double alpha, double gamma)
		{
			int actionIndex;
			if (Random.Range(0, eValue) == 0)
			{
				actionIndex = Random.Range(0, actions.Count);
			}
			else
			{
				actionIndex = GetNextActionIndex(state, eValue);
			}
			StateReward stateReward = world.GetNextStateReward(state, actions[actionIndex]);
			State nextState = stateReward.GetState();
			int reward = stateReward.GetReward();
			double qValue = GetQValue(state, actionIndex);
			double maxQValue = GetMaxQValue(nextState);
			double newQValue = GetNewQValue(maxQValue, qValue, alpha, gamma, reward);
			UpdateQValues(state, actionIndex, newQValue);
			return nextState;
		}

		public void RunEpoch(World world, State state, int eValue, double alpha, double gamma)
		{
			while (!world.IsTerminal(state))
			{
				state = RunStep(world, state, eValue, alpha, gamma);
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

		public int GetNextActionIndex(State state, int eValue)
		{
			if (Random.Range(0, eValue) == 0)
			{
				return Random.Range(0, actions.Count);
			}
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

		public Action GetNextAction(int actionIndex)
		{
			return actions[actionIndex];
		}

		public Action GetNextAction(State state, int eValue)
		{
			return actions[GetNextActionIndex(state, eValue)];
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
		private Vector2 goalPos;

		public State(Vector2 playerPos, Vector2 goalPos)
		{
			this.playerPos = playerPos;
			this.playerPos.x = Mathf.Round(this.playerPos.x);
			this.playerPos.y = Mathf.Round(this.playerPos.y);
			this.goalPos = goalPos;
			this.goalPos.x = Mathf.Round(this.goalPos.x);
			this.goalPos.y = Mathf.Round(this.goalPos.y);
		}

		public Vector2 GetPlayerPos()
		{
			return playerPos;
		}

		public Vector2 GetGoalPos()
		{
			return goalPos;
		}

		public List<int> GetInputs()
		{
			List<int> inputs = new List<int>();
			inputs.Add((int)playerPos.x);
			inputs.Add((int)playerPos.y);
			//inputs.Add((int)goalPos.x);
			//inputs.Add((int)goalPos.y);
			return inputs;
		}

		public State MovePlayerTo(Vector2 playerPos)
		{
			return new State(playerPos, this.goalPos);
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

		public World(int mapSize)
		{
			this.mapSize = mapSize;
		}

		public void UpdateWorld(int winReward, int stepReward, int loseReward)
		{
			this.winReward = winReward;
			this.stepReward = stepReward;
			this.loseReward = loseReward;
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
			return new State(nextPlayerPos, state.GetGoalPos());
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
				return stepReward * GetMod(nextState);
			}
		}

		private int GetMod(State state)
		{
			int x = (int)state.GetPlayerPos().x + 1;
			int y = (int)state.GetPlayerPos().y + 1;
			return x % y + y % x + 1;
		}

		private bool HasWon(State state)
		{
			return state.GetPlayerPos().x == state.GetGoalPos().x && state.GetPlayerPos().y == state.GetGoalPos().y;
		}

		private bool HasLost(State state)
		{
			return state.GetPlayerPos().x < 0 || state.GetPlayerPos().x >= mapSize || state.GetPlayerPos().y < 0 || state.GetPlayerPos().y >= mapSize;
		}

		public bool IsTerminal(State state)
		{
			return HasWon(state) || HasLost(state);
		}
	}
}

