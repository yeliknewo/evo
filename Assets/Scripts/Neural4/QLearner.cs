using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackProp;

namespace QLearningNeural4
{
	public class QLearner
	{
		private BackPropNeuralNet network;
		private List<Action> actions;

		public QLearner(List<Action> actions, int hiddenLayer, int numInputs, int seed)
		{
			this.actions = actions;
			network = new BackPropNeuralNet(numInputs, hiddenLayer, actions.Count);
		}

		public Action GetNextAction(State state)
		{
			return actions[GetNextActionIndex(state, GetQValues(state))];
		}

		private int GetNextActionIndex(State state, List<double> qValues)
		{
			double maxQ = double.MinValue;
			int maxIndex = 0;
			for (int i = 0; i < qValues.Count; i++)
			{
				double q = qValues[i];
				if (maxQ < q)
				{
					maxQ = q;
					maxIndex = i;
				}
			}
			return maxIndex;
		}

		public State RunStep(World world, State state, int eValue, double alpha, double gamma, double eta, double mom)
		{
			List<double> qValues = GetQValues(state);
			int actionIndex;
			if (Random.Range(0, eValue) == 0)
			{
				actionIndex = Random.Range(0, actions.Count);
			}
			else
			{
				actionIndex = GetNextActionIndex(state, qValues);
			}
			StateReward stateReward = world.GetNextStateReward(state, actions[actionIndex]);
			State nextState = stateReward.GetState();
			int reward = stateReward.GetReward();
			double qValue = qValues[actionIndex];
			double maxQValue = GetMaxQValue(nextState, qValues);
			double newQValue = GetNewQValue(maxQValue, qValue, alpha, reward, gamma);
			UpdateNetwork(state, qValues, actionIndex, newQValue, eta, mom);
			return nextState;
		}

		public void RunEpoch(World world, State state, int eValue, double alpha, double gamma, double eta, double mom)
		{
			while (!world.IsTerminal(state))
			{
				state = RunStep(world, state, eValue, alpha, gamma, eta, mom);
			}
		}

		private void UpdateNetwork(State state, List<double> qValues, int actionIndex, double newQValue, double eta, double mom)
		{
			//List<List<double>> targetOutputs = new List<List<double>>();
			//List<List<double>> inputs = new List<List<double>>();
			//for (int i = 0; i < actions.Count; i++)
			//{
			//	if (i == actionIndex)
			//	{
			//		inputs.Add(GetInput(state, actionIndex));
			//		List<double> newQValues = new List<double>();
			//		newQValues.Add(newQValue);
			//		targetOutputs.Add(newQValues);
			//	}
			//	else
			//	{
			//		inputs.Add(GetInput(state, i));
			//		List<double> newQValues = new List<double>();
			//		newQValues.Add(GetQValue(state, i));
			//		targetOutputs.Add(newQValues);
			//	}
			//}
			List<double> targetOutputs = new List<double>();
			targetOutputs.AddRange(qValues);
			targetOutputs[actionIndex] = newQValue;
			network.UpdateWeights(targetOutputs.ToArray(), eta, mom);

			//network.UpdateBatch(inputs, targetOutputs, eta);
		}

		private double GetNewQValue(double maxQ, double q, double alpha, int reward, double gamma)
		{
			return q + alpha * (reward + gamma * maxQ - q);
		}

		private double GetMaxQValue(State state, List<double> qValues)
		{
			double max = double.MinValue;
			for (int i = 0; i < qValues.Count; i++)
			{
				double qVal = qValues[i];
				if (max < qVal)
				{
					max = qVal;
				}
			}
			return max;
		}

		private List<double> GetInput(State state)
		{
			return state.GetInputs();
		}

		private List<double> GetQValues(State state)
		{
			List<double> output = new List<double>();
			output.AddRange(network.ComputeOutputs(GetInput(state).ToArray()));
			return output;
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
			int reward = GetReward(nextState, state);
			return new StateReward(nextState, reward);
		}

		public State GetNextState(State state, Action action)
		{
			Vector2 nextPlayerPos = state.GetPlayerPos() + action.GetPlayerPosDelta();
			Vector2 nextGoalPos = state.GetGoalPos() + action.GetGoalPosDelta();
			return new State(nextPlayerPos, nextGoalPos, mapSize);
		}

		private int GetReward(State state, State lastState)
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
				//return Mathf.RoundToInt((-Vector2.Distance(state.GetPlayerPos(), state.GetGoalPos()) + Vector2.Distance(lastState.GetPlayerPos(), lastState.GetGoalPos())) * 100);
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
