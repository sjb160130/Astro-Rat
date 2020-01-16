using PKG;
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

	public PlanetAndData[] PlanetsAndConf;

	private void OnValidate()
	{
		foreach (PlanetAndData planetData in PlanetsAndConf)
		{
			if (planetData == null || planetData.Prefab == null)
				continue;
			var renderer = planetData.Prefab.GetComponentInChildren<Renderer>(); ;
			var bounds = renderer.bounds;
			planetData.Radius = Mathf.Max(bounds.extents.x, bounds.extents.y);
		}
	}

	public void Build(Bounds[] screenBounds)
	{
		foreach (var go in GameObject.FindGameObjectsWithTag("Planet"))
		{
			PoolManager.ReleaseObject(go);
		}

		foreach (var bounds in screenBounds)
		{
			Build(screenBounds);
		}
	}

	public void Build(Bounds screenBounds)
	{
		if (screenBounds.size.x > screenBounds.size.y)
		{
			// Build horizontally
		}
		else
		{
			// Build vertically
		}
	}
}
