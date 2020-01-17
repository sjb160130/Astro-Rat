using PKG;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
	[System.Serializable]
	public class PlanetAndData
	{
		public GameObject Prefab;
		public float Radius;
	}
	public float MandatoryPlanetPadding = 1f;
	public float EdgePadding = 5f;

	public PlanetAndData[] PlanetsAndConf;

	static public LevelGenerator Instance { get; private set; }

	[HideInInspector, SerializeField]
	float _smallestPlanetRadius, _largestPlanetRadius;

	public class ScreenData : IComparable
	{
		public Bounds Bounds;

		public ScreenData(Bounds bounds)
		{
			Bounds = bounds;
		}

		public int CompareTo(object obj)
		{
			if (obj.GetType().IsInstanceOfType(typeof(ScreenData)))
				return 0;
			return this.GetHashCode().CompareTo(obj.GetHashCode());
		}
	}

	private void OnValidate()
	{
		if (PlanetsAndConf == null)
			return;

		_smallestPlanetRadius = float.MaxValue;
		_largestPlanetRadius = float.MinValue;

		foreach (PlanetAndData planetData in PlanetsAndConf)
		{
			if (planetData == null || planetData.Prefab == null)
				continue;
			var renderer = planetData.Prefab.GetComponentInChildren<Renderer>(); ;
			var bounds = renderer.bounds;
			planetData.Radius = Mathf.Max(bounds.extents.x, bounds.extents.y);
			_smallestPlanetRadius = Mathf.Min(_smallestPlanetRadius, planetData.Radius);
			_largestPlanetRadius = Mathf.Max(_largestPlanetRadius, planetData.Radius);
		}

		Array.Sort(PlanetsAndConf, (x, y) => { return x.Radius.CompareTo(y.Radius); });
	}

	private void Awake()
	{
		Instance = this;
	}

	public void Run()
	{
		Debug.Log("Generating Level");
		int count = MonitorManager.Instance.DisplayCount;
		ScreenData[] screenData = new ScreenData[count];
		for (int i = 0; i < count; i++)
		{
			screenData[i] = new ScreenData(MonitorManager.Instance.GetBounds(i));
		}
		Build(screenData);
	}

	public void Build(ScreenData[] screenBounds)
	{
		foreach (var go in GameObject.FindGameObjectsWithTag("Planet"))
		{
			PoolManager.ReleaseObject(go);
		}

		var screenJunctions = new HashSet<Tuple<ScreenData, ScreenData>>(new TupleComparer());
		Dictionary<ScreenData, List<GameObject>> spawnedPlanets = new Dictionary<ScreenData, List<GameObject>>();

		//generate data for screen juntions, creating a map where you can traverse the screens
		for (int i = 0; i < screenBounds.Length; i++)
		{
			spawnedPlanets.Add(screenBounds[i], new List<GameObject>());

			ScreenData closestScreen = null;
			float closestScreenDistance = float.MaxValue;

			for (int j = 0; j < screenBounds.Length; j++)
			{
				if (j == i)
					continue;
				float dist = GetDistance(screenBounds[i].Bounds, screenBounds[j].Bounds);
				if (dist < closestScreenDistance)
				{
					closestScreen = screenBounds[j];
					closestScreenDistance = dist;
				}
			}

			screenJunctions.Add(new Tuple<ScreenData, ScreenData>(screenBounds[i], closestScreen));
		}

		//spawn planets at the junctions
		foreach (var junction in screenJunctions)
		{
			Bounds intersection = GetIntersection(junction.Item1.Bounds, junction.Item2.Bounds);
			PlanetAndData planet = PlanetsAndConf.GetRandom();
			spawnedPlanets[junction.Item1].Add(SpawnPlanet(intersection.center, PlanetsAndConf.GetRandom(), junction.Item1));
			spawnedPlanets[junction.Item2].Add(SpawnPlanet(intersection.center, PlanetsAndConf.GetRandom(), junction.Item2));
		}

		//sort spawned planets by axis we'd like to traverse
		foreach (var pair in spawnedPlanets)
		{
			if (pair.Key.Bounds.size.y > pair.Key.Bounds.size.x) // sort by height
			{
				pair.Value.Sort(comparison: (planetA, planetB) => { return planetA.transform.position.y.CompareTo(planetB.transform.position.y); });
			}
			else //sort by width
			{
				pair.Value.Sort(comparison: (planetA, planetB) => { return planetA.transform.position.x.CompareTo(planetB.transform.position.x); });
			}
		}
	}

	Collider2D[] _planetCheckColliders = new Collider2D[8];
	public ContactFilter2D PlanetCheckFilter;

	GameObject SpawnPlanet(Vector3 startPoint, PlanetAndData planet, ScreenData data)
	{
		Vector3 point = GetPointInBounds(startPoint, data.Bounds, planet.Radius + MandatoryPlanetPadding + EdgePadding);
		point.z = 0f;
		GameObject planetSpawnedGO = PoolManager.SpawnObject(planet.Prefab, point, Quaternion.identity);
		Depenetrate(planetSpawnedGO, data.Bounds);
		return planetSpawnedGO;
	}

	void Depenetrate(GameObject planet, Bounds bounds, int iterations = 2)
	{
		Collider2D collider = planet.GetComponent<Collider2D>();

		for (int i = 0; i < iterations; i++)
		{
			int hitCount = Physics2D.OverlapCollider(collider, PlanetCheckFilter, _planetCheckColliders);
			if (hitCount > 1) //did we hit more than just ourselves?
			{
				for (int j = 0; j < hitCount; j++)
				{
					Collider2D hit = _planetCheckColliders[j];
					// Ignore our own collider.
					if (hit == collider)
						continue;


					ColliderDistance2D colliderDistance = hit.Distance(collider);

					// Ensure that we are still overlapping this collider.
					// The overlap may no longer exist due to another intersected collider
					// pushing us out of this one.
					if (colliderDistance.isOverlapped)
					{
						Vector3 offset = colliderDistance.pointA - colliderDistance.pointB;
						transform.Translate(offset + (offset.normalized * this.MandatoryPlanetPadding * 2f), Space.World);
						transform.position = GetPointInBounds(transform.position, bounds, MandatoryPlanetPadding + EdgePadding);
					}
				}
			}
			else
			{
				break;
			}
		}
	}

	Vector3 GetPointInBounds(Vector3 target, Bounds b, float padding)
	{
		Bounds shrunkBounds = new Bounds(b.center, b.size - new Vector3(padding * 2f, padding * 2f, 0));
		return shrunkBounds.ClosestPoint(target);
	}

	Bounds GetIntersection(Bounds boundsA, Bounds boundsB)
	{
		Vector3 min = Vector3.Max(boundsA.min, boundsB.min);
		Vector3 max = Vector3.Min(boundsA.max, boundsB.max);
		Bounds b = new Bounds();
		b.SetMinMax(min, max);
		return b;
	}

	float GetDistance(Bounds boundsA, Bounds boundsB)
	{
		Bounds union = new Bounds();
		Vector3 min = Vector3.Max(boundsA.min, boundsB.min);
		Vector3 max = Vector3.Min(boundsB.max, boundsB.max);
		union.SetMinMax(min, max);
		float dist = Mathf.Min(union.size.x, union.size.y);
		return dist;
	}

	/// <summary>
	/// considres flipped tuples to be duplicates
	/// </summary>
	public class TupleComparer : IEqualityComparer<Tuple<ScreenData, ScreenData>>
	{
		public bool Equals(Tuple<ScreenData, ScreenData> x, Tuple<ScreenData, ScreenData> y)
		{
			return (x.Item1 == y.Item1 && x.Item2 == y.Item2) ||
					(x.Item1 == y.Item2 && x.Item2 == y.Item1);
		}

		public int GetHashCode(Tuple<ScreenData, ScreenData> obj)
		{
			return string.Concat(new ScreenData[] { obj.Item1, obj.Item2 }.OrderBy(x => x)).GetHashCode();
			//or
			//return (string.Compare(obj.Item1, obj.Item2) < 0 ? obj.Item1 + obj.Item2 : obj.Item2 + obj.Item1).GetHashCode(); 
		}
	}
}
