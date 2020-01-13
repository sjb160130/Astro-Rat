using PKG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shadow : MonoBehaviour
{
	public float Offset = 0f;
	public GameObject ShadowPrefab;

	GameObject _shadow;
	static GameObject _container;

	private void OnEnable()
	{
		if (_container == null)
			_container = new GameObject("Shadows");

		_shadow = PoolManager.SpawnObject(ShadowPrefab);
		_shadow.transform.SetParent(_container.transform);
	}

	private void OnDisable()
	{
		PoolManager.ReleaseObject(this._shadow);
	}

	private void Update()
	{
		Vector3 point = Planet.GetClosestPointOnPlanet(this.transform.position, this.Offset);
		_shadow.transform.position = point;
		Vector3 up = this.transform.position - point;
		_shadow.transform.rotation = Quaternion.LookRotation(Vector3.forward, up);
	}
}
