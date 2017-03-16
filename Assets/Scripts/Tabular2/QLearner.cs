using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Tablular2
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

		public void RunEpoch(World world, State state, int eValue, double alpha, double gamma)
		{
			while(!world.IsTerminal(state))
			{
				state = RunStep(world, state, eValue, alpha, gamma);
			}
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
			State nextState;
			int reward;
			world.GetNextStateReward(state, actions[actionIndex], out nextState, out reward);
			double qValue = GetQValue(state, actionIndex);
			double maxQValue = GetMaxQValue(nextState);
			double newQValue = GetNewQValue(maxQValue, qValue, alpha, gamma, reward);
			UpdateQValues(state, actionIndex, newQValue);
			return nextState;
		}

		public void UpdateQValues(State state, int actionIndex, double newQ)
		{
			List<int> inputs = state.GetInputs();
			inputs.Add(actionIndex);
			qValues.SetAt(inputs, newQ);
		}

		public double GetNewQValue(double maxQValue, double qValue, double alpha, double gamma, int reward)
		{
			return qValue + alpha * (reward + gamma * maxQValue - qValue);
		}

		public double GetMaxQValue(State state)
		{
			List<int> inputs = state.GetInputs();
			double maxQ = double.MinValue;
			for(int actionIndex = 0; actionIndex < actions.Count; actionIndex++)
			{
				inputs.Add(actionIndex);
				double q = GetQValue(inputs);
				if(maxQ < q)
				{
					maxQ = q;
				}
				inputs.RemoveAt(inputs.Count - 1);
			}
			return maxQ;
		}

		public int GetNextActionIndex(State state, int eValue)
		{
			if (Random.Range(0, eValue) == 0)
			{
				return Random.Range(0, actions.Count);
			}
			double maxQ = double.MinValue;
			int maxIndex = 0;
			for (int actionIndex = 0; actionIndex < actions.Count; actionIndex++)
			{
				double q = GetQValue(state, actionIndex);
				if (maxQ < q)
				{
					maxQ = q;
					maxIndex = actionIndex;
				}
			}
			return maxIndex;
		}

		public double GetQValue(State state, int actionIndex)
		{
			List<int> inputs = state.GetInputs();
			inputs.Add(actionIndex);
			return GetQValue(inputs);
		}

		public double GetQValue(List<int> inputs)
		{
			return qValues.GetAt(inputs);
		}
	}

	public interface Action
	{

	}

	public interface State
	{
		List<int> GetInputs();
	}

	public interface World
	{
		void GetNextStateReward(State state, Action action, out State nextState, out int reward);
		bool IsTerminal(State state);
	}
}

