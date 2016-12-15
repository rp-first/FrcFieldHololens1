//    Copyright 2016 United States Government as represented by the
//    Administrator of the National Aeronautics and Space Administration.
//    All Rights Reserved.
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//      http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.



using UnityEngine;
using System.Collections;

public class MathUtil {

    /** Returns distances between the points passed to it.  
     * For 2 or fewer points, the function returns a "false" 
     * loop back to the starting point*/
	public static float[] getDistances(Vector3[] points)
    {
        float[] distances = new float[points.Length];
        for(int i = 0; i < distances.Length; i++)
        {
            distances[i] = Vector3.Distance(points[i], points[(i + 1) % points.Length]);
        }
        return distances;
    }

    public static float[] getDistances(Transform[] points)
    {
        float[] distances = new float[points.Length];
        for (int i = 0; i < distances.Length; i++)
        {
            distances[i] = Vector3.Distance(points[i].position, points[(i + 1) % points.Length].position);
        }
        return distances;
    }

    public static Vector3 getCenter(Vector3[] points)
    {
        if (points.Length == 0)
            return Vector3.zero;

        Vector3 center = Vector3.zero;
        foreach (Vector3 point in points)
            center += point;

        center /= points.Length;
        return center;
    }

    public static Vector3 getCenter(Transform[] points)
    {
        if (points.Length == 0)
            return Vector3.zero;

        Vector3 center = Vector3.zero;
        foreach (Transform point in points)
            center += point.position;

        center /= points.Length;
        return center;
    }

    public static Vector3 getNormal(Vector3 point1, Vector3 point2, Vector3 point3)
    {
        Vector3 U = point2 - point1;
        Vector3 V = point3 - point1;

        return Vector3.Cross(U, V);
    }

    public static Vector3 getNormal(Transform point1, Transform point2, Transform point3)
    {
        Vector3 U = point2.position - point1.position;
        Vector3 V = point3.position - point1.position;

        return Vector3.Cross(U, V);
    }

    public static Vector3 getAverageNormal(Vector3[] points)
    {
        Vector3 normal = Vector3.zero;
        for (int i =2; i < points.Length; i++)
        {
            normal += getNormal(points[i - 2], points[i - 1], points[i]);
        }

        return normal.normalized;
    }

    public static Vector3 getAverageNormal(Transform[] points)
    {
        Vector3 normal = Vector3.zero;
        for (int i = 2; i < points.Length; i++)
        {
            normal += getNormal(points[i - 2], points[i - 1], points[i]);
        }

        return normal.normalized;
    }

    //Get an average (mean) from more then two quaternions (with two, slerp would be used).
    //Note: this only works if all the quaternions are relatively close together.
    //Usage: 
    //-Cumulative is an external Vector4 which holds all the added x y z and w components.
    //-newRotation is the next rotation to be added to the average pool
    //-firstRotation is the first quaternion of the array to be averaged
    //-addAmount holds the total amount of quaternions which are currently added
    //This function returns the current average quaternion
    public static Quaternion AverageQuaternion(ref Vector4 cumulative, Quaternion newRotation, Quaternion firstRotation, int addAmount)
    {

        float w = 0.0f;
        float x = 0.0f;
        float y = 0.0f;
        float z = 0.0f;

        //Before we add the new rotation to the average (mean), we have to check whether the quaternion has to be inverted. Because
        //q and -q are the same rotation, but cannot be averaged, we have to make sure they are all the same.
        if (!AreQuaternionsClose(newRotation, firstRotation))
        {

            newRotation = InverseSignQuaternion(newRotation);
        }

        //Average the values
        float addDet = 1f / (float)addAmount;
        cumulative.w += newRotation.w;
        w = cumulative.w * addDet;
        cumulative.x += newRotation.x;
        x = cumulative.x * addDet;
        cumulative.y += newRotation.y;
        y = cumulative.y * addDet;
        cumulative.z += newRotation.z;
        z = cumulative.z * addDet;

        //note: if speed is an issue, you can skip the normalization step
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

    //Changes the sign of the quaternion components. This is not the same as the inverse.
    public static Quaternion InverseSignQuaternion(Quaternion q)
    {

        return new Quaternion(-q.x, -q.y, -q.z, -q.w);
    }

    //Returns true if the two input quaternions are close to each other. This can
    //be used to check whether or not one of two quaternions which are supposed to
    //be very similar but has its component signs reversed (q has the same rotation as
    //-q)
    public static bool AreQuaternionsClose(Quaternion q1, Quaternion q2)
    {

        float dot = Quaternion.Dot(q1, q2);

        if (dot < 0.0f)
        {

            return false;
        }

        else
        {

            return true;
        }
    }
}
