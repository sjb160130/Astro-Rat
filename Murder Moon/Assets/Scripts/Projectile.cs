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

	Vector3 _originalScale;

	private void Awake()
	{
		_originalScale = this.transform.localScale;
	}

	internal void Grab(Yeeter yeeter)
	{
		IsHeld = true;
		_lastYeeter = yeeter;
		MyCollider.enabled = false;
		MyRigidbody.simulated = false;

		this.transform.SetParent(yeeter.ItemMountPoint);
		this.transform.Reset();
		this.transform.localScale = _originalScale;
	}

	internal void Release()
	{
		this.transform.SetParent(null);
		this.transform.Reset();
		this.transform.localScale = _originalScale;

		IsHeld = false;
		_lastHeldTimestamp = Time.time;
		MyCollider.enabled = true;
		MyRigidbody.simulated = true;
		StartCoroutine(MakeInvulnerable());
	}

	IEnumerator MakeInvulnerable()
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

		if (collision.otherCollider.CompareTag("Player"))
			collision.otherRigidbody.GetComponent<RatPlayer>().Kill();
	}
}
