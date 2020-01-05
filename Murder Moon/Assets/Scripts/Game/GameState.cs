using DG.Tweening;
using System.Collections;
using UnityEngine;

public class GameState : StateMachine<GameState.State>
{
	public enum State
	{
		None,
		GameStart,
		InGame,
		VictoryScreen
	}

	//singleton
	public static GameState Instance { get; private set; }

	const float StartDelayDuration = 3f;
	static float StartDelay = StartDelayDuration;

	public GameView StartView, GameplayView, WinnerView;

	public static RatPlayer Winner { get; private set; }

	[SerializeField]
	float _gameDuration = 60f;
	public static float GameTimer { get; private set; }

	public bool IsPlaying { get { return _currentState == State.InGame; } }

	private void Awake()
	{
		//singleton
		Instance = this;

		//initialize DOTween
		DOTween.Init();
	}

	private void Start()
	{
		SetState(GameState.State.GameStart);
	}

	private void Update()
	{
		//update logic
		switch (this._currentState)
		{
			case State.GameStart:
				//poll player manager to see if we're readied up
				if (PlayerManager.Instance.IsAllReady())
				{
					//timer for a short delay before setting state
					StartDelay -= Time.deltaTime;
					Debug.Log(StartDelay.ToString("0.0"));
					if (StartDelay <= 0f)
						this.SetState(State.InGame);
				}
				else
				{
					//if not readied, reset timer
					StartDelay = StartDelayDuration;
				}
				break;
			case State.InGame:
				GameTimer -= Time.deltaTime;
				if (GameTimer <= 0f)
				{
					var winner = RatPlayer.FindCurrentWinner();
					Winner = winner;
					if (winner != null)
						SetState(State.VictoryScreen);
				}
				break;
			case State.VictoryScreen:
				break;
			default:
				break;
		}
	}

	IEnumerator VictoryScreenDelay()
	{
		yield return new WaitForSeconds(7f);
		SetState(State.GameStart);
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
		Debug.Log(nextState);

		StartView?.Toggle(nextState == State.GameStart);
		GameplayView?.Toggle(nextState == State.InGame);
		WinnerView?.Toggle(nextState == State.VictoryScreen);

		switch (nextState)
		{
			case State.GameStart:
				PlayerManager.Instance.Reset();
				PlayerManager.Instance.Locked = false;
				RatPlayer.ResetAllPlayers();
				break;
			case State.InGame:
				GameTimer = _gameDuration;
				break;
			case State.VictoryScreen:
				StartCoroutine(VictoryScreenDelay());
				break;
			default:
				break;
		}
	}
}
