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

	const float HitOriginDelay = 8f / 30f; //8 frames at 30fps

	Vector3 _originalScale;

	bool _killMode = false;

	Planet _lastPlanet;
	Vector3 _lastGravity;

	private void Awake()
	{
		_originalScale = this.transform.localScale;
	}

	private void FixedUpdate()
	{
		Planet planet;
		Vector3 gravity = Planet.GetSimpleGravityDirection(this.MyCollider, out planet);
		if (_lastPlanet == null)
		{
			_lastPlanet = planet;
			_lastGravity = gravity;
			return;
		}
		else
		{
			Quaternion rot = Quaternion.FromToRotation(gravity, gravity);
			var r2d = this.GetComponent<Rigidbody2D>();
			//r2d.velocity = rot * r2d.velocity;
			if (planet == _lastPlanet)
			{
				float degreesTravelled = Mathf.Abs( Vector2.SignedAngle(_lastGravity, gravity) );
				r2d.velocity = Vector3.RotateTowards(r2d.velocity, _lastGravity, degreesTravelled * Mathf.Deg2Rad, 0f);
			}
			_lastPlanet = planet;
			_lastGravity = gravity;
		}
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
		_killMode = true;
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

		if (_killMode == true)
			Debug.Log("killhit " + collision.collider.gameObject.name);

		if (collision.otherCollider.CompareTag("Player"))
			collision.otherRigidbody.GetComponent<RatPlayer>().Kill();

		_killMode = false;
	}
}
