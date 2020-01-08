using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class ParallaxLayer : MonoBehaviour
{
	public float speedX;
	public float speedY;
	public bool moveInOppositeDirection;

	private Transform cameraTransform;
	private Vector3 previousCameraPosition;
	private bool previousMoveParallax;
	public ParallaxOption Options;

	private void OnValidate()
	{
		GameObject gameCamera = Camera.main.gameObject;
		Options = Options ?? gameCamera.GetComponent<ParallaxOption>();
	}

	void OnEnable()
	{
		GameObject gameCamera = Camera.main.gameObject;
		cameraTransform = gameCamera.transform;
		previousCameraPosition = cameraTransform.position;
	}

	void Update()
	{
		if (Options.moveParallax && !previousMoveParallax)
			previousCameraPosition = cameraTransform.position;

		previousMoveParallax = Options.moveParallax;

		if (!Application.isPlaying && !Options.moveParallax)
			return;

		Vector3 distance = cameraTransform.position - previousCameraPosition;
		float direction = (moveInOppositeDirection) ? -1f : 1f;
		transform.position += Vector3.Scale(distance, new Vector3(speedX, speedY)) * direction;

		previousCameraPosition = cameraTransform.position;
	}
}
