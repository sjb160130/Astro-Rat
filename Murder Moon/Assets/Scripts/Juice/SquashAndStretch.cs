using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquashAndStretch : MonoBehaviour
{
	public enum AxisDirection { X, Y }

	public Transform SquashAndStretchTransform;
	public Transform SpriteTransform;

	[Header("Look Ahead")]
	public float MinVelocity = 0.01f;
	public Rigidbody2D R2D;
	public RatController Controller;

	[Header("Squash And Stretch")]
	public float Bias = 1f;
	public float Strength = 1f;
	public AxisDirection Axis = AxisDirection.Y;
	public AnimationCurve SnSCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	private Vector2 _startScale;
	private float _scaleMagnitude;

	Vector3 _lastPos;

	private void Awake()
	{
		_lastPos = this.transform.position;
		_startScale = SquashAndStretchTransform.localScale;
		_scaleMagnitude = _startScale.magnitude;
	}

	void Look(Vector3 velocity, float velocityMagnitude)
	{
		if (velocityMagnitude < MinVelocity)
		{
			SquashAndStretchTransform.localRotation = Quaternion.identity;
			return;
		}

		//look by velocity
		var rotation = SquashAndStretchTransform.eulerAngles;

		var angle = Vector2.SignedAngle(Vector2.up, velocity);
		rotation.z = angle;

		SquashAndStretchTransform.eulerAngles = rotation;
	}

	void SnS(Vector3 velocity, float velocityMagnitude)
	{
		if (velocityMagnitude < MinVelocity)
		{
			SquashAndStretchTransform.localScale = Vector3.one;
			return;
		}

		//scale by velocity
		var amount = SnSCurve.Evaluate(velocityMagnitude) * Strength + Bias;
		var inverseAmount = (1f / amount) * _scaleMagnitude;
		if (amount == 0f || float.IsNaN(amount))
		{
			amount = 1f;
			inverseAmount = 1f;
		}


		switch (Axis)
		{
			case AxisDirection.X:
				SquashAndStretchTransform.localScale = new Vector3(amount, inverseAmount, 1f);
				return;
			case AxisDirection.Y:
				SquashAndStretchTransform.localScale = new Vector3(inverseAmount, amount, 1f);
				return;
		}
	}

	void ResetRot(Vector3 velocity, float velocityMagnitude)
	{
		//rotate sprite
		SpriteTransform.rotation = this.transform.rotation;
	}

	private void Update()
	{
		Vector3 velocity;
		if (R2D == null)
			velocity = (this.transform.position - _lastPos) / Time.deltaTime;
		else
			velocity = R2D.isKinematic ? (Controller.Velocity) : (Vector3)R2D.velocity;

		var velocityMagnitude = velocity.magnitude;

		Look(velocity, velocityMagnitude);
		SnS(velocity, velocityMagnitude);
		ResetRot(velocity, velocityMagnitude);

		_lastPos = this.transform.position;
	}
}
