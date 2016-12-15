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



//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;

//public class MarkerlessController : MonoBehaviour {

//    public TrackableLocation location;
//    public float delta = 0.1f;
//	// Use this for initialization
//	void Start () {
//        StartCoroutine(locate(location));

//    }
	
//	// Update is called once per frame
//	void Update () {
	
//	}

//    IEnumerator locate(TrackableLocation location)
//    {
//        float[] distances, originalDistances;
//        Vector3[] points = new Vector3[location.TrackingPoints.Count]; 
//         for (int i = 0; i < points.Length; )
//        {
//            if (Input.GetMouseButtonUp(0))
//            {
//                //raycast from click
//                Ray mousePoint = Camera.main.ScreenPointToRay(Input.mousePosition);
//                RaycastHit hit;
//                if (Physics.Raycast(mousePoint, out hit, 10))
//                {
//                    print("hit[" + i+"] = " + hit.point);
//                    points[i] = hit.point;
//                    i++;
//                }
//            }
//            yield return null;
//        }
//        //at this point we have all the points
//        //calculate the distances for the original points
//        originalDistances = MathUtil.getDistances(location.TrackingPoints.ToArray());
//        distances = MathUtil.getDistances(points);
//        for(int i=0; i<originalDistances.Length; i++)
//        {
//            float diff = Mathf.Abs(originalDistances[i] - distances[i]);
//            Debug.Log("delta  is " + diff);
//            if (diff > delta)
//                Debug.Log("Distance from " + i + " to " + (i + 1) % originalDistances.Length + " exceeds delta");
//        }
//        //TODO: validate the deltas and recal if necessary
//        //calculate the center these points
//        Vector3 originalCenter, center;
//        originalCenter = location.transform.InverseTransformPoint(MathUtil.getCenter(location.TrackingPoints.ToArray()));//local coordinates
//        center = MathUtil.getCenter(points);
//        Debug.Log("center of selected area is " + center);
//        Quaternion rotation = getRotation(location.TrackingPoints.ToArray(), points);

//        Vector3 pos = center;
//        Vector3 eulerRotation = rotation.eulerAngles;
//        //StartCoroutine(moveto(location, pos, center, originalCenter, eulerRotation));

//        location.transform.position = center;//should have the center resting at the center
//        location.transform.localPosition -= originalCenter;
//        //location.transform.rotation = rotation;


//        //Vector3 centerDelta = obj2Center - obj1Center
//        ////////////////
//        //Vector3 origPointRelCenter = location.TrackingPoints[0] - originalCenter;

//        //Vector3 touchedPointRelCenter = points[0] - center;
//        //Quaternion rot = Quaternion.FromToRotation(origPointRelCenter, touchedPointRelCenter);

//        //Vector3 singlePointRotation = rot.eulerAngles;
//        //location.transform.RotateAround(center, Vector3.right, singlePointRotation.x);
//        //location.transform.RotateAround(center, Vector3.up, singlePointRotation.y);
//        //location.transform.RotateAround(center, Vector3.forward, singlePointRotation.z);

//        ////point 2
//        //origPointRelCenter = location.transform.TransformPoint(location.TrackingPoints[1]) - location.transform.TransformPoint(location.TrackingPoints[0]);
//        //touchedPointRelCenter = points[1] - points[0];
//        //rot = Quaternion.FromToRotation(origPointRelCenter, touchedPointRelCenter);

//        //singlePointRotation = rot.eulerAngles;
//        //location.transform.RotateAround(points[0], Vector3.right, singlePointRotation.x);
//        //location.transform.RotateAround(points[0], Vector3.up, singlePointRotation.y);
//        //location.transform.RotateAround(points[0], Vector3.forward, singlePointRotation.z);

//        ////point 3
//        //origPointRelCenter = location.transform.TransformPoint(location.TrackingPoints[2]) - location.transform.TransformPoint(location.TrackingPoints[0]/2+location.TrackingPoints[1]/2);
//        //touchedPointRelCenter = points[2] - (points[1]/2 + points[0]/2);
//        //rot = Quaternion.FromToRotation(origPointRelCenter, touchedPointRelCenter);

//        //singlePointRotation = rot.eulerAngles;
//        //location.transform.RotateAround((points[1] / 2 + points[0] / 2), Vector3.right, singlePointRotation.x);
//        //location.transform.RotateAround((points[1] / 2 + points[0] / 2), Vector3.up, singlePointRotation.y);
//        //location.transform.RotateAround((points[1] / 2 + points[0] / 2), Vector3.forward, singlePointRotation.z);

//        /////////////////
//        location.transform.position = center;//should have the center resting at the center
//        location.transform.localPosition -= originalCenter;
//        //location.transform.rotation = rotation;
//        //Vector3 eulerRotation = rotation.eulerAngles;
//        //location.transform.RotateAround(center, Vector3.right, eulerRotation.x);
//        //location.transform.RotateAround(center, Vector3.up, eulerRotation.y);
//        //location.transform.RotateAround(center, Vector3.forward, eulerRotation.z);


//        ///normal rotationer
//        //original normal
//        Quaternion originalLookRotation = Quaternion.LookRotation(location.forward, location.up);//
//        Debug.Log("original orientation = " + originalLookRotation.eulerAngles);
//        // touched normal
//        Vector3 touchedNormal = MathUtil.getAverageNormal(points);
//        Quaternion touchedLookRotation = Quaternion.LookRotation(points[points.Length-1]-points[0], touchedNormal);//
//        Debug.Log("touched orientation = " + touchedLookRotation.eulerAngles);
//        Quaternion deltaRotation = touchedLookRotation * Quaternion.Inverse(originalLookRotation);
//        Debug.Log("delta orientation = " + deltaRotation.eulerAngles);
//        //location.transform.rotation = deltaRotation * location.transform.rotation;
//        location.transform.Rotate(deltaRotation.eulerAngles);
//        location.transform.position = center;//should have the center resting at the center
//        originalCenter = MathUtil.getCenter(location.TrackingPoints.ToArray());//local coordinates

//        location.transform.position -= originalCenter - center;
//        //location.transform.RotateAround(center, Vector3.right, deltaRotation.eulerAngles.x);
//        //location.transform.RotateAround(center, Vector3.up, deltaRotation.eulerAngles.y);
//        //location.transform.RotateAround(center, Vector3.forward, deltaRotation.eulerAngles.z);
//    }
//    private IEnumerator moveto(TrackableLocation location, Vector3 pos, Vector3 center, Vector3 offset,  Vector3 rotation)
//    {
//       while( Vector3.Distance(location.transform.position, pos) > 0.01f)
//        {
//            location.transform.position = Vector3.Lerp(location.transform.position, pos, Time.deltaTime);
//            yield return null;
//        }
//        Vector3 offsetWorld = location.transform.TransformPoint(-offset);
//        while (Vector3.Distance(location.transform.position, offsetWorld) > 0.01f)
//        {
//            location.transform.position = Vector3.Lerp(location.transform.position, offsetWorld, Time.deltaTime);
//            yield return null;
//        }
//        Vector3 rotationAmount = Vector3.zero;
//        while (!Mathf.Approximately(rotationAmount.x,rotation.x))
//        {
//            float rotAmount;
//            if(rotation.x > rotationAmount.x)
//                rotAmount = Time.deltaTime * rotation.x;
//            else 
//                rotAmount = -Time.deltaTime * rotation.x;

//            if (Mathf.Abs(rotationAmount.x - rotation.x) < Mathf.Abs(rotationAmount.x + rotAmount - rotation.x))
//                rotAmount = rotation.x - rotationAmount.x;
//            rotationAmount.x += rotAmount;
            
//            location.transform.RotateAround(center, Vector3.right, rotAmount);

//            yield return null;
//        }
//        while (!Mathf.Approximately(rotationAmount.y, rotation.y))
//        {
//            float rotAmount;
//            if (rotation.y > rotationAmount.y)
//                rotAmount = Time.deltaTime * rotation.y;
//            else
//                rotAmount = -Time.deltaTime * rotation.y;

//            if (Mathf.Abs(rotationAmount.y - rotation.y) < Mathf.Abs(rotationAmount.y + rotAmount - rotation.y))
//                rotAmount = rotation.y - rotationAmount.y;
//            rotationAmount.y += rotAmount;

//            location.transform.RotateAround(center, Vector3.up, rotAmount);
//            yield return null;
//        }
//        while (!Mathf.Approximately(rotationAmount.z, rotation.z))
//        {
//            float rotAmount;
//            if (rotation.z > rotationAmount.z)
//                rotAmount = Time.deltaTime * rotation.z;
//            else
//                rotAmount = -Time.deltaTime * rotation.z;

//            if (Mathf.Abs(rotationAmount.z - rotation.z) < Mathf.Abs(rotationAmount.z + rotAmount - rotation.z))
//                rotAmount = rotation.z - rotationAmount.z;
//            rotationAmount.z += rotAmount;

//            location.transform.RotateAround(center, Vector3.forward, rotAmount);
//            yield return null;
//        }
//    }

//    //private Quaternion getRotation(Vector3[] obj1, Vector3[] obj2)
//    //{
//    //    Vector3 rotation = Vector3.zero;
//    //    Quaternion quatRotation = new Quaternion();
//    //    Vector3 obj1Center = MathUtil.getCenter(obj1);
//    //    Vector3 obj2Center = MathUtil.getCenter(obj2);
//    //    //Vector3 centerDelta = obj2Center - obj1Center;
//    //    Vector4 cumulative = new Vector4();
//    //    Quaternion rot0 = Quaternion.identity ;
//    //    for (int i = 0; i < obj1.Length; i++)
//    //    {
//    //        //move both objects to 0,0
//    //        obj1[i] -= obj1Center;
//    //        obj2[i] -= obj2Center;

//    //        Quaternion rot = Quaternion.FromToRotation(obj1[i], obj2[i]);
//    //        if (i == 0)
//    //            rot0 = rot;
//    //        Debug.Log("rotation delta = " + rot.eulerAngles);
//    //        rotation += rot.eulerAngles;

//    //        quatRotation = MathUtil.AverageQuaternion(ref cumulative, rot, rot0, i);
//    //    }
//    //    rotation /= obj1.Length;

//    //    Debug.Log("average rotation " + rotation);
//    //    Debug.Log("average quat rotation " + rot0.eulerAngles);
//    //    return quatRotation;
//    //}

//    private Quaternion getRotation(Vector3[] obj1, Vector3[] obj2)
//    {
//        Vector3 rotation = Vector3.zero;
//        Quaternion quatRotation = new Quaternion();
//        Vector3 obj1Center = MathUtil.getCenter(obj1);
//        Vector3 obj2Center = MathUtil.getCenter(obj2);
//        //Vector3 centerDelta = obj2Center - obj1Center;
//        Vector4 cumulative = new Vector4();
//        Quaternion rot0 = Quaternion.identity ;
//        for (int i = 0; i < obj1.Length; i++)
//        {
//            //move both objects to 0,0

//            Vector3 zeroedStart = obj1[i] - obj1Center;
//            Vector3 zeroedEnd = obj2[i] - obj2Center;
//            //Quaternion rot = Quaternion.FromToRotation(zeroedStart, zeroedEnd);
//            float angle = Vector3.Angle(zeroedStart, zeroedEnd);
//            Vector3 cp = Vector3.Cross(zeroedStart, zeroedEnd);
//            Quaternion rot = Quaternion.AngleAxis(angle, cp);
//            //Quaternion rot = Quaternion.(zeroedStart, zeroedEnd);

//            if (i == 0)
//                rot0 = rot;
//            Debug.Log("rotation delta = " + rot.eulerAngles);
//            rotation += rot.eulerAngles;

//            quatRotation = MathUtil.AverageQuaternion(ref cumulative, rot, rot0, i);
//        }
//        rotation /= obj1.Length;
       
//        Debug.Log("average rotation " + rotation);
//        Debug.Log("average quat rotation " + rot0.eulerAngles);
//        return quatRotation;
//    }
//}
