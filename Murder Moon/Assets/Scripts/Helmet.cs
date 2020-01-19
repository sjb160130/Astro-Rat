using PKG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Helmet : MonoBehaviour
{
	static GameObject _prefab;

	static public void Spawn(Vector3 point)
	{
		Vector3 surfacePoint = Planet.GetClosestPointOnPlanet(point, -1f);
		Vector3 up = point - surfacePoint;
		up.z = 0f;
		Spawn(point, Quaternion.LookRotation(Vector3.forward, up * -1f));
	}

	static public void Spawn(Vector3 point, Quaternion rot)
	{
		if (_prefab == null)
			_prefab = Resources.Load<GameObject>("Helmet");
		GameObject spawned = PoolManager.SpawnObject(_prefab, point, rot);
		Helmet helmet = spawned.GetComponent<Helmet>();
		helmet.StartCoroutine(helmet.Run());
	}

	public Rigidbody2D[] HelmetChunks;
	Vector3[] localPositions;

	private void Awake()
	{
		localPositions = new Vector3[HelmetChunks.Length];
		for (int i = 0; i < HelmetChunks.Length; i++)
		{
			localPositions[i] = HelmetChunks[i].transform.localPosition;
		}
	}

	const float MinForce = 1f;
	const float MaxForce = 6f;

	IEnumerator Run()
	{
		for (int i = 0; i < HelmetChunks.Length; i++)
		{
			HelmetChunks[i].transform.localPosition = localPositions[i];
			HelmetChunks[i].transform.localScale = Vector3.one;
			Vector2 force = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized * Random.Range(MinForce, MaxForce);
			HelmetChunks[i].velocity = Vector2.zero;
			HelmetChunks[i].AddForce(force, ForceMode2D.Impulse);
		}

		yield return null;

		for (float t = 0; t < 1f; t += Time.deltaTime / 2f)
		{
			float tMod = 1f - (t * t);
			Vector3 scale = tMod * Vector3.one;
			for (int i = 0; i < HelmetChunks.Length; i++)
			{
				HelmetChunks[i].transform.localScale = scale;
			}
			yield return null;
		}

		PoolManager.ReleaseObject(this.gameObject);
	}
}
