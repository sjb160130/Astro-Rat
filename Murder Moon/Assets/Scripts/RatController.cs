#define DEBUG_CC2D_RAYS
using UnityEngine;
using System;
using System.Collections.Generic;


[RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D))]
public class RatController : MonoBehaviour
{
	public RatCalculator calc;

	public event Action<RaycastHit2D> onControllerCollidedEvent;
	public event Action<Collider2D> onTriggerEnterEvent;
	public event Action<Collider2D> onTriggerStayEvent;
	public event Action<Collider2D> onTriggerExitEvent;

	public Collider2D Collider;

	public bool IsGrounded { get; private set; }

	public Vector3 Velocity { get; private set; }

	Collider2D[] _results = new Collider2D[15];

	public ContactFilter2D CollisionFilter;

	private void Awake()
	{
		calc = GetComponent<RatCalculator>();
	}

	public void Move(Vector3 localMoveDelta)
	{
		IsGrounded = false;

		Vector3 startPoint = this.transform.position;

		this.transform.Translate(localMoveDelta, Space.Self);

		//rotate
		calc.UpdateDirections();
		this.transform.rotation = Quaternion.LookRotation(Vector3.forward, calc.GetUp());

		// Retrieve all colliders we have intersected after velocity has been applied.
		int hitCount = Physics2D.OverlapCollider(Collider, CollisionFilter, _results);


		for (int i = 0; i < hitCount; i++)
		{
			Collider2D hit = _results[i];
			// Ignore our own collider.
			if (hit == Collider)
				continue;

			ColliderDistance2D colliderDistance = hit.Distance(Collider);

			// Ensure that we are still overlapping this collider.
			// The overlap may no longer exist due to another intersected collider
			// pushing us out of this one.
			if (colliderDistance.isOverlapped)
			{
				Vector3 offset = colliderDistance.pointA - colliderDistance.pointB;
				DrawRay(this.transform.position, offset.normalized * 50f, Color.blue);
				transform.Translate(offset, Space.World);

				// If we intersect an object beneath us, set grounded to true. 
				if (Mathf.Abs(Vector2.SignedAngle(colliderDistance.normal, calc.GetUp())) < 90 && localMoveDelta.y < 0)
				{
					IsGrounded = true;
				}
			}
		}

		this.Velocity = (this.transform.position - startPoint) / Time.deltaTime;

		DrawRay(this.transform.position, this.Velocity, Color.yellow);
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
