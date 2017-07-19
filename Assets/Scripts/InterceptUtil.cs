using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterceptUtil {
    Vector3 targetPos, projectilePos;
    Vector3 targetVel;
    float projectileSpeed;
    public InterceptUtil() { }
    public InterceptUtil(Vector3 tp, Vector3 tv, Vector3 pp)
    {
        Init(tp, tv, pp);
    }
    public void Init(Vector3 tp, Vector3 tv, Vector3 pp)
    {
        targetPos = tp;
        targetVel = tv;
        projectilePos = pp;
    }
    public float GetTime()
    {
        var offsetPos = targetPos - projectilePos;
        var x = offsetPos.x;
        var y = offsetPos.y;
        var z = offsetPos.z;
        var u = targetVel.x;
        var v = targetVel.y;
        var w = targetVel.z;
        var x2 = x * x;
        var y2 = y * y;
        var z2 = z * z;
        var u2 = u * u;
        var v2 = v * v;
        var w2 = w * w;
        var s = projectileSpeed;
        var s2 = s * s;
        var t = (Mathf.Sqrt(Mathf.Pow(-2 * u * x - 2 * v * y - 2 * w * z, 2f)
                      - 4 * (-x2 - y2 - z2) * (s2 - u2 - v2 - w2)
                     )
                     + 2 * u * x + 2 * v * y + 2 * w * z)
                / (2 * (s2 - u2 - v2 - w2));
        return t;
    }
    //I = targetPos + t * targetVel
    public Vector3 GetInterception()
    {
        var t = GetTime();
        var I = targetPos + t * targetVel;
        return I;
    }
//we know the vector for our projectile from:
//projectileVel = (I - ProjectilePos)/t
    public Vector3 GetProjectileVelocity()
    {
        var t = GetTime();
        var I = targetPos + t * targetVel;
        return (I - projectilePos) / t;
    }
}