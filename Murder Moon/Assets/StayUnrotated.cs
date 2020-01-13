using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StayUnrotated : MonoBehaviour
{
	private void Update()
	{
		this.transform.rotation = Quaternion.identity;
	}
}
