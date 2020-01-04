using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
	public enum State
	{
		GameStart,
		InGame,
		VictoryScreen
	}

	State _currentState;

	void SetState(State newState)
	{
		if (newState == _currentState)
			return;
		ExitState(_currentState);
		_currentState = newState;
		EnterState(_currentState);
	}

	private void ExitState(State currentState)
	{
		switch (currentState)
		{
			case State.GameStart:
				break;
			case State.InGame:
				break;
			case State.VictoryScreen:
				break;
			default:
				break;
		}
	}

	private void EnterState(State currentState)
	{
		switch (currentState)
		{
			case State.GameStart:
				break;
			case State.InGame:
				break;
			case State.VictoryScreen:
				break;
			default:
				break;
		}
	}

	private void Update()
	{
		switch (this._currentState)
		{
			case State.GameStart:
				break;
			case State.InGame:
				break;
			case State.VictoryScreen:
				break;
			default:
				break;
		}
	}
}
