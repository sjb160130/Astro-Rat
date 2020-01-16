using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class MonitorManager : GameView
{
	public static MonitorManager Instance { get; private set; }

	public RectTransform MonitorBounds;
	public RectTransform[] MonitorRects;
	public Transform[] Cameras;

	public float MoveSpeed = 2f;

	int _selectedScreen;

	public int EditorScreenCount = 2;

	public float Scale = 1f;

	public GameObject CameraCoverPrefab;

	List<CanvasGroup> _cgs = new List<CanvasGroup>();

	int DisplayCount
	{
		get
		{
#if UNITY_EDITOR
			return EditorScreenCount;
#else
			return Display.displays.Length;
#endif
		}
	}

	private void Awake()
	{
		Instance = this;

		for (int i = 0; i < Display.displays.Length; i++)
		{
			Display.displays[i].Activate();
		}

		for (int i = 1; i < DisplayCount; i++) //skip the first main camera
		{
			GameObject coverGO = GameObject.Instantiate(CameraCoverPrefab);
			Canvas coverCanvas = coverGO.GetComponent<Canvas>();
			CanvasGroup coverCG = coverGO.GetCreateComponent<CanvasGroup>();
			_cgs.Add(coverCG);
			coverCanvas.targetDisplay = i;
			coverCG.alpha = 1f;
		}
	}

	bool GetAnyDown(string action)
	{
		foreach (var p in Rewired.ReInput.players.Players)
		{
			if (p.GetButtonDown(action))
				return true;
		}
		return false;
	}

	Vector2 GetMoveInput()
	{
		Vector2 input = Vector2.zero;
		foreach (var p in Rewired.ReInput.players.Players)
		{
			input += p.GetAxis2D("Move X", "Move Y");
		}
		return Vector2.ClampMagnitude(input, 1f);
	}

	Vector3[] boundsCorners = new Vector3[4];
	Vector3[] monitorCorners = new Vector3[4];
	const int BottomLeft = 0, TopLeft = 1, TopRight = 2, BottomRight = 3;

	float GetAspect(int index)
	{
		var cam = Cameras[index].GetComponent<Camera>();
		if (cam != null)
			return cam.aspect;
		return UnityEngine.Camera.main.aspect;
	}

	float GetOrthoSize(int index)
	{
		var cam = Cameras[index].GetComponent<Camera>();
		if (cam != null)
			return cam.orthographicSize;
		return UnityEngine.Camera.main.orthographicSize;
	}

	private void Update()
	{
		if (_isOn)
		{
			for (int i = 0; i < MonitorRects.Length; i++)
			{
				MonitorRects[i].gameObject.SetActive(DisplayCount > i);
			}

			if (GetAnyDown("Join Game"))
				_selectedScreen += 1;

			if (_selectedScreen < 0)
				_selectedScreen = 0;
			else if (DisplayCount <= _selectedScreen)
				_selectedScreen = 0;

			Vector2 input = GetMoveInput();
			RectTransform selectedRT = this.MonitorRects[_selectedScreen];
			selectedRT.position += (Vector3)input * MoveSpeed * Time.deltaTime;

			MonitorBounds.GetWorldCorners(boundsCorners);

			for (int i = 0; i < DisplayCount; i++)
			{
				RectTransform rt = this.MonitorRects[i];
				float aspect = GetAspect(i);
				float orhtoSizeScaled = GetOrthoSize(i) * Scale;
				rt.sizeDelta = new Vector2(orhtoSizeScaled * aspect, orhtoSizeScaled);
				//rt.GetWorldCorners(monitorCorners);
				rt.position = new Vector3(
					Mathf.Clamp(rt.transform.position.x, boundsCorners[BottomLeft].x, boundsCorners[TopRight].x),
					Mathf.Clamp(rt.transform.position.y, boundsCorners[BottomLeft].y, boundsCorners[TopRight].y),
					0f);
			}
		}

		foreach (var cg in _cgs)
		{
			cg.alpha = Mathf.MoveTowards(cg.alpha, GameState.Instance.IsPlaying ? 0f : 1f, Time.unscaledDeltaTime);
		}
	}
}
