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

	const float GravityStrength = 2f;

	private void Awake()
	{
		_originalScale = this.transform.localScale;
	}

	void UpdateDrag()
	{
		if (_killMode)
			this.MyRigidbody.drag = 0f;
		else
			this.MyRigidbody.drag = 1f;
	}

	private void FixedUpdate()
	{
		UpdateDrag();
		Planet planet;
		Vector3 gravity = Planet.GetSimpleGravityDirection(this.MyCollider, out planet);
		if (_lastPlanet != null)
		{
			var r2d = this.GetComponent<Rigidbody2D>();
			if (_killMode)
			{
				//r2d.velocity = rot * r2d.velocity;
				if (planet == _lastPlanet)
				{
					float degreesTravelled = Mathf.Abs(Vector2.SignedAngle(_lastGravity, gravity));
					r2d.velocity = Vector3.RotateTowards((Vector3)r2d.velocity, (Vector3)GetPerpendicularGravity(r2d.velocity, gravity), degreesTravelled * Mathf.Deg2Rad, 0f);
				}
			}
			else
			{
				if (!r2d.IsSleeping())
					r2d.AddForce(gravity * GravityStrength);
			}
		}
		_lastPlanet = planet;
		_lastGravity = gravity;
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

		UpdateDrag();
	}

	Vector2 GetPerpendicularGravity(Vector2 velocity, Vector2 gravity)
	{
		Vector2 perpendicular = gravity.Rotate(90f);
		if (Vector2.Dot(velocity, perpendicular) < 0f)
			perpendicular *= -1f;
		return perpendicular;
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

		if (collision.collider.CompareTag("Player"))
			collision.collider.GetComponent<RatPlayer>().Kill();

		_killMode = false;

		if (collision.collider.CompareTag("Planet"))
		{
			var r2d = GetComponent<Rigidbody2D>();
			r2d.velocity = Vector3.zero;
			r2d.Sleep();
		}
	}
}
