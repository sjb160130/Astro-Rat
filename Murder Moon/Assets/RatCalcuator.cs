using UnityEngine;
using System;
using System.Collections.Generic;

public class RatCalcuator : MonoBehaviour
{
    public Vector2 planetPoint;

    public Vector2 me;

    public Vector2 calcLeft;
    public Vector2 calcRight;
    public Vector2 calcDown;
    public Vector2 calcUp;

    public void Awake()
    {
        me = transform.position;
    }

    public void Update()
    {
        me = transform.position;

        var holder = planetPoint - me;
        Debug.Log(holder);

        calcDown = RoundVector(holder);
        calcLeft = RoundVector(Vector2.Perpendicular(holder));
        calcRight = calcLeft * -1;
    }


    private Vector2 RoundVector(Vector2 v)
    {
        float x = (float) Math.Round(v.x, 2);
        float y = (float) Math.Round(v.y, 2);
        return new Vector2(x, y);
    }
}

