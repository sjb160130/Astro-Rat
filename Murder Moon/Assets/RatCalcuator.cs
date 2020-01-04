using UnityEngine;
using System;
using System.Collections.Generic;

public class RatCalcuator : MonoBehaviour
{
    public Vector2 planetPoint = new Vector2(0,0);

    public Vector2 me;

    public Vector2 calcLeft;
    public Vector2 calcRight;
    public Vector2 calcDown;
    public Vector2 calcUp;

    public void Update()
    {
        me = transform.position;

        var holder = planetPoint + me;
        Debug.Log(holder);

        calcDown = RoundVector(holder).normalized;
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

