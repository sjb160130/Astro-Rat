using UnityEngine;
using System.Collections;

public class ParallaxLayer : MonoBehaviour
{
	public float speedX;
	public float speedY;
	public bool moveInOppositeDirection;

	private Transform cameraTransform;
	private Vector3 previousCameraPosition;
	private bool previousMoveParallax;
	public ParallaxOption Options;

	float _zoomTileAmount;
	float _zoomSpeed01 = 1f;

	Vector3 _pos;
	public float RepeatHeight = 21.5f;
	public float AutoscrollSpeed = 10f;

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
		_pos = this.transform.position;
	}

	void Update()
	{
		if (Options.moveParallax && !previousMoveParallax)
			previousCameraPosition = cameraTransform.position;

		previousMoveParallax = Options.moveParallax;

		if (!Application.isPlaying && !Options.moveParallax)
			return;

		_zoomSpeed01 = Mathf.MoveTowards(_zoomSpeed01, GameState.Instance.IsAtStartScreen ? 1f : 0f, Time.deltaTime);
		float tZoomDelta = Time.deltaTime * AutoscrollSpeed * (1f - this.speedY) * Mathf.SmoothStep(0f, 1f, _zoomSpeed01);
		_zoomTileAmount += tZoomDelta;

		Vector3 distance = cameraTransform.position - previousCameraPosition;
		float direction = (moveInOppositeDirection) ? -1f : 1f;
		_pos = _pos + Vector3.Scale(distance, new Vector3(speedX, speedY)) * direction;
		transform.position = _pos + new Vector3(0f, _zoomTileAmount % RepeatHeight, 0f);

		previousCameraPosition = cameraTransform.position;
	}
}
