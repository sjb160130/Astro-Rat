using System.Collections;
using System.Collections.Generic;
using DG.Tweening;



public class WinnerCam : GameView
{
	private void Update()
	{
		RatPlayer winner = RatPlayer.FindCurrentWinner();
		if (winner == null)
			return;
		bool lookAtNull = this.Camera.LookAt == null;
		this.Camera.LookAt = GameState.Winner.transform;
	}
}
