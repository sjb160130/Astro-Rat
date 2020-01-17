using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Yeeter : StateMachine<Yeeter.State>
{
	public enum State
	{
		Empty,
		Charging,
		Holding,
		Yeeting
	}

	public enum Direction
	{
		None, Left, Right
	}

	private int _playerID;

	Rewired.Player _player { get { return Rewired.ReInput.players.GetPlayer(_playerID); } }

	public float WindupDuration = 1.2f;
	float _windup01;

	public float YeetDuration = 0.1f;
	float _yeet01;

	public float GrabRadius = 1f;

	Grabbable _heldItem;

	public LayerMask GrabbableItemMask;

	Collider2D[] _grabResults = new Collider2D[10];

	public float YeetStrength = 30f;

	public LineRenderer LineRenderer;
	public float LineRendererLength = 3f;

	public Transform ItemMountPoint;

	public State CurrentState { get { return this._currentState; } }
	public Direction CurrentDirection { get; private set; }

	RatBrain _brain;
	RatPlayer _ratPlayer;

	private void Awake()
	{
		_brain = GetComponent<RatBrain>();
		_playerID = GetComponent<RatPlayer>().PlayerID;
		_ratPlayer = GetComponent<RatPlayer>();
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(this.transform.position, GrabRadius);
	}

	private void Update()
	{
		var p = _player;

		UpdateLineRenderer();

		if (_ratPlayer.Dead) {
			if (this._heldItem != null)
				Drop();
			return;
		}

		switch (this._currentState)
		{
			case State.Empty:
				if (p.GetButtonDown("Interact"))
				{
					TryGrab();
				}
				break;
			case State.Charging:
				if (p.GetButton("Interact") == false)
				{
					Yeet();
					SetState(State.Yeeting);
				}
				else
				{
					_windup01 += Time.deltaTime / WindupDuration;
					_windup01 = Mathf.Clamp01(_windup01);
				}
				break;
			case State.Holding:
				if (p.GetButtonDown("Interact"))
				{
					SetState(State.Charging);
				}
				break;
			case State.Yeeting:
				_yeet01 += Time.deltaTime / YeetDuration;
				_yeet01 = Mathf.Clamp01(_yeet01);
				if (_yeet01 >= 1f || Mathf.Approximately(1f, _yeet01))
					SetState(State.Empty);
				break;
			default:
				break;
		}
	}

	void TryGrab()
	{
		int resultsCount
			= Physics2D.OverlapCircleNonAlloc(this.transform.position, this.GrabRadius, _grabResults, GrabbableItemMask);
		for (int i = 0; i < resultsCount; i++)
		{
			Grabbable p = _grabResults[i].GetComponent<Grabbable>();
			if (p == null || p.gameObject == this.gameObject)
				continue;
			if (p.IsHeld == false)
			{
				RatPlayer player = p.GetComponent<RatPlayer>();
				if (player != null && player.Dead == true)
					continue;
				p.Grab(this);
				this._heldItem = p;
				SetState(State.Holding);
				//Debug.Log("Grab");
				break;
			}
		}
	}

	public void Drop()
	{
		if (_heldItem == null)
			return;

		this._heldItem.Release();
		//Debug.Log(_windup01);
		this._heldItem.MyRigidbody.transform.position = this.ItemMountPoint.transform.position;
		this._heldItem.MyRigidbody.velocity = Vector2.zero;
		this._heldItem.MyRigidbody.AddTorque(Random.Range(-5f, 5f), ForceMode2D.Impulse);
		this._heldItem = null;
		this.SetState(State.Empty);
	}

	void Yeet()
	{
		//Debug.Log("yeet");
		if (this._heldItem != null)
		{
			this._heldItem.Release();
			Vector3 direction = GetYeetDirection();
			direction.Normalize();
			//Debug.Log(_windup01);
			Vector3 throwVel = direction * YeetStrength;
			if (this._heldItem.IsPlayer)
				throwVel *= this._heldItem.MyRigidbody.mass;
			this._heldItem.MyRigidbody.AddForce(throwVel, ForceMode2D.Impulse);
			float angle = Vector2.Angle(Vector2.right, direction);
			this._heldItem.MyRigidbody.SetRotation(angle);
			this._heldItem.MyRigidbody.transform.rotation = Quaternion.Euler(0, 0, angle);
			this._heldItem.MyRigidbody.transform.position = this.ItemMountPoint.transform.position;
			this._heldItem.MyRigidbody.AddTorque(Random.Range(-5f, 5f), ForceMode2D.Impulse);
			this._heldItem = null;
		}
	}

	protected override void ExitState(State previousState)
	{
		switch (this._currentState)
		{
			case State.Empty:
				break;
			case State.Charging:
				break;
			case State.Holding:
				break;
			case State.Yeeting:
				this.CurrentDirection = Direction.None;
				break;
			default:
				break;
		}
	}

	AudioSource _yeetSound;

	protected override void EnterState(State nextState)
	{
		switch (this._currentState)
		{
			case State.Empty:
				break;
			case State.Charging:
				_windup01 = 0f;
				LineRenderer.enabled = true;
				this.CurrentDirection = _brain.FacingRight ? Direction.Right : Direction.Left;
				_yeetSound = AudioManager.Instance.PlaySound(this._brain.Sounds.ChargeYeet, this.transform.position, volume: 1f);
				break;
			case State.Holding:
				AudioManager.Instance.PlaySound(this._brain.Sounds.Grab, this.transform.position, volume: 1f);
				break;
			case State.Yeeting:
				_yeetSound.Stop();
				AudioManager.Instance.PlaySound(this._brain.Sounds.Yeet, this.transform.position, volume: 1f);
				_yeet01 = 0f;
				break;
			default:
				break;
		}
	}

	Vector3 GetYeetDirection()
	{
		Vector2 direction = Vector2.Lerp(
			this.CurrentDirection == Direction.Right ? this.transform.right : this.transform.right * -1f,
			this.transform.up,
			_windup01 * 0.9f);
		return direction.normalized;
	}

	private void UpdateLineRenderer()
	{
		Vector3 direction = GetYeetDirection();
		if (this._currentState == State.Charging)
		{
			this.LineRenderer.enabled = true;
			if (LineRenderer.positionCount != 6)
				LineRenderer.positionCount = 6;
			Vector3 v1 = this.transform.position;
			Vector3 v2 = this.transform.position + (direction * LineRendererLength);
			for (int i = 0; i < 6; i++)
			{
				float t = ((float)i) / 5f;
				LineRenderer.SetPosition(i, Vector3.Lerp(v1, v2, t));
			}


			float angle = Vector2.Angle(Vector2.right, direction);
			this.ItemMountPoint.rotation = Quaternion.Euler(0, 0, angle);
		}
		else
		{
			this.LineRenderer.enabled = false;
			this.ItemMountPoint.localRotation = Quaternion.identity;
		}
	}
}
