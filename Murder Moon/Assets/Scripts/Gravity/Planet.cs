using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
	static Collider2D _dummyCollider;

	static void ValidateCollider(ref Collider2D collider)
	{
		if (_dummyCollider == null && Application.isPlaying)
		{
			var c = new GameObject("Dummer Collider").AddComponent<CircleCollider2D>();
			c.enabled = false;
			c.radius = 0.1f;
			_dummyCollider = c;
		}

		collider = collider ?? _dummyCollider;
	}

	public static Vector2 GetClosestPointOnPlanet(Vector2 position, float offset = 0f)
	{
		Collider2D col = null;
		ValidateCollider(ref col);
		col.transform.position = position;
		return (GetClosestPointOnPlanet(col, offset));
	}

	public static Vector2 GetClosestPointOnPlanet(Collider2D collider, float offset = 0f)
	{
		ValidateCollider(ref collider);
		//bool wasOn = collider.enabled;
		collider.enabled = true;
		var planets = GameObject.FindGameObjectsWithTag("Planet");
		float closestDistance = float.MaxValue;
		GameObject closestPlanetGO = null;
		ColliderDistance2D closestCollisionData = default;
		foreach (var p in planets)
		{
			Collider2D planetCollider = p.GetComponent<Collider2D>();
			var colliderDist = planetCollider.Distance(collider);
			float dist = colliderDist.distance;
			if (dist < closestDistance)
			{
				closestPlanetGO = p;
				closestDistance = dist;
				closestCollisionData = colliderDist;
			}
		}
		//collider.enabled = wasOn;


		if (offset == 0f)
			return closestCollisionData.pointA;
		else
		{
			return ((closestCollisionData.pointB - closestCollisionData.pointA).normalized * offset) + closestCollisionData.pointA;
		}
	}

	public static Vector2 GetSimpleGravityDirection(Collider2D collider, out Planet closestPlanet)
	{
		ValidateCollider(ref collider);
		var planets = GameObject.FindGameObjectsWithTag("Planet");
		float closestDistance = float.MaxValue;
		GameObject closestPlanetGO = null;
		foreach (var p in planets)
		{
			Collider2D planetCollider = p.GetComponent<Collider2D>();
			float dist = planetCollider.Distance(collider).distance;
			if (dist < closestDistance)
			{
				closestPlanetGO = p;
				closestDistance = dist;
			}
		}

		Vector2 gravityDirection = Vector2.down;
		if (closestPlanetGO != null)
		{
			gravityDirection = (closestPlanetGO.transform.position - collider.transform.position).normalized;
			closestPlanet = closestPlanetGO.GetComponent<Planet>();
		}
		else
			closestPlanet = null;

		return gravityDirection;
	}
}
