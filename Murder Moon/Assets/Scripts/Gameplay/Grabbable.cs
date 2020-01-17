using System;
using System.Collections;
using UnityEngine;

public class Grabbable : MonoBehaviour
{
	public bool IsHeld { get; private set; }
	protected Yeeter LastYeeter { get; private set; }
	Vector3 _originalScale;
	bool _kinematic;

	public Rigidbody2D MyRigidbody;
	public Collider2D MyCollider;

	public Action OnGrabCallback;
	public Action OnReleaseCallback;

	const float HitOriginDelay = 8f / 30f; //8 frames at 30fps

	public bool IsPlayer = false;

	protected virtual void Awake()
	{
		_originalScale = this.transform.localScale;
		_kinematic = this.MyRigidbody.isKinematic;
		MyCollider = MyCollider ?? GetComponent<Collider2D>();
	}

	public void Grab(Yeeter yeeter)
	{
		IsHeld = true;
		LastYeeter = yeeter;
		this.transform.SetParent(yeeter.ItemMountPoint);
		this.transform.Reset();
		this.transform.localScale = _originalScale;

		MyCollider.enabled = false;
		MyRigidbody.simulated = false;
		MyRigidbody.isKinematic = this._kinematic;

		OnGrab();
		OnGrabCallback?.Invoke();

		Debug.Log(this.gameObject.name + " grabbed by " + yeeter.gameObject.name);
	}

	public void Release()
	{
		Quaternion prevRot = this.transform.rotation;
		Vector3 prevPos = this.transform.position;
		this.transform.SetParent(null);
		this.transform.Reset();
		this.transform.localScale = _originalScale;
		if (IsPlayer)
		{
			this.transform.position = prevPos;
			this.transform.rotation = prevRot;
		}


		IsHeld = false;

		MyCollider.enabled = true;
		MyRigidbody.isKinematic = false;
		MyRigidbody.simulated = true;

		StartCoroutine(MakeInvulnerable());

		OnRelease();
		OnReleaseCallback?.Invoke();
	}

	public void ReleaseAndSetKinematic()
	{
		Release();

		IsHeld = false;

		MyCollider.enabled = true;
		MyRigidbody.isKinematic = true;
		MyRigidbody.simulated = true;
		MyRigidbody.velocity = Vector2.zero;
		MyRigidbody.angularVelocity = 0f;
	}

	IEnumerator MakeInvulnerable()
	{
		if (this.LastYeeter == null)
			yield break;
		Collider2D[] originColliders = this.LastYeeter.GetComponentsInChildren<Collider2D>();
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

	virtual protected void OnRelease() { }
	virtual protected void OnGrab() { }
}
