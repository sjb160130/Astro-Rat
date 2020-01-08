using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DisplayPlayerTapIn : MonoBehaviour
{
	public int PlayerID;
	public Transform OutPosition;
	Vector3 _inPosition;

	public Ease EaseType = Ease.OutElastic;
	public float EaseDuration = 1.5f;

	private void Awake()
	{
		_inPosition = this.transform.position;
		this.transform.position = OutPosition.position;
	}

	bool _isIn;

	private void Update()
	{
		bool shouldBeIn = PlayerManager.Instance.IsPlaying(this.PlayerID);
		if (shouldBeIn != _isIn)
		{
			Vector3 target = shouldBeIn ? _inPosition : OutPosition.position;
			this.transform.DOMove(target, EaseDuration).SetEase(EaseType);
			_isIn = shouldBeIn;
		}
	}
}
