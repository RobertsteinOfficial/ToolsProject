using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class BezierMath
{
    public static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        t = Mathf.Clamp01(t);
        float oneMinusT = 1f - t;

        return oneMinusT * oneMinusT * p0 + 2f * oneMinusT * t * p1 + t * t * p2;
    }

    public static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        t = Mathf.Clamp01(t);
        float oneMinusT = 1f - t;

        return oneMinusT * oneMinusT * oneMinusT * p0 +
            3f * oneMinusT * oneMinusT * t * p1 +
            3f * oneMinusT * t * t * p2 +
            t * t * t * p3;
    }

    public static Vector3 GetPoint(List<Vector3> points, float t)
    {
        Vector3 point = new Vector3();

        for (int i = 0; i < points.Count; i++)
        {
            double barnstein = Barnstein(points.Count - 1, i, t);
            point.x += (float)barnstein * points[i].x;
            point.y += (float)barnstein * points[i].y;
            point.z += (float)barnstein * points[i].z;
        }

        return point;
    }

    public static Vector3 GetFirstDerivative(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        return 2f * (1f - t) * (p1 - p0) + 2f * t * (p2 - p1);
    }

    public static Vector3 GetFirstDerivative(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float oneMinusT = 1f - t;

        return 3f * oneMinusT * oneMinusT * (p1 - p0) +
            6f * oneMinusT * t * (p2 - p1) +
            3f * t * t * (p3 - p2);
    }

    public static ulong Factorial(int n)
    {
        ulong res = 1;

        for (int i = 1; i <= n; i++)
        {
            res *= (ulong)i;
        }

        return res;
    }

    private static double Binomial(int n, int i)
    {
        //double nF = Factorial(n);
        //double iF = Factorial(i);
        //double niF = Factorial(n - i);

        //return nF / (iF * niF);


        long r = 1;
        long d;
        if (i > n) return 0;
        for (d = 1; d <= i; d++)
        {
            r *= n--;
            r /= d;
        }
        return r;

    }

    private static double Barnstein(int n, int i, float t)
    {
        float ti = Mathf.Pow(t, i);
        float oneMinusTni = Mathf.Pow(1 - t, n - i);

        return Binomial(n, i) * ti * oneMinusTni;
    }
}
