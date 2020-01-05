using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
	public static Vector2 GetSimpleGravityDirection(Collider2D collider, out Planet closestPlanet)
	{
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
