﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PKG;

public class Projectile : Grabbable
{
	public Animator Animator;

	public bool SpinOnThrow;

	Planet _lastPlanet;
	Vector3 _lastGravity;

	const float GravityStrength = 2f;

	public bool Kills = true;

	public AudioClip HitSFX;
	public AudioClip HitGroundSFX;

	void UpdateDrag()
	{
		if (_killMode)
			this.MyRigidbody.drag = 0f;
		else
			this.MyRigidbody.drag = 1f;
	}

	private void FixedUpdate()
	{
		UpdateLayer();

		UpdateDrag();
		var r2d = this.GetComponent<Rigidbody2D>();
		if (r2d.isKinematic)
			return;
		Planet planet;
		Vector3 gravity = Planet.GetSimpleGravityDirection(this.MyCollider, out planet);
		if (_lastPlanet != null)
		{
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

	private void LateUpdate()
	{
		if (!IsPlayer && Vector2.Distance(this.gameObject.transform.position, new Vector2(0, 0)) > 30)
		{
			PoolManager.Instance.releaseObject(this.gameObject);
		}
	}

	protected void RevolvePlanet(Planet p, Vector3 gravity)
	{

	}

	protected override void OnGrab()
	{
	}

	public void ResetKillmode()
	{
		_killMode = false;
	}

	protected override void OnRelease(bool kill)
	{
		_killMode = kill;

		UpdateDrag();

		if (SpinOnThrow)
			this.Animator?.Play("ItemSpin");
		else
			this.Animator?.Play("ItemIdle");
	}

	Vector2 GetPerpendicularGravity(Vector2 velocity, Vector2 gravity)
	{
		Vector2 perpendicular = gravity.Rotate(90f);
		if (Vector2.Dot(velocity, perpendicular) < 0f)
			perpendicular *= -1f;
		return perpendicular;
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (IsHeld == true)
			return;

		if (IsPlayer)
		{
			RatBrain brain = GetComponent<RatBrain>();
			if (brain.IsYote == false)
				return;
		}

		if (_killMode == true)
			Debug.Log("killhit " + collision.collider.gameObject.name);

		if (collision.collider.CompareTag("Player") && _killMode && Kills)
		{
			RatPlayer yeeter = this.LastYeeter.GetComponent<RatPlayer>();
			RatPlayer hit = collision.collider.GetComponent<RatPlayer>();
			hit.Kill();
			if (hit != yeeter)
				yeeter.AwardPoint();
			Cinemachine.CinemachineImpulseSource impulse = GetComponent<Cinemachine.CinemachineImpulseSource>();
			impulse?.GenerateImpulse(this.MyRigidbody.velocity);
			AudioManager.Instance.PlaySound(HitSFX, this.transform.position);

			if (!IsPlayer)
				PoolManager.Instance.releaseObject(this.gameObject);

			Cloud.Spawn(collision.contacts[0].point, Cloud.Size.Large);
			_killMode = false;
		}
		else
		{
			if (_killMode) {
				AudioManager.Instance.PlaySound(HitGroundSFX, collision.contacts[0].point, AudioManager.MixerGroup.SFX, 0.8f);
				Cloud.Spawn(collision.contacts[0].point, Cloud.Size.Small);
			}
			_killMode = false;
		}


		if (SpinOnThrow)
			this.Animator?.Play("ItemSpin");
		else
			this.Animator?.Play("ItemIdle");

		if (collision.collider.CompareTag("Planet"))
		{
			var r2d = GetComponent<Rigidbody2D>();
			r2d.velocity = Vector3.zero;
			r2d.Sleep();
		}

		if (IsPlayer)
			ReleaseAndSetKinematic();
	}
}
