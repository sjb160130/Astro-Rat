using UnityEngine;

public class DebugClosestPoint : MonoBehaviour
{
	public float Offset = 5f;
	private void OnDrawGizmosSelected()
	{
		if (Application.isPlaying)
			Gizmos.DrawWireSphere(Planet.GetClosestPointOnPlanet(this.transform.position, Offset), 1f);
	}
}
