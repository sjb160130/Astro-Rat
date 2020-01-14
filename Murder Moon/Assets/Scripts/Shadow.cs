using PKG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shadow : MonoBehaviour
{
	public float Offset = 0f;
	public GameObject ShadowPrefab;

	public float MaxDistance = 3f;
	public float MinScale = 0.5f;
	public float MinAlpha = 0.4f;

	GameObject _shadow;
	static GameObject _container;

	Vector3 _originalScale;
	float _originalAlpha;
	SpriteRenderer _renderer;

	private void Awake()
	{
		_originalScale = this.ShadowPrefab.transform.localScale;
	}

	private void OnEnable()
	{
		if (_container == null)
			_container = new GameObject("Shadows");

		_shadow = PoolManager.SpawnObject(ShadowPrefab);
		_shadow.transform.SetParent(_container.transform);
		if (_renderer == null)
		{
			_renderer = _shadow.GetComponent<SpriteRenderer>();
			_originalAlpha = _renderer.color.a;
		}

	}

	private void OnDisable()
	{
		PoolManager.ReleaseObject(this._shadow);
		_renderer.color = new Color(_renderer.color.r, _renderer.color.g, _renderer.color.b, _originalAlpha);
		_renderer = null;
	}

	private void Update()
	{
		Vector3 point = Planet.GetClosestPointOnPlanet(this.transform.position, this.Offset);
		_shadow.transform.position = point;
		Vector3 up = this.transform.position - point;
		float dist = up.magnitude;
		_shadow.transform.rotation = Quaternion.LookRotation(Vector3.forward, up);

		float distT = Mathf.Clamp01(dist / MaxDistance);
		_renderer.color = new Color(_renderer.color.r, _renderer.color.g, _renderer.color.b, Mathf.Lerp(_originalAlpha, MinAlpha * _originalAlpha, distT));
		_shadow.transform.localScale = Vector3.Lerp(_originalScale, _originalScale * MinScale, distT);
	}
}
