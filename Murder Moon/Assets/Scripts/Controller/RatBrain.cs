using UnityEngine;

public class RatBrain : MonoBehaviour
{
	public float jumpGravity = -11.84f;
	public float gravityAfterButtonRelease = -20f;
	public float gravityAfterPeak = -25f;
	public float runSpeed = 8f;
	public float yeetinSpeed = 0.5f;
	public float groundDamping = 20f; // how fast do we change direction? higher means faster
	public float inAirDamping = 5f;
	public float jumpHeight = 3f;

	[HideInInspector]
	private float normalizedHorizontalSpeed = 0;

	private RatController _controller;
	private RatPlayer _player;
	private Yeeter _yeeter;
	private Animator _animator;
	private Vector3 _velocity;

	private bool _isHoldingJump;

	RatPlayer _ratPlayer;
	RatCalculator _ratCalculator;

	public SpriteRenderer Sprite;

	public bool FacingRight { get; private set; }

	void FixNaN()
	{
		if (float.IsNaN(this._velocity.x))
			this._velocity = Vector3.zero;
	}

	void Awake()
	{
		_animator = GetComponent<Animator>();
		_controller = GetComponent<RatController>();
		_player = GetComponent<RatPlayer>();
		_yeeter = GetComponent<Yeeter>();
		_ratPlayer = GetComponent<RatPlayer>();
		_ratCalculator = GetComponent<RatCalculator>();

		// listen to some events for illustration purposes
		_controller.onControllerCollidedEvent += onControllerCollider;
		_controller.onTriggerEnterEvent += onTriggerEnterEvent;
		_controller.onTriggerExitEvent += onTriggerExitEvent;
	}

	void SetFlipDirection(float direction)
	{
		if (direction > 0.1f)
		{
			Sprite.flipX = false;
			FacingRight = true;
		}
		else if (direction < -0.1f)
		{
			Sprite.flipX = true;
			FacingRight = false;
		}
	}

	// the Update loop contains a very simple example of moving the character around and controlling the animation
	void Update()
	{
		FixNaN();
		if (_controller.IsGrounded)
			_velocity.y = 0;

		var player = Rewired.ReInput.players.GetPlayer(this._ratPlayer.PlayerID);
		Vector2 input = player.GetAxis2D("Move X", "Move Y");
		Vector3 worldRight = this._ratCalculator.GetRight();

		normalizedHorizontalSpeed = Mathf.Clamp01(Vector3.Project(input * 1.5f, worldRight).magnitude);
		if (Vector3.Dot(worldRight, input) > 0)
			normalizedHorizontalSpeed *= -1f;

		float speed = this.runSpeed;
		bool canJump = true;

		if (_player.Dead == true)
		{
			speed = 0f;
			_animator.Play(Animator.StringToHash("Dead"));
		}
		else if (this._yeeter.CurrentState == Yeeter.State.Charging)
		{
			_animator.Play(Animator.StringToHash("Charging"));
			canJump = false;
			speed = this.yeetinSpeed;
			SetFlipDirection(normalizedHorizontalSpeed);
		}
		else if (this._yeeter.CurrentState == Yeeter.State.Yeeting)
		{
			_animator.Play(Animator.StringToHash("Yeet"));
			canJump = false;
			speed = this.yeetinSpeed / 2f;
			SetFlipDirection(normalizedHorizontalSpeed);
		}
		else if (normalizedHorizontalSpeed > 0.1f)
		{
			if (_controller.IsGrounded)
				_animator.Play(Animator.StringToHash("Run"));

			Sprite.flipX = false;
			FacingRight = true;
		}
		else if (normalizedHorizontalSpeed < -0.1f)
		{
			normalizedHorizontalSpeed = -1;

			if (_controller.IsGrounded)
				_animator.Play(Animator.StringToHash("Run"));

			Sprite.flipX = true;
			FacingRight = false;
		}
		else
		{
			normalizedHorizontalSpeed = 0;

			if (_controller.IsGrounded)
				_animator.Play(Animator.StringToHash("Idle"));
		}

		_isHoldingJump = _isHoldingJump && player.GetButton("Jump");

		float gravity = 0f;

		// we can only jump whilst grounded
		if (_controller.IsGrounded && player.GetButtonDown("Jump") && canJump)
		{
			_velocity.y = Mathf.Sqrt(2f * jumpHeight * -this.jumpGravity);
			_animator.Play(Animator.StringToHash("Jump"));
			_isHoldingJump = true;
			gravity = 0f;
		}
		else if (_velocity.y <= 0f)
		{
			// apply down gravity
			gravity = gravityAfterPeak;
		}
		else if (_isHoldingJump)
		{
			// apply held gravity
			gravity = this.jumpGravity;
		}
		else
		{
			// apply releaded gravity
			gravity = gravityAfterButtonRelease;
		}

		//apply gravity
		_velocity.y = _velocity.y + (gravity * Time.deltaTime);


		// apply horizontal speed smoothing it. dont really do this with Lerp. Use SmoothDamp or something that provides more control
		var smoothedMovementFactor = _controller.IsGrounded ? groundDamping : inAirDamping; // how fast do we change direction?
		_velocity.x = Mathf.Lerp(_velocity.x, normalizedHorizontalSpeed * speed, Time.deltaTime * smoothedMovementFactor);

		FixNaN();
		_controller.Move(_velocity * Time.deltaTime);

		// grab our current _velocity to use as a base for all calculations
		_velocity = this.transform.InverseTransformDirection(_controller.Velocity);
	}


	void onControllerCollider(RaycastHit2D hit)
	{
		// bail out on plain old ground hits cause they arent very interesting
		if (hit.normal.y == 1f)
			return;

		// logs any collider hits if uncommented. it gets noisy so it is commented out for the demo
		//Debug.Log( "flags: " + _controller.collisionState + ", hit.normal: " + hit.normal );
	}


	void onTriggerEnterEvent(Collider2D col)
	{
		Debug.Log("onTriggerEnterEvent: " + col.gameObject.name);
	}


	void onTriggerExitEvent(Collider2D col)
	{
		Debug.Log("onTriggerExitEvent: " + col.gameObject.name);
	}

}
