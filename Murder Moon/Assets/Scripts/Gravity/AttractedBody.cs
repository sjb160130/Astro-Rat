using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AttractedBody : MonoBehaviour
{
	public static List<AttractedBody> AttractedBodies = new List<AttractedBody>();
	public Rigidbody2D MyRigidbody2D;

	public float Modifier = 1f;

	private void OnEnable()
	{
		AttractedBodies.Add(this);
	}

	private void OnDisable()
	{
		AttractedBodies.Remove(this);
	}

	private void OnValidate()
	{
		this.MyRigidbody2D = this.MyRigidbody2D ?? GetComponent<Rigidbody2D>();
	}
}
