using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugGravity : MonoBehaviour
{
	[SerializeField]
	Collider2D _collider;
	private void OnValidate()
	{
		_collider = _collider ?? this.gameObject.AddComponent<BoxCollider2D>();
	}

	private void Update()
	{
	}

	private void OnDrawGizmosSelected()
	{
		Planet p;
		Vector2 gravity = Planet.GetSimpleGravityDirection(_collider, out p);
		Gizmos.color = Color.cyan;
		Gizmos.DrawLine(this.transform.position, this.transform.position + (Vector3)(gravity * 15f));
	}
}
