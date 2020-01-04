using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
	public bool IsHeld { get; internal set; }

	Yeeter _lastYeeter;
	float _lastHeldTimestamp;

	public Rigidbody2D MyRigidbody;
	public Collider2D MyCollider;

	const float HitOriginDelay = 0.18f; //3 frames at 30fps

	internal void Grab(Yeeter yeeter)
	{
		IsHeld = true;
		_lastYeeter = yeeter;
		MyCollider.enabled = false;
		MyRigidbody.simulated = false;
	}

	internal void Release()
	{
		IsHeld = false;
		_lastHeldTimestamp = Time.time;
		MyCollider.enabled = true;
		MyRigidbody.simulated = true;
		StartCoroutine(MakeInvulnerabale());
	}

	IEnumerator MakeInvulnerabale()
	{
		Collider2D[] originColliders = _lastYeeter.GetComponentsInChildren<Collider2D>();
		foreach (var collider in originColliders)
		{
			Physics2D.IgnoreCollision(collider, this.MyCollider, true);
		}

		yield return new WaitForSeconds(HitOriginDelay);

		foreach (var collider in originColliders)
		{
			Physics2D.IgnoreCollision(collider, this.MyCollider, false);
		}
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (IsHeld == true)
			return;
		Debug.Log("hit");
	}
}
