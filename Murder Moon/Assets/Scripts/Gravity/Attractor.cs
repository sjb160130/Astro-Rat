using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Attractor : MonoBehaviour
{
	public Rigidbody2D MyRigidbody2D;

	const float G = 6.674f;

	private void OnValidate()
	{
		this.MyRigidbody2D = this.MyRigidbody2D ?? GetComponent<Rigidbody2D>();
	}

	private void FixedUpdate()
	{
		var bodies = AttractedBody.AttractedBodies;
		for (int i = 0; i < bodies.Count; i++)
		{
			Attract(bodies[i]);
		}
	}

	void Attract(AttractedBody otherAttractor)
	{
		Rigidbody2D rb = otherAttractor.MyRigidbody2D;

		Vector3 myCenterOfMass = this.MyRigidbody2D.worldCenterOfMass;
		Vector3 otherCenterOfMass = rb.worldCenterOfMass;
		Vector3 offset = otherCenterOfMass - myCenterOfMass;
		float sqrOffset = offset.sqrMagnitude;
		if (sqrOffset == 0f)
			return;
		float forceMagnitude = G * (rb.mass * this.MyRigidbody2D.mass) / sqrOffset;

		rb.AddForce(offset.normalized * forceMagnitude * -1f * Time.deltaTime);
	}
}
