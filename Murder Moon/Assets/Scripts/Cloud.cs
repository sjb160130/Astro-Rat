using PKG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cloud : MonoBehaviour
{
	static GameObject _prefab;

	public enum Size
	{
		VerySmall,
		Small,
		Large
	}

	static public void Spawn(Vector3 point, Size size)
	{
		Vector3 surfacePoint = Planet.GetClosestPointOnPlanet(point, -1f);
		Vector3 up = point - surfacePoint;
		up.z = 0f;
		Spawn(point, Quaternion.LookRotation(Vector3.forward, up * -1f), size);
	}

	static public void Spawn(Vector3 point, Quaternion rot, Size size)
	{
		if (_prefab == null)
			_prefab = Resources.Load<GameObject>("Cloud");
		GameObject spawned = PoolManager.SpawnObject(_prefab, point, rot);
		switch (size)
		{
			case Size.VerySmall:
				spawned.transform.localScale = Vector3.one * 0.35f;
				break;
			case Size.Small:
				spawned.transform.localScale = Vector3.one * 0.5f;
				break;
			case Size.Large:
				spawned.transform.localScale = Vector3.one;
				break;
			default:
				break;
		}
	}

	public GameObject[] CloudSprites;

	IEnumerator Start()
	{

		GameObject selection = CloudSprites.GetRandom();
		foreach (var cloud in CloudSprites)
		{
			cloud.SetActive(cloud == selection);
		}

		yield return new WaitForSeconds(1f);
		PoolManager.ReleaseObject(this.gameObject);
	}
}
