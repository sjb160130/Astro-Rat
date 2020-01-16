using UnityEngine;
using DG.Tweening;

public class GameView : MonoBehaviour
{
	[SerializeField]
	protected Cinemachine.CinemachineVirtualCamera Camera;
	[SerializeField]
	protected CanvasGroup UI;

	protected bool _isOn { get; private set; }

	private void OnValidate()
	{
		UI = UI ?? GetComponentInChildren<CanvasGroup>();
		Camera = Camera ?? GetComponentInChildren<Cinemachine.CinemachineVirtualCamera>();
	}

	public void Toggle(bool val)
	{
		_isOn = val;
		if (val)
			TurnOn();
		else
			TurnOff();
	}

	public void TurnOn()
	{
		UI.transform.localScale = Vector3.one;
		UI.DOFade(1f, 0.5f);
		UI.transform.DOScale(Vector3.one, 0.5f);
		Camera.Priority = 100;
	}

	public void TurnOff()
	{
		UI.DOFade(0f, 0.2f);
		UI.transform.DOScale(Vector3.one * 0.6f, 0.2f);
		Camera.Priority = -1;
	}
}
