using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//http://www.cse.unsw.edu.au/~cs9417ml/RL1/source/CatAndMouseWorld.java

namespace CatAndMouse
{
	public class QLearner
	{
		private RLWorld world;
		private RLPolicy policy;

		private double alpha;   //1.0
		private double gamma; //0.1
							  //private double lambda; //0.1
		private double epsilon; //0.1

		List<int> dimSize, state, newState;
		int action;
		double reward;

		public QLearner(RLWorld world)
		{
			this.world = world;

			dimSize = world.GetDimension();

			policy = new RLPolicy(dimSize);

			policy.InitValues(world.GetInitValues());

			epsilon = 0.1;

			alpha = 1;
			gamma = 0.1;
			//lambda = 0.1;
		}

		public void RunEpoch()
		{
			state = world.ResetState();

			//start q learning
			double thisQ;
			double maxQ;
			double newQ;
			while (!world.IsOver())
			{
				action = SelectAction(state);
				newState = world.GetNextState(action);
				reward = world.GetReward(action);

				thisQ = policy.GetQValue(state, action);
				maxQ = policy.GetMaxQValue(newState);

				//calculate new value for Q
				newQ = thisQ + alpha * (reward + gamma * maxQ - thisQ);
				policy.SetQValue(state, action, newQ);

				state = newState;
			}
		}

		private int SelectAction(List<int> state)
		{
			List<double> qValues = policy.GetQValuesAt(state);
			int selectedAction = -1;

			//E greedy

			double maxQ = double.MinValue;

			List<int> doubleValues = new List<int>();
			int maxDV = 0;

			//Explore
			if (Random.value < epsilon)
			{
				selectedAction = -1;
			}
			else
			{
				for (action = 0; action < qValues.Count; action++)
				{
					if (qValues[action] > maxQ)
					{
						selectedAction = action;
						maxQ = qValues[action];
						maxDV = 0;
						while (doubleValues.Count <= maxDV)
						{
							doubleValues.Add(0);
						}
						doubleValues[maxDV] = selectedAction;
					}
					else if (qValues[action] == maxQ)
					{
						maxDV++;
						while (doubleValues.Count <= maxDV)
						{
							doubleValues.Add(0);
						}
						doubleValues[maxDV] = action;
					}
				}
				if (maxDV > 0)
				{
					int randomIndex = Random.Range(0, maxDV);
					selectedAction = doubleValues[randomIndex];
				}
			}
			if (selectedAction == -1)
			{
				selectedAction = Random.Range(0, qValues.Count);
			}
			while (!world.IsValidAction(selectedAction))
			{
				selectedAction = Random.Range(0, qValues.Count);
			}
			return selectedAction;
		}

		private double GetMaxQValue(List<int> state, int action)
		{
			double maxQ = 0;
			List<double> qValues = policy.GetQValuesAt(state);

			for (action = 0; action < qValues.Count; action++)
			{
				if (qValues[action] > maxQ)
				{
					maxQ = qValues[action];
				}
			}
			return maxQ;
		}
	}

}
