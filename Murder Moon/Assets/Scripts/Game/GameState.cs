using System.Collections;
using UnityEngine;

public class GameState : StateMachine<GameState.State>
{
	public enum State
	{
		GameStart,
		InGame,
		VictoryScreen
	}

	//singleton
	public static GameState Instance { get; private set; }

	const float StartDelayDuration = 5f;
	static float StartDelay = StartDelayDuration;

	private void Awake()
	{
		//singleton
		Instance = this;
	}

	private void Update()
	{
		//update logic
		switch (this._currentState)
		{
			case State.GameStart:
				if (PlayerManager.Instance.IsAllReady())
				{
					StartDelay -= Time.deltaTime;
					if (StartDelay <= 0f)
						this.SetState(State.InGame);
				}
				else
				{
					StartDelay = StartDelayDuration;
				}
				break;
			case State.InGame:
				break;
			case State.VictoryScreen:
				break;
			default:
				break;
		}
	}

	protected override void ExitState(State previousState)
	{
		switch (previousState)
		{
			case State.GameStart:
				PlayerManager.Instance.Locked = true;
				break;
			case State.InGame:
				foreach (var player in FindObjectsOfType<RatPlayer>())
				{
					player.EndGame();
				}
				break;
			case State.VictoryScreen:
				break;
			default:
				break;
		}
	}

	protected override void EnterState(State nextState)
	{
		switch (nextState)
		{
			case State.GameStart:
				PlayerManager.Instance.Locked = false;
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
