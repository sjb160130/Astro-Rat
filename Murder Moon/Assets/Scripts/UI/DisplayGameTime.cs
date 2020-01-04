using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DisplayGameTime : ValueObserver<float>
{
	protected override float Value { get { return Mathf.Clamp(GameState.GameTimer, 0f, float.MaxValue); } }

	protected override void OnValueChanged(float previousValue, float newValue)
	{
		if (newValue >= 11f)
			return;
		if ((newValue % 1f) > (previousValue % 1f))
			this.transform.DOPunchScale(Vector3.one * 1.2f, 0.2f);
		base.OnValueChanged(previousValue, newValue);
	}

	protected override string ConvertToDisplayText(float value)
	{
		return (value.ToString("0.0"));
	}
}