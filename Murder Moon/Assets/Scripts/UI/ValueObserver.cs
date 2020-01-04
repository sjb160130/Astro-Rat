using System;
using UnityEngine;

public abstract class ValueObserver<T> : MonoBehaviour where T : IComparable
{

	public TMPro.TextMeshProUGUI Text;

	T lastDisplayedScore = default(T);

	protected abstract T Value { get; }

	protected virtual void OnValueChanged(T previousValue, T newValue) { }

	protected virtual string ConvertToDisplayText(T value)
	{
		return "<sprite=0> " + value.ToString();
	}

	public void Update()
	{
		T value = Value;
		if (lastDisplayedScore == null)
			return;
		if (lastDisplayedScore.CompareTo(Value) != 0)
		{
			Text.text = ConvertToDisplayText(value);
			OnValueChanged(lastDisplayedScore, value);
			lastDisplayedScore = value;
		}
	}
}
