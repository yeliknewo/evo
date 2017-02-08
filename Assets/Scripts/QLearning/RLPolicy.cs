using System.Collections.Generic;
using UnityEngine;

public class RLPolicy
{
	private List<int> dimSize;
	private List<double> qValues;
	private EndlessArray<List<double>> qValuesTable;
	private int states, actions;

	public RLPolicy(List<int> dimSize)
	{
		this.dimSize = dimSize;

		qValuesTable = new EndlessArray<List<double>>(dimSize);

		states = dimSize[0];
		for (int j = 1; j < dimSize.Count - 1; j++)
		{
			states *= dimSize[j];
		}

		actions = dimSize[dimSize.Count - 1];
	}

	public void InitValues(double initValue)
	{
		//List<int> state = new List<int>();
		//for (int i = 0; i < dimSize.Count - 1; i++)
		//{
		//	state.Add(0);
		//}
		//for (int j = 0; j < states; j++)
		//{
		//	qValues = new List<double>();
		//	for (int i = 0; i < actions; i++)
		//	{
		//		qValues.Add(initValue);
		//	}
		//	qValuesTable.SetAt(state, qValues);

		//	state = GetNextState(state);
		//}
	}

	private List<int> GetNextState(List<int> state)
	{
		int i;
		int actualDim = 0;

		state[actualDim]++;
		if (state[actualDim] >= dimSize[actualDim])
		{
			while ((actualDim < dimSize.Count - 1) && (state[actualDim] >= dimSize[actualDim]))
			{
				actualDim++;
				if (actualDim == dimSize.Count - 1)
				{
					return state;
				}
				state[actualDim]++;
			}
			for (i = 0; i < actualDim; i++)
			{
				state[i] = 0;
			}
			actualDim = 0;
		}
		return state;
	}

	private List<double> MyQValues(List<int> state)
	{
		List<double> vals = qValuesTable.GetAt(state);
		if(vals == null)
		{
			vals = new List<double>();
			for(int i = 0; i< actions; i++)
			{
				vals.Add(0);
			}
			qValuesTable.SetAt(state, vals);
		}
		return vals;
	}

	public List<double> GetQValuesAt(List<int> state)
	{
		List<double> returnValues;

		qValues = MyQValues(state);
		returnValues = new List<double>();
		returnValues.AddRange(qValues);
		return returnValues;
	}

	public void SetQValue(List<int> state, int action, double newQValue)
	{
		qValues = MyQValues(state);
		qValues[action] = newQValue;
	}

	public double GetMaxQValue(List<int> state)
	{
		double maxQ = double.MinValue;
		qValues = MyQValues(state);
		for (int action = 0; action < qValues.Count; action++)
		{
			if (qValues[action] > maxQ)
			{
				maxQ = qValues[action];
			}
		}
		return maxQ;
	}

	public double GetQValue(List<int> state, int action)
	{
		double qValue = 0;
		qValues = MyQValues(state);
		qValue = qValues[action];

		return qValue;
	}

	public int GetBestAction(List<int> state)
	{
		double maxQ = double.MinValue;
		int selectedAction = -1;
		List<int> doubleValues = new List<int>(qValues.Count);
		int maxDV = 0;

		qValues = MyQValues(state);

		for (int action = 0; action < qValues.Count; action++)
		{
			if (qValues[action] > maxQ)
			{
				selectedAction = action;
				maxQ = qValues[action];
				maxDV = 0;
				doubleValues[maxDV] = selectedAction;
			}
			else if (qValues[action] == maxQ)
			{
				maxDV++;
				doubleValues[maxDV] = action;
			}
		}

		if (maxDV > 0)
		{
			int randomIndex = Random.Range(0, maxDV);
			selectedAction = doubleValues[randomIndex];
		}

		if (selectedAction == -1)
		{
			selectedAction = Random.Range(0, qValues.Count - 1);
		}

		return selectedAction;
	}

	private double GetBestAction(List<int> state, int bestAction)
	{
		double maxQ = 0;
		bestAction = -1;
		qValues = GetQValuesAt(state);
		for (int action = 0; action < qValues.Count; action++)
		{
			if (qValues[action] > maxQ)
			{
				bestAction = action;
				maxQ = qValues[action];
			}
		}

		return maxQ;
	}
}
