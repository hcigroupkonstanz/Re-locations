using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MathUtility : MonoBehaviour
{
    public static Vector3 Average(IEnumerable<Vector3> vectors)
    {
        var avg = Vector3.zero;
        foreach (var vec in vectors)
        {
            avg += vec;
        }
        return avg / (Mathf.Max(vectors.Count(), 1));
    }

    // Based on: https://forum.unity.com/threads/average-quaternions.86898/
    public static Quaternion Average(IEnumerable<Quaternion> quats, bool quaternionsSimilar = false)
    {
        var count = quats.Count();
        if (count == 0)
            return Quaternion.identity;
        if (count == 1)
            return quats.ElementAt(0);
        if (count == 2)
            return Quaternion.Slerp(quats.ElementAt(0), quats.ElementAt(1), 0.5f);

        if (quaternionsSimilar)
        {
            var cumulative = Vector4.zero;
            var avg = Quaternion.identity;
            var firstQuat = Quaternion.identity;
            var addAmount = 0;

            foreach (var q in quats)
            {
                addAmount++;
                avg = AverageQuaternion(ref cumulative, q, firstQuat, addAmount);
            }

            return avg;
        }
        else
        {
            Quaternion qAvg = quats.First();
            int i = 1;

            foreach (var q in quats.Skip(1))
            {
                var weight = 1.0f / (float)(i + 1);
                qAvg = Quaternion.Slerp(qAvg, q, weight);
                i++;
            }
            return qAvg;
        }
    }

    public static Quaternion AverageQuaternion(ref Vector4 cumulative, Quaternion newRotation, Quaternion firstRotation, int addAmount)
    {
        float w = 0.0f;
        float x = 0.0f;
        float y = 0.0f;
        float z = 0.0f;

        // Before we add the new rotation to the average (mean), we have to check whether the quaternion has to be inverted. Because
        // q and -q are the same rotation, but cannot be averaged, we have to make sure they are all the same.
        if (!AreQuaternionsClose(newRotation, firstRotation))
            newRotation = InverseSignQuaternion(newRotation);

        // Average the values
        float addDet = 1f / (float)addAmount;
        cumulative.w += newRotation.w;
        w = cumulative.w * addDet;
        cumulative.x += newRotation.x;
        x = cumulative.x * addDet;
        cumulative.y += newRotation.y;
        y = cumulative.y * addDet;
        cumulative.z += newRotation.z;
        z = cumulative.z * addDet;

        // note: if speed is an issue, you can skip the normalization step
        return NormalizeQuaternion(x, y, z, w);
    }

    public static Quaternion NormalizeQuaternion(float x, float y, float z, float w)
    {
        float lengthD = 1.0f / (w * w + x * x + y * y + z * z);
        w *= lengthD;
        x *= lengthD;
        y *= lengthD;
        z *= lengthD;

        return new Quaternion(x, y, z, w);
    }

    // Changes the sign of the quaternion components. This is not the same as the inverse.
    public static Quaternion InverseSignQuaternion(Quaternion q)
    {
        return new Quaternion(-q.x, -q.y, -q.z, -q.w);
    }

    // Returns true if the two input quaternions are close to each other. This can
    // be used to check whether or not one of two quaternions which are supposed to
    // be very similar but has its component signs reversed (q has the same rotation as -q)
    public static bool AreQuaternionsClose(Quaternion q1, Quaternion q2)
    {
        float dot = Quaternion.Dot(q1, q2);

        if (dot < 0.0f)
            return false;
        else
            return true;
    }

}
