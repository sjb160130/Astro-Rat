using System;
using UnityEngine;

public abstract class StateMachine<T> : MonoBehaviour where T : IConvertible, IComparable
{
	protected T _currentState { get; private set; }

	protected void SetState(T newState)
	{
		if (newState.CompareTo(_currentState) == 0)
			return;
		ExitState(_currentState);
		_currentState = newState;
		EnterState(_currentState);
	}

	abstract protected void ExitState(T previousState);

	abstract protected void EnterState(T nextState);
}
