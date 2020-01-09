﻿using System.Collections;
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

	public float LerpSpeed = 5f;
	public float NoiseMagnitudeX = 2f;
	public float NoiseSpeedX = 1f;
	public float NoiseMagnitudeY = 1f;
	public float NoiseSpeedY = 1f;

	Vector3 _targetPosition;

	public Animator Animator;

	private void Awake()
	{
		_inPosition = this.transform.position;
		_targetPosition = this.transform.position = OutPosition.position;
	}

	bool _isIn;

	private void Update()
	{
		bool shouldBeIn = PlayerManager.Instance.IsPlaying(this.PlayerID);
		if (shouldBeIn != _isIn)
		{
			_targetPosition = shouldBeIn ? _inPosition : OutPosition.position;
			_isIn = shouldBeIn;
		}

		if (PlayerManager.Instance.IsReady(this.PlayerID))
		{
			//TODO: optimize state hashing as a cached int value
			this.Animator?.Play("Ready");
		}
		else
		{
			this.Animator?.Play("Not Ready");
		}

		float noiseX = Mathf.PerlinNoise(this.transform.position.x, this.transform.position.y + (Time.time * NoiseSpeedX)) * this.NoiseMagnitudeX;
		float noiseY = Mathf.PerlinNoise(this.transform.position.x + (Time.time * NoiseSpeedY), this.transform.position.y) * this.NoiseMagnitudeY;
		Vector3 target = _targetPosition + new Vector3(noiseX, noiseY, 0f) - new Vector3(-0.5f, -0.5f, 0f);
		this.transform.position = Vector3.Lerp(this.transform.position, target, Time.deltaTime * LerpSpeed);
	}
}