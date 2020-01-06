using UnityEngine;

public class RatBrain : MonoBehaviour
{
	public float gravity = -25f;
	public float runSpeed = 8f;
	public float groundDamping = 20f; // how fast do we change direction? higher means faster
	public float inAirDamping = 5f;
	public float jumpHeight = 3f;

	[HideInInspector]
	private float normalizedHorizontalSpeed = 0;

	private RatController _controller;
	private Animator _animator;
	private Vector3 _velocity;

	RatPlayer _ratPlayer;
	RatCalculator _ratCalculator;

	public SpriteRenderer Sprite;

	public bool FacingRight { get; private set; }

	void Awake()
	{
		_animator = GetComponent<Animator>();
		_controller = GetComponent<RatController>();
		_ratPlayer = GetComponent<RatPlayer>();
		_ratCalculator = GetComponent<RatCalculator>();

		// listen to some events for illustration purposes
		_controller.onControllerCollidedEvent += onControllerCollider;
		_controller.onTriggerEnterEvent += onTriggerEnterEvent;
		_controller.onTriggerExitEvent += onTriggerExitEvent;
	}


	// the Update loop contains a very simple example of moving the character around and controlling the animation
	void Update()
	{
		if (_controller.IsGrounded)
			_velocity.y = 0;

		var player = Rewired.ReInput.players.GetPlayer(this._ratPlayer.PlayerID);
		Vector2 input = player.GetAxis2D("Move X", "Move Y");
		Vector3 worldRight = this._ratCalculator.GetRight();
		normalizedHorizontalSpeed = Mathf.Clamp01(Vector3.Project(input * 1.5f, worldRight).magnitude);
		if (Vector3.Dot(worldRight, input) > 0)
			normalizedHorizontalSpeed *= -1f;

		if (normalizedHorizontalSpeed > 0.1f)
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


		// we can only jump whilst grounded
		if (_controller.IsGrounded && player.GetButtonDown("Jump"))
		{
			_velocity.y = Mathf.Sqrt(2f * jumpHeight * -gravity);
			_animator.Play(Animator.StringToHash("Jump"));
		}


		// apply horizontal speed smoothing it. dont really do this with Lerp. Use SmoothDamp or something that provides more control
		var smoothedMovementFactor = _controller.IsGrounded ? groundDamping : inAirDamping; // how fast do we change direction?
		_velocity.x = Mathf.Lerp(_velocity.x, normalizedHorizontalSpeed * runSpeed, Time.deltaTime * smoothedMovementFactor);

		// apply gravity before movin
		_velocity = _velocity + (-1 * (Vector3)(gravity * Vector3.down * Time.deltaTime));


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
