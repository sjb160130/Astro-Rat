using UnityEngine;
using System;

public class RatCalcuator : MonoBehaviour
{
    public Vector2 planetPoint = new Vector2(0,0);

    public Vector2 me;

    private Vector2 calcLeft;
    private Vector2 calcRight;
    private Vector2 calcDown;
    private Vector2 calcUp;

    public void Update()
    {
        me = transform.position;
        var holder = planetPoint - me;

        calcDown = holder;
        calcUp = calcDown * -1;
        calcLeft = RoundVector(Vector2.Perpendicular(holder));
        calcRight = calcLeft * -1;
    }


    public Vector2 GetLeft()
    {
        return calcLeft;
    }

    public Vector2 GetRight()
    {
        return calcRight;
    }

    public Vector2 GetDown()
    {
        return calcDown;
    }

    public Vector2 GetUp()
    {
        return calcUp;
    }

    private Vector2 RoundVector(Vector2 v)
    {
        float x = (float) Math.Round(v.x, 2);
        float y = (float) Math.Round(v.y, 2);
        return new Vector2(x, y).normalized;
    }
}

