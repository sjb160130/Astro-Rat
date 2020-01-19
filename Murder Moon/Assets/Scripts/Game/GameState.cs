using DG.Tweening;
using PKG;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

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

	const float StartDelayDuration = 3.1f;
	static float StartDelay = StartDelayDuration;

	public GameView StartView, GameplayView, WinnerView;

	public static RatPlayer Winner { get; private set; }

	[SerializeField]
	float _gameDuration = 60f;
	public static float GameTimer { get; private set; }

	public bool IsPlaying { get { return _currentState == State.InGame; } }
	public bool IsAtStartScreen { get { return _currentState == State.GameStart; } }

	bool _timerRunning = false;

	[Header("Sequence Directors")]
	public PlayableDirector GameStartDirector;
	public AudioClip[] GoSFX;
	public PlayableDirector SuddenDeathDirector;
	public AudioClip[] SuddenDeathSFX;
	public PlayableDirector WinnerDirector;
	public AudioClip[] WinnerSFX;
	public AudioClip[] WelcomeSFX;
	public PlayableDirector Countdown;

	const float ShipDelay = 2f;

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
					this.Countdown.gameObject?.SetActive(true);
				}
				else
				{
					//if not readied, reset timer
					StartDelay = StartDelayDuration;
					this.Countdown.gameObject?.SetActive(false);
				}
				break;
			case State.InGame:
				if (_timerRunning)
					GameTimer -= Time.deltaTime;
				if (GameTimer <= 0f)
				{
					var winner = RatPlayer.FindCurrentWinner();
					Winner = winner;
					if (winner != null)
					{
						SetState(State.VictoryScreen);
					}
					else
					{
						if (SuddenDeathDirector.gameObject.activeSelf == false)
							AudioManager.Instance.PlaySound(this.SuddenDeathSFX.GetRandom(), this.transform.position, AudioManager.MixerGroup.Announcer, 1f);
						SuddenDeathDirector.gameObject.SetActive(true);
					}
				}
				break;
			case State.VictoryScreen:
				break;
			default:
				break;
		}
	}

	IEnumerator StartDirectorRoutine()
	{
		AudioManager.Instance.PlaySong(AudioManager.Song.Nothing);
		yield return new WaitForSeconds(RatShipRespawner.AnimationLength + ShipDelay);
		this.GameStartDirector.gameObject.SetActive(true);
		AudioManager.Instance.PlaySound(this.GoSFX.GetRandom(), this.transform.position, AudioManager.MixerGroup.Announcer, 1f);
		AudioManager.Instance.PlaySong(AudioManager.Song.Battle);
		StartTimerAndStarSpawning();
		yield return new WaitForSeconds(2f);
		this.GameStartDirector.gameObject.SetActive(false);
	}

	void StartTimerAndStarSpawning()
	{
		_timerRunning = true;
		SpawnManager.Instance.SpawningActive = true;
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
				StartDelay = StartDelayDuration;
				this.Countdown.gameObject?.SetActive(false);
				break;
			case State.InGame:
				foreach (var player in FindObjectsOfType<RatPlayer>())
				{
					player.EndGame();
				}
				SuddenDeathDirector.gameObject.SetActive(false);
				SpawnManager.Instance.SpawningActive = false;
				break;
			case State.VictoryScreen:
				WinnerDirector.gameObject.SetActive(false);
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
				AudioManager.Instance.Mixer.FindSnapshot("Title").TransitionTo(0.2f);
				PlayerManager.Instance.Reset();
				PlayerManager.Instance.Locked = false;
				RatPlayer.ResetAllPlayers();
				AudioManager.Instance.PlaySong(AudioManager.Song.Main);
				break;
			case State.InGame:
				AudioManager.Instance.Mixer.FindSnapshot("Gameplay").TransitionTo(1f);
				GameTimer = _gameDuration;
				_timerRunning = false;
				RatPlayer.SetupPlayersForPlay(ShipDelay);
				foreach (var go in GameObject.FindGameObjectsWithTag("Item"))
				{
					PoolManager.ReleaseObject(go);
				}
				StartCoroutine(StartDirectorRoutine());
				SpawnManager.Instance.SpawningActive = false;
				AudioManager.Instance.PlaySound(WelcomeSFX.GetRandom(), this.transform.position, AudioManager.MixerGroup.Announcer, 1f);
				break;
			case State.VictoryScreen:
				AudioManager.Instance.Mixer.FindSnapshot("Title").TransitionTo(0.2f);
				StartCoroutine(VictoryScreenDelay());
				WinnerDirector.gameObject.SetActive(true);
				AudioManager.Instance.PlaySong(AudioManager.Song.Nothing);
				AudioManager.Instance.PlaySound(WinnerSFX.GetRandom(), this.transform.position, AudioManager.MixerGroup.Announcer, 1f);
				break;
			default:
				break;
		}
	}
}
