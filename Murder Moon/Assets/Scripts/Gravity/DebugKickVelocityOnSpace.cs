using UnityEngine;

public class DebugKickVelocityOnSpace : MonoBehaviour
{
	public float Intensity = 10f;

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			this.GetComponent<Rigidbody2D>()?.AddForce(Random.onUnitSphere * Intensity, ForceMode2D.Impulse);
		}
	}
}