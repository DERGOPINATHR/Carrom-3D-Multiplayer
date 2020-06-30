using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrigerRayCast : MonoBehaviour
{
    public static LineRenderer LineRenderer;
    // Start is called before the first frame update
    void Start()
    {
        LineRenderer = GetComponent<LineRenderer>();
        //LineRenderer.useWorldSpace = false;
    }

    public static void castLine(Vector3 Origin, Vector3 End)
    {
        RaycastHit hit;
        LineRenderer.transform.position = Origin;
        End.y = 6f;
        LineRenderer.SetPosition(0, Origin);
        
        if (Physics.Raycast(Origin, End, out hit, Mathf.Infinity))
        {
            LineRenderer.SetPosition(1, hit.point);
        }
        else
        {
            LineRenderer.SetPosition(1, End);
        }
        
    }

    public static void toggleLine(bool toggle)
    {
        LineRenderer.enabled = toggle;
    }

    public static void drawCircle(Vector3 Origin, float radius)
    {
        LineRenderer.transform.position = Origin;
        LineRenderer.useWorldSpace = false;
        LineRenderer.startWidth = 0.2F;
        LineRenderer.endWidth = 0.2F;
        LineRenderer.positionCount = 0;
        for (double theta = 0; theta < 360; theta++)
        {
            double radian_theta = theta * Mathf.Deg2Rad;
            double double_x = radius * System.Math.Cos(radian_theta);
            double double_y = radius * System.Math.Sin(radian_theta);
            float float_X = float.Parse(double_x.ToString());
            float float_Z = float.Parse(double_y.ToString());
            Vector3 pos = new Vector3(float_X, 1, float_Z - radius);
            LineRenderer.positionCount = (int)theta + 1;
            LineRenderer.SetPosition((int)theta, pos);
        }
    }

    public static Vector3 GetLocation(double theta, float radius)
    {
        float x = radius * (float)Math.Cos(theta * Mathf.Deg2Rad);
        float y = radius * (float)Math.Sin(theta * Mathf.Deg2Rad);
        return new Vector3(x, 0, y);
    }

    public static double GetAngle(float x, float y, float radius)
    {
        double theta = (Math.Acos(x) / radius);
        if (theta == double.NaN)
        {
            theta = (Math.Asin(y) / radius);
        }
        return theta;
    }
    // Update is called once per frame
    void Update()
    {

    }
}
