using UnityEngine;
using System;

public class RatCalculator : MonoBehaviour
{
    private Vector2 calcLeft;
    private Vector2 calcRight; 
    private Vector2 calcDown;
    private Vector2 calcUp;

    public void UpdateDirections()
    {
		Planet p;
		var holder = Planet.GetSimpleGravityDirection(GetComponent<Collider2D>(), out p);
		holder.Normalize();

        calcDown = holder;
        calcUp = calcDown * -1;
        calcLeft = (Vector2.Perpendicular(holder));
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
}

