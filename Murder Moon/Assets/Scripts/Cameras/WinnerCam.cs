using System.Collections;
using System.Collections.Generic;
using DG.Tweening;



public class WinnerCam : GameView
{
	private void Update()
	{
		RatPlayer winner = GameState.Winner;
		if (winner == null)
			return;
		bool lookAtNull = this.Camera.LookAt == null;
		this.Camera.Follow = winner.transform;
	}
}
