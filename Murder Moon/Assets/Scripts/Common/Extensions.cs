using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
	public static Transform Reset(this Transform transform)
	{
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		transform.localScale = Vector3.one;
		return transform;
	}

	public static Vector3 TransformPointUnscaled(this Transform transform, Vector3 position)
	{
		var localToWorldMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
		return localToWorldMatrix.MultiplyPoint3x4(position);
	}

	public static Vector3 InverseTransformPointUnscaled(this Transform transform, Vector3 position)
	{
		var worldToLocalMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one).inverse;
		return worldToLocalMatrix.MultiplyPoint3x4(position);
	}


	public static T GetCreateComponent<T>(this MonoBehaviour c) where T : UnityEngine.Component
	{
		return c.gameObject.GetCreateComponent<T>();
	}

	public static T GetCreateComponent<T>(this GameObject go) where T : UnityEngine.Component
	{
		T comp = go.GetComponent<T>();
		if (comp == null)
			comp = go.AddComponent<T>();
		return comp;
	}

	public static T GetCreateComponent<T>(this Transform t) where T : UnityEngine.Component
	{
		return t.gameObject.GetCreateComponent<T>();
	}
	public static T GetCreateComponent<T>(this Component t) where T : UnityEngine.Component
	{
		return t.gameObject.GetCreateComponent<T>();
	}

	public static T GetRandom<T>(this List<T> list)
	{
		if (list == null || list.Count <= 0)
		{
			return default(T);
		}
		return list[Random.Range(0, list.Count)];
	}

	public static T GetRandom<T>(this T[] list)
	{
		if (list == null || list.Length <= 0)
		{
			return default(T);
		}
		return list[Random.Range(0, list.Length)];
	}

	public static void ZeroAll(this Transform t)
	{
		t.localPosition = Vector3.zero;
		t.localRotation = Quaternion.identity;
		t.localScale = Vector3.one;
	}

	public static void ZeroPosRot(this Transform t)
	{
		t.localPosition = Vector3.zero;
		t.localRotation = Quaternion.identity;
	}
}
