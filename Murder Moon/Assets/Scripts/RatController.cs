#define DEBUG_CC2D_RAYS
using UnityEngine;
using System;
using System.Collections.Generic;


[RequireComponent( typeof( BoxCollider2D ), typeof( Rigidbody2D ) )]
public class RatController : MonoBehaviour
{
	struct CharacterRaycastOrigins
	{
		public Vector3 topLeft;
		public Vector3 bottomRight;
		public Vector3 bottomLeft;
	}


    public Transform TopLeft;
    public Transform TopRight;
    public Transform BottomLeft;
    public Transform BottomRight;


    public RatCalcuator calc;

	public event Action<RaycastHit2D> onControllerCollidedEvent;
	public event Action<Collider2D> onTriggerEnterEvent;
	public event Action<Collider2D> onTriggerStayEvent;
	public event Action<Collider2D> onTriggerExitEvent;

	

	[SerializeField]
	[Range( 0.001f, 0.3f )]
	float _skinWidth = 0.02f;

	/// <summary>
	/// defines how far in from the edges of the collider rays are cast from. If cast with a 0 extent it will often result in ray hits that are
	/// not desired (for example a foot collider casting horizontally from directly on the surface can result in a hit)
	/// </summary>
	public float skinWidth
	{
		get { return _skinWidth; }
		set
		{
			_skinWidth = value;
			recalculateDistanceBetweenRays();
		}
	}


	/// <summary>
	/// mask with all layers that the player should interact with
	/// </summary>
	public LayerMask platformMask = 0;

	/// <summary>
	/// mask with all layers that trigger events should fire when intersected
	/// </summary>
	public LayerMask triggerMask = 0;

	/// <summary>
	/// the threshold in the change in vertical movement between frames that constitutes jumping
	/// </summary>
	/// <value>The jumping threshold.</value>
	public float jumpingThreshold = 0.07f;


	/// <summary>
	/// curve for multiplying speed based on slope (negative = down slope and positive = up slope)
	/// </summary>
	public AnimationCurve slopeSpeedMultiplier = new AnimationCurve( new Keyframe( -90f, 1.5f ), new Keyframe( 0f, 1f ), new Keyframe( 90f, 0f ) );

	[Range( 2, 20 )]
	public int totalHorizontalRays = 8;
	[Range( 2, 20 )]
	public int totalVerticalRays = 4;


	/// <summary>
	/// this is used to calculate the downward ray that is cast to check for slopes. We use the somewhat arbitrary value 75 degrees
	/// to calculate the length of the ray that checks for slopes.
	/// </summary>
	float _slopeLimitTangent = Mathf.Tan( 75f * Mathf.Deg2Rad );


	[HideInInspector][NonSerialized]
	public new Transform transform;
	[HideInInspector][NonSerialized]
	public BoxCollider2D boxCollider;
	[HideInInspector][NonSerialized]
	public Rigidbody2D rigidBody2D;

	[HideInInspector][NonSerialized]
	public RatCollisionState collisionState = new RatCollisionState();
	[HideInInspector][NonSerialized]
	public Vector3 velocity;
	public bool isGrounded { get { return collisionState.below; } }

	const float kSkinWidthFloatFudgeFactor = 0.001f;

	/// <summary>
	/// holder for our raycast origin corners (TR, TL, BR, BL)
	/// </summary>
	CharacterRaycastOrigins _raycastOrigins;

	/// <summary>
	/// stores our raycast hit during movement
	/// </summary>
	RaycastHit2D _raycastHit;

	/// <summary>
	/// stores any raycast hits that occur this frame. we have to store them in case we get a hit moving
	/// horizontally and vertically so that we can send the events after all collision state is set
	/// </summary>
	List<RaycastHit2D> _raycastHitsThisFrame = new List<RaycastHit2D>( 2 );

	// horizontal/vertical movement data
	float _verticalDistanceBetweenRays;
	float _horizontalDistanceBetweenRays;

	// we use this flag to mark the case where we are travelling up a slope and we modified our delta.y to allow the climb to occur.
	// the reason is so that if we reach the end of the slope we can make an adjustment to stay grounded
	bool _isGoingUpSlope = false;


    public Vector2 GetDown()
    {
        return calc.GetDown();
    }


	void Awake()
	{
        calc = GetComponent<RatCalcuator>();
		transform = GetComponent<Transform>();
		boxCollider = GetComponent<BoxCollider2D>();
		rigidBody2D = GetComponent<Rigidbody2D>();

		// here, we trigger our properties that have setters with bodies
		skinWidth = _skinWidth;

		// we want to set our CC2D to ignore all collision layers except what is in our triggerMask
		for( var i = 0; i < 32; i++ )
		{
			// see if our triggerMask contains this layer and if not ignore it
			if( ( triggerMask.value & 1 << i ) == 0 )
				Physics2D.IgnoreLayerCollision( gameObject.layer, i );
		}
	}



	/// <summary>
	/// attempts to move the character to position + deltaMovement. Any colliders in the way will cause the movement to
	/// stop when run into.
	/// </summary>
	/// <param name="deltaMovement">Delta movement.</param>
	public void move( Vector3 deltaMovement )
	{
        DrawRay(transform.position, calc.GetDown() * 1000, Color.yellow);
        DrawRay(transform.position, calc.GetLeft() * 1000, Color.magenta);


        // save off our current grounded state which we will use for wasGroundedLastFrame and becameGroundedThisFrame
        collisionState.wasGroundedLastFrame = collisionState.below;

		// clear our state
		collisionState.reset();
		_raycastHitsThisFrame.Clear();

		primeRaycastOrigins();


		// now we check movement in the horizontal dir
		if( deltaMovement.x != 0f )
			moveHorizontally( ref deltaMovement );

		// next, check movement in the vertical dir
		if( deltaMovement.y != 0f )
			moveVertically( ref deltaMovement );

		// move then update our state
		deltaMovement.z = 0;
		transform.Translate( deltaMovement, Space.World );

		// only calculate velocity if we have a non-zero deltaTime
		if( Time.deltaTime > 0f )
			velocity = deltaMovement / Time.deltaTime;

		// set our becameGrounded state based on the previous and current collision state
		if( !collisionState.wasGroundedLastFrame && collisionState.below )
			collisionState.becameGroundedThisFrame = true;

		// send off the collision events if we have a listener
		if( onControllerCollidedEvent != null )
		{
			for( var i = 0; i < _raycastHitsThisFrame.Count; i++ )
				onControllerCollidedEvent( _raycastHitsThisFrame[i] );
		}

	
	}


	/// <summary>
	/// this should be called anytime you have to modify the BoxCollider2D at runtime. It will recalculate the distance between the rays used for collision detection.
	/// It is also used in the skinWidth setter in case it is changed at runtime.
	/// </summary>
	public void recalculateDistanceBetweenRays()
	{
		// figure out the distance between our rays in both directions
		// horizontal
		//var colliderUseableHeight = boxCollider.size.y * Mathf.Abs( transform.localScale.y ) - ( 2f * _skinWidth );
		//_verticalDistanceBetweenRays = colliderUseableHeight / ( totalHorizontalRays - 1 );

        //// vertical
        //var colliderUseableWidth = boxCollider.size.x * Mathf.Abs( transform.localScale.x ) - ( 2f * _skinWidth );
        //_horizontalDistanceBetweenRays = colliderUseableWidth / ( totalVerticalRays - 1 );


        // horizontal
        var colliderUseableHeight = Mathf.Abs(TopLeft.position.y - TopRight.position.y);
        _verticalDistanceBetweenRays = colliderUseableHeight / (totalHorizontalRays - 1);

        //// vertical
        var colliderUseableWidth = Mathf.Abs(TopLeft.position.y - BottomLeft.position.y);
        _horizontalDistanceBetweenRays = colliderUseableWidth / ( totalVerticalRays - 1 );

    }





    /// <summary>
    /// resets the raycastOrigins to the current extents of the box collider inset by the skinWidth. It is inset
    /// to avoid casting a ray from a position directly touching another collider which results in wonky normal data.
    /// </summary>
    /// <param name="futurePosition">Future position.</param>
    /// <param name="deltaMovement">Delta movement.</param>
    void primeRaycastOrigins()
	{
        _raycastOrigins.topLeft = TopLeft.position;
		_raycastOrigins.bottomRight = BottomRight.position;
        _raycastOrigins.bottomLeft = BottomLeft.position;
	}


	/// <summary>
	/// we have to use a bit of trickery in this one. The rays must be cast from a small distance inside of our
	/// collider (skinWidth) to avoid zero distance rays which will get the wrong normal. Because of this small offset
	/// we have to increase the ray distance skinWidth then remember to remove skinWidth from deltaMovement before
	/// actually moving the player
	/// </summary>
	void moveHorizontally( ref Vector3 deltaMovement )
	{
		var isGoingRight = deltaMovement.x > 0;
		var rayDistance = Mathf.Abs( deltaMovement.x ) + _skinWidth;
		var rayDirection = isGoingRight ? calc.GetRight() : calc.GetLeft();
		var initialRayOrigin = isGoingRight ? _raycastOrigins.bottomRight : _raycastOrigins.bottomLeft;

		for( var i = 0; i < totalHorizontalRays; i++ )
		{
			var ray = new Vector2( initialRayOrigin.x, initialRayOrigin.y + i * _verticalDistanceBetweenRays );

			//DrawRay( ray, (rayDirection * rayDistance) * 10, Color.red );

			// if we are grounded we will include oneWayPlatforms only on the first ray (the bottom one). this will allow us to
			// walk up sloped oneWayPlatforms
			if( i == 0 && collisionState.wasGroundedLastFrame )
				_raycastHit = Physics2D.Raycast( ray, rayDirection, rayDistance, platformMask );

			if( _raycastHit )
			{
				// set our new deltaMovement and recalculate the rayDistance taking it into account
				deltaMovement.x = _raycastHit.point.x - ray.x;
				rayDistance = Mathf.Abs( deltaMovement.x );

				// remember to remove the skinWidth from our deltaMovement
				if( isGoingRight )
				{
					deltaMovement.x -= _skinWidth;
					collisionState.right = true;
				}
				else
				{
					deltaMovement.x += _skinWidth;
					collisionState.left = true;
				}

				_raycastHitsThisFrame.Add( _raycastHit );

				// we add a small fudge factor for the float operations here. if our rayDistance is smaller
				// than the width + fudge bail out because we have a direct impact
				if( rayDistance < _skinWidth + kSkinWidthFloatFudgeFactor )
					break;
			}
		}
	}


	void moveVertically( ref Vector3 deltaMovement )
	{

		var isGoingUp = deltaMovement.y > 0;
		var rayDistance = Mathf.Abs( deltaMovement.y ) + _skinWidth;
		var rayDirection = isGoingUp ? calc.GetUp() : calc.GetDown();
        rayDirection = calc.GetDown();


          var initialRayOrigin = isGoingUp ? _raycastOrigins.topLeft : _raycastOrigins.bottomLeft;
  
        // apply our horizontal deltaMovement here so that we do our raycast from the actual position we would be in if we had moved
        initialRayOrigin.x += deltaMovement.x;

		// if we are moving up, we should ignore the layers in oneWayPlatformMask
		var mask = platformMask;

		for( var i = 0; i < totalVerticalRays; i++ )
		{
			var rayOrigin = new Vector2( initialRayOrigin.x + (i * _horizontalDistanceBetweenRays), initialRayOrigin.y );

            //DrawRay( rayOrigin, (rayDirection * rayDistance) * 10, Color.green );
			_raycastHit = Physics2D.Raycast( rayOrigin, rayDirection, rayDistance, mask );
			if( _raycastHit )
			{
				// set our new deltaMovement and recalculate the rayDistance taking it into account
				deltaMovement.y = _raycastHit.point.y - rayOrigin.y;
				rayDistance = Mathf.Abs( deltaMovement.y );

				// remember to remove the skinWidth from our deltaMovement
				if( isGoingUp )
				{
					deltaMovement.y -= _skinWidth;
					collisionState.above = true;
				}
				else
				{
					deltaMovement.y += _skinWidth;
					collisionState.below = true;
				}

				_raycastHitsThisFrame.Add( _raycastHit );

				// we add a small fudge factor for the float operations here. if our rayDistance is smaller
				// than the width + fudge bail out because we have a direct impact
				if( rayDistance < _skinWidth + kSkinWidthFloatFudgeFactor )
					break;
			}
		}
	}





    public void OnTriggerEnter2D(Collider2D col)
    {
        if (onTriggerEnterEvent != null)
            onTriggerEnterEvent(col);
    }


    public void OnTriggerStay2D(Collider2D col)
    {
        if (onTriggerStayEvent != null)
            onTriggerStayEvent(col);
    }


    public void OnTriggerExit2D(Collider2D col)
    {
        if (onTriggerExitEvent != null)
            onTriggerExitEvent(col);
    }




    [System.Diagnostics.Conditional("DEBUG_CC2D_RAYS")]
    void DrawRay(Vector3 start, Vector3 dir, Color color)
    {
        Debug.DrawRay(start, dir, color);
    }


}
