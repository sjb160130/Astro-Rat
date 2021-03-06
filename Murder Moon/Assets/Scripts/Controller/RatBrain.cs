﻿using System.Collections.Generic;
using System.Linq;
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

    public string SpriteSheetName;
    public SpriteRenderer Sprite;

    public bool FacingRight { get; private set; }

    public SpriteRenderer crownSprite;

    public RatSounds Sounds { get { return this._ratPlayer.Sounds; } }

    [HideInInspector]
    private float normalizedHorizontalSpeed = 0;

    private RatController _controller;
    private RatPlayer _player;
    private Yeeter _yeeter;
    private Animator _animator;
    private Vector3 _velocity;

    private bool _isHoldingJump;

    private RatPlayer _ratPlayer;
    private RatCalculator _ratCalculator;
    private Grabbable _grabbable;
    private Rigidbody2D _myRigidbody;

    public bool IsYote { get; private set; }

	public Transform[] FlipScaleWhenLeft;

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
        _grabbable = GetComponent<Grabbable>();
        _grabbable.OnGrabCallback += this.OnGrab;
        _grabbable.OnReleaseCallback += this.OnRelease;
        _myRigidbody = GetComponent<Rigidbody2D>();

        // listen to some events for illustration purposes
        _controller.onControllerCollidedEvent += onControllerCollider;
        _controller.onTriggerEnterEvent += OnTriggerEnterEvent;
        _controller.onTriggerExitEvent += OnTriggerExitEvent;

        crownSprite.enabled = false;

    }

    private void Start()
    {
        SetUpSpriteSheet();
    }

    void SetFlipDirection(float direction)
    {
        if (direction > 0.1f)
        {
            Sprite.flipX = false;
            FacingRight = true;
			foreach (var t in FlipScaleWhenLeft)
				t.localScale = new Vector3(1f, 1f, 1f);
        }
        else if (direction < -0.1f)
        {
            Sprite.flipX = true;
            FacingRight = false;
			foreach (var t in FlipScaleWhenLeft)
				t.localScale = new Vector3(-1f, 1f, 1f);
		}
    }

    // the Update loop contains a very simple example of moving the character around and controlling the animation
    void Update()
    {
        if (this._grabbable.IsHeld)
		{
			this._myRigidbody.isKinematic = true;
			Rewired.Player p = Rewired.ReInput.players.GetPlayer(this._ratPlayer.PlayerID);
			if (p.GetButtonDoublePressDown("Jump") || p.GetButtonDoublePressDown("Interact"))
			{
				this.GetComponent<Projectile>()?.LastYeeter?.Drop();
				this._velocity = Vector3.zero;
				this._myRigidbody.velocity = Vector3.zero;
				this._myRigidbody.angularVelocity = 0f;
				//depen with no fling after the fact.
				this._controller.Move(Vector3.zero);
				this._controller.ResetVelocity();
			}
			_animator.Play(Animator.StringToHash("Yote"));
		}
        else if (this.IsYote)
        {
            Rewired.Player p = Rewired.ReInput.players.GetPlayer(this._ratPlayer.PlayerID);
            if (p.GetButtonDown("Jump") || p.GetButtonDown("Interact") || this._myRigidbody.isKinematic)
            {
                LeaveYote();
			}
			_animator.Play(Animator.StringToHash("Yote"));
		}
        else
		{
			//we shouldn't need to set these values, but this is a quickfix for some weird edgecase I can't repro.
			this._myRigidbody.isKinematic = true;
			this._myRigidbody.simulated = true;
			DoMove();
        }
    }

    void DoMove()
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
        }
        else if (this._yeeter.CurrentState == Yeeter.State.Yeeting)
        {
            _animator.Play(Animator.StringToHash("Yeet"));
            canJump = false;
            speed = this.yeetinSpeed / 2f;
            this._velocity = Vector3.zero;
            this._myRigidbody.velocity = Vector3.zero;
        }
        else if (normalizedHorizontalSpeed > 0.1f)
        {
            if (_controller.IsGrounded)
                _animator.Play(Animator.StringToHash("Run"));

            SetFlipDirection(normalizedHorizontalSpeed);
        }
        else if (normalizedHorizontalSpeed < -0.1f)
        {
            normalizedHorizontalSpeed = -1;

            if (_controller.IsGrounded)
                _animator.Play(Animator.StringToHash("Run"));

            SetFlipDirection(normalizedHorizontalSpeed);
        }
        else
        {
            normalizedHorizontalSpeed = 0;

            if (_controller.IsGrounded)
                _animator.Play(Animator.StringToHash("Idle"));
        }

        _isHoldingJump = _isHoldingJump && player.GetButton("Jump");

        float gravity = 0f;

        bool wasGrounded = this._controller.IsGrounded;

        // we can only jump whilst grounded
		if (this._ratPlayer.Dead)
		{
			gravity = gravityAfterButtonRelease;
		}
		else if (_controller.IsGrounded && player.GetButtonDown("Jump") && canJump)
        {
            _velocity.y = Mathf.Sqrt(2f * jumpHeight * -this.jumpGravity);
            _animator.Play(Animator.StringToHash("Jump"));
            _isHoldingJump = true;
            gravity = 0f;
			AudioManager.Instance.PlaySound(this.Sounds.Jump, this.transform.position, volume: 0.8f);
        }
        else if (_velocity.y <= 0f)
        {
            // apply down gravity
            gravity = gravityAfterPeak;
            if (!_controller.IsGrounded && this._yeeter.CurrentState != Yeeter.State.Yeeting && this._yeeter.CurrentState != Yeeter.State.Charging)
                _animator.Play(Animator.StringToHash("Falling"));
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

        //check if landed
        if (!wasGrounded && _controller.IsGrounded)
            AudioManager.Instance.PlaySound(this.Sounds.Land, this.transform.position);

        // grab our current _velocity to use as a base for all calculations
        _velocity = this.transform.InverseTransformDirection(_controller.Velocity);
    }



    Dictionary<string, Sprite> spriteSheet;
    // Runs after the animation has done its work
    private void LateUpdate()
    {
        try
        {
            Sprite.sprite = spriteSheet[Sprite.sprite.name];
        }
        catch
        {
            Debug.LogError("ERROR - Missing frame - " + Sprite.sprite.name);
        }
    }

  

    private void SetUpSpriteSheet()
    {
        if (SpriteSheetName.Count() != 0 && SpriteSheetName != null)
        {
            var sprites = Resources.LoadAll<Sprite>(this.SpriteSheetName);

            try
            {
                spriteSheet = sprites.ToDictionary(x => x.name, x => x);
                Sprite.sprite = spriteSheet[Sprite.sprite.name];
            }
            catch
            {
                Debug.LogError(gameObject.name + " - ERROR - Missing frame - " + Sprite.sprite.name);
            }

        }
    }

    public void SetCrownWinner(bool flag)
    {
        crownSprite.enabled = flag;
    }

    void onControllerCollider(RaycastHit2D hit)
    {
        // bail out on plain old ground hits cause they arent very interesting
        if (hit.normal.y == 1f)
            return;

        // logs any collider hits if uncommented. it gets noisy so it is commented out for the demo
        //Debug.Log( "flags: " + _controller.collisionState + ", hit.normal: " + hit.normal );
    }


    void OnTriggerEnterEvent(Collider2D col)
    {
        Debug.Log("onTriggerEnterEvent: " + col.gameObject.name);
    }


    void OnTriggerExitEvent(Collider2D col)
    {
        Debug.Log("onTriggerExitEvent: " + col.gameObject.name);
    }

    private void OnCollisionEnter(Collision collision)
    {
        LeaveYote();
    }

    void LeaveYote()
    {
        if (IsYote)
		{
			Projectile p = GetComponent<Projectile>();
			p?.ResetKillmode();
			p?.ReleaseAndSetKinematic();
			this._animator.Play("Idle");
            this._yeeter.enabled = true;
            this._velocity = this._myRigidbody.velocity;
            this.IsYote = false;
		}
    }

    protected void OnRelease()
    {
        IsYote = true;
    }

    protected void OnGrab()
    {
        this._yeeter.Drop();
        this._yeeter.enabled = false;
    }
}
