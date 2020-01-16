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

	public class SpawnedPlanet
	{

	}

	private void OnValidate()
	{
		if (PlanetsAndConf == null)
			return;
		foreach (PlanetAndData planetData in PlanetsAndConf)
		{
			if (planetData == null || planetData.Prefab == null)
				continue;
			var renderer = planetData.Prefab.GetComponentInChildren<Renderer>(); ;
			var bounds = renderer.bounds;
			planetData.Radius = Mathf.Max(bounds.extents.x, bounds.extents.y);
		}
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
		Dictionary<ScreenData, List<SpawnedPlanet>> spawnedPlanets = new Dictionary<ScreenData, List<SpawnedPlanet>>();

		for (int i = 0; i < screenBounds.Length; i++)
		{
			spawnedPlanets.Add(screenBounds[i], new List<SpawnedPlanet>());

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

		foreach (var junction in screenJunctions)
		{
			Bounds intersection = GetIntersection(junction.Item1.Bounds, junction.Item2.Bounds);
			GameObject intersectionGO = new GameObject("Intersection");
			intersectionGO.transform.position = intersection.center;
		}
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
