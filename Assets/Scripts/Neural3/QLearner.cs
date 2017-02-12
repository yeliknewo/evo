using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Neural3;

namespace QLearningNotSure
{
	public class QLearner
	{
		private NeuralNetwork network;
		private List<Action> actions;
		private double alpha;
		private double gamma;
		private double eta; //learning rate
		private int eValue; //e greedy
		private int maxReward;

		private double maxQValueTemp;

		public QLearner(int maxReward, int eValue, double alpha, double gamma, double eta, List<Action> actions, List<int> hiddenSizes, int numInputs, int seed)
		{
			this.maxReward = maxReward;
			this.eValue = eValue;
			this.alpha = alpha;
			this.gamma = gamma;
			this.eta = eta;
			this.actions = actions;
			hiddenSizes.Add(1);
			network = new NeuralNetwork(hiddenSizes, numInputs + actions.Count, seed);
		}

		public Action GetNextAction(State state)
		{
			return actions[GetNextActionIndex(state)];
		}

		private int GetNextActionIndex(State state)
		{
			double maxQ = double.MinValue;
			int maxIndex = 0;
			for (int i = 0; i < actions.Count; i++)
			{
				double q = GetQValue(state, i);
				if (q > maxQ)
				{
					maxQ = q;
					maxIndex = i;
				}
			}
			maxQValueTemp = maxQ;
			return maxIndex;
		}

		public void RunEpoch(World world, State state)
		{
			int count = 0;
			while (!world.IsTerminal(state) && count < 1000)
			{
				count++;
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
				double qValue = GetQValue(state, actionIndex);
				double newQValue = GetNewQValue(maxQValueTemp, qValue, alpha, stateReward.GetReward(), gamma);
				UpdateNetwork(state, actionIndex, newQValue, eta);
				state = stateReward.GetState();
			}
		}

		private void UpdateNetwork(State state, int actionIndex, double newQValue, double eta)
		{
			List<List<double>> inputs = new List<List<double>>();
			inputs.Add(GetInput(state, actionIndex));
			List<List<double>> targetOutputs = new List<List<double>>();
			List<double> newQValues = new List<double>();
			newQValues.Add(newQValue);
			targetOutputs.Add(newQValues);
			network.UpdateBatch(inputs, targetOutputs, eta);
		}

		private double GetNewQValue(double maxQ, double q, double alpha, int reward, double gamma)
		{
			return q + alpha * (((double)reward / (double)(maxReward)) + gamma * maxQ - q);
		}

		private double GetMaxValue(List<double> values)
		{
			double max = double.MinValue;
			foreach (double val in values)
			{
				if (val > max)
				{
					max = val;
				}
			}
			return max;
		}

		private List<double> GetInput(State state, int actionIndex)
		{
			List<double> input = state.GetInputs();
			for (int i = 0; i < actions.Count; i++)
			{
				if (i == actionIndex)
				{
					input.Add(1.0);
				}
				else
				{
					input.Add(0.0);
				}
			}
			return input;
		}

		private double GetQValue(State state, int actionIndex)
		{

			return network.FeedForward(GetInput(state, actionIndex))[0];
		}
	}

	public class Action
	{
		private Vector2 playerPosDelta;
		private Vector2 goalPosDelta;

		public Action(Vector2 playerPosDelta, Vector2 goalPosDelta)
		{
			this.playerPosDelta = playerPosDelta;
			this.goalPosDelta = goalPosDelta;
		}

		public Vector2 GetPlayerPosDelta()
		{
			return playerPosDelta;
		}

		public Vector2 GetGoalPosDelta()
		{
			return goalPosDelta;
		}
	}

	public class State
	{
		private Vector2 playerPos;
		private Vector2 goalPos;
		private float mapSize;

		public State(Vector2 playerPos, Vector2 goalPos, float mapSize)
		{
			this.playerPos = playerPos;
			this.goalPos = goalPos;
			this.mapSize = mapSize;
		}

		public Vector2 GetPlayerPos()
		{
			return playerPos;
		}

		public Vector2 GetGoalPos()
		{
			return goalPos;
		}

		public List<double> GetInputs()
		{
			List<double> inputs = new List<double>();
			inputs.Add(playerPos.x / mapSize);
			inputs.Add(playerPos.y / mapSize);
			inputs.Add(goalPos.x / mapSize);
			inputs.Add(goalPos.y / mapSize);
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
		private float mapSize;
		private float winDistance;
		private int winReward;
		private int stepReward;
		private int loseReward;

		public World(float mapSize, float winDistance, int winReward, int stepReward, int loseReward)
		{
			this.mapSize = mapSize;
			this.winDistance = winDistance;
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
			Vector2 nextGoalPos = state.GetGoalPos() + action.GetGoalPosDelta();
			return new State(nextPlayerPos, nextGoalPos, mapSize);
		}

		private int GetReward(State state)
		{
			if (HasWon(state))
			{
				return winReward;
			}
			else if (HasLost(state))
			{
				return loseReward;
			}
			else
			{
				return stepReward;
			}
		}

		private bool HasWon(State state)
		{
			return Vector2.Distance(state.GetPlayerPos(), state.GetGoalPos()) < winDistance;
		}

		private bool HasLost(State state)
		{
			return state.GetPlayerPos().magnitude > mapSize;
		}

		public bool IsTerminal(State state)
		{
			return HasWon(state) || HasLost(state);
		}
	}
}
