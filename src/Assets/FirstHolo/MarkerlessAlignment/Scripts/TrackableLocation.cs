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
using System.Collections.Generic;
using System;
using UnityEngine.VR.WSA.Persistence;
using UnityEngine.VR.WSA;
using UnityEngine.Events;

[Serializable]
public class TrackableLocation : MonoBehaviour
{
    public string id;
    private WorldAnchorStore store;

    public GameObject pointStandin;

    [SerializeField]
    private bool m_visualizeTrackingPoints = false;
    public bool visualizeTrackingPoints
    {
        get { return m_visualizeTrackingPoints; }
        set {
            m_visualizeTrackingPoints = value;
            foreach(Transform t in m_trackingPoints)
            {

                if (t.childCount == 0)
                    GameObject.Instantiate(pointStandin, t, false);
                
                for (int i = 0; i < t.childCount; i++)
                    t.GetChild(i).gameObject.SetActive(value);
            }
        }
    }
    [SerializeField]
    private List<Transform> m_trackingPoints = new List<Transform>();

    private List<GameObject> m_anchors = new List<GameObject>();

    public bool anchored = true;
    
    public List<Transform> TrackingPoints
    {
        get { return m_trackingPoints; }
        //set { m_trackingPoints = value; }
    }

    public List<GameObject> Anchors {
        get { return m_anchors; }
    }

    public Vector3 forward
    {
        get { if (TrackingPoints.Count == 0)
                return Vector3.zero;
                return TrackingPoints[TrackingPoints.Count - 1].transform.position - TrackingPoints[0].transform.position; }
    }

    public Vector3 up
    {
        get {
            if (TrackingPoints.Count < 3)
                return Vector3.up;

            Vector3[] points = new Vector3[TrackingPoints.Count];
            for (int i = 0; i < points.Length; i++)
                points[i] = TrackingPoints[i].transform.position;
            return MathUtil.getAverageNormal(points); }
    }

    public Vector3 right
    {
        get { return Vector3.Cross(up, forward); }
    }

    public Vector3 center
    {
        get {
            Vector3[] points = new Vector3[TrackingPoints.Count];
            for (int i = 0; i < points.Length; i++)
                points[i] = TrackingPoints[i].transform.position;
            return MathUtil.getCenter(points);
        }
    }
    public float[] distances
    {
        get
        {
            Vector3[] points = new Vector3[TrackingPoints.Count];
            for (int i = 0; i < points.Length; i++)
                points[i] = TrackingPoints[i].transform.position;
            return MathUtil.getDistances(points);
        }
    }

    void OnDrawGizmos()
    {
        //show the points
        foreach (Transform point in m_trackingPoints)
        {
            Gizmos.color = new Color(1, 0, 0, 0.5F);
            Gizmos.DrawCube(point.transform.position, new Vector3(.01f, .01f, .01f));
        }
        //Draw lines connecting the points
        for (int i = 1; i < m_trackingPoints.Count; i++)
        {
            Gizmos.color = new Color(1, 0, 0, 0.5F);
            Gizmos.DrawLine(m_trackingPoints[i].transform.position, m_trackingPoints[i - 1].transform.position);
        }
        //last line
        if (m_trackingPoints.Count > 2)
        {
            Gizmos.color = new Color(1, 0, 0, 0.5F);
            Gizmos.DrawLine(m_trackingPoints[m_trackingPoints.Count - 1].transform.position, m_trackingPoints[0].transform.position);
        }

        //points center
        Gizmos.color = new Color(0, 0, 1, 0.5F);
        Vector3[] points = new Vector3[m_trackingPoints.Count];

        for (int i = 0; i < points.Length; i++)
            points[i] = m_trackingPoints[i].transform.position;
        Vector3 center = MathUtil.getCenter(points);
        Gizmos.DrawCube(center, new Vector3(.01f, .01f, .01f));

        //points normal
        Gizmos.color = new Color(0, 1, 0, 0.5F);
        Gizmos.DrawLine(center, center + up);

        //points pseudo-forward
        Gizmos.color = new Color(0, 0, 1, 0.5F);
        Gizmos.DrawLine(center, center + forward);

        //points pseudo-right
        Gizmos.color = new Color(1, 0, 0, 0.5F);
        Gizmos.DrawLine(center, center + right);

        //transform center
        Gizmos.color = new Color(0, 1, 0, 0.5F);
        Gizmos.DrawCube(transform.position, new Vector3(.01f, .01f, .01f));
    }


    void Start()
    {
        //retrieve the WorldAnchorStore and load previous calibration values
        WorldAnchorStore.GetAsync(StoreLoaded);
        visualizeTrackingPoints = visualizeTrackingPoints;
    }
    void Update()
    {
        //TODO:  Only do this when anchors position change
        if(anchored)
            AlignToAnchors();
    }
    public void LoadSavedLocation()
    {
        
        for(int i = 0; i < m_trackingPoints.Count; i++)
        {
            if (i >= Anchors.Count)
            {
                m_anchors.Add(new GameObject());
                //default location to where the workspace currently is
                m_anchors[i].transform.position = m_trackingPoints[i].position;
            }
            string sub_id = id + "_" + i;
            if (store.Load(sub_id, m_anchors[i].gameObject))
                Debug.Log("Loaded anchor for " + sub_id);
            else
            {
                Debug.Log("Failed to load anchor for " + sub_id+ ", creating new one");
                m_anchors[i].gameObject.AddComponent<WorldAnchor>();
            }
        }
       

    }

    private void AlignToAnchors()
    {
        if (m_anchors.Count == m_trackingPoints.Count)
        {
            Vector3 trackingPointsCenter, anchorsGlobalCenter;
            //transform.rotation = Quaternion.identity;
            Vector3[] anchorPoints = new Vector3[m_anchors.Count];
            for(int i = 0; i < m_anchors.Count; i++)
            {
                //points[i] = m_anchors[i].transform.position;
                anchorPoints[i] = m_anchors[i].transform.position;
            }
            //global center
            anchorsGlobalCenter = MathUtil.getCenter(anchorPoints);
            
            Vector3[] localTrackingPoints = new Vector3[m_trackingPoints.Count];
            
            for (int i = 0; i < m_trackingPoints.Count; i++)
            {
                //Get the tracking points position relative to the TrackableLocation space
                localTrackingPoints[i] = this.transform.InverseTransformPoint(m_trackingPoints[i].transform.position);
            }
            Vector3 localTrackingPointsNormal = MathUtil.getAverageNormal(localTrackingPoints);
            Vector3 localForward = localTrackingPoints[localTrackingPoints.Length - 1] - localTrackingPoints[0];
            Quaternion originalLookRotation = Quaternion.LookRotation(localForward, localTrackingPointsNormal);
            
            // Anchor's normal
            Vector3 anchorsNormal = MathUtil.getAverageNormal(anchorPoints);
            Vector3 anchorsForward = anchorPoints[anchorPoints.Length - 1] - anchorPoints[0];
            Quaternion anchorsLookRotation = Quaternion.LookRotation(anchorsForward, anchorsNormal);//
            Quaternion deltaRotation = anchorsLookRotation * Quaternion.Inverse(originalLookRotation);

            transform.rotation= deltaRotation;
            trackingPointsCenter = this.center;
            Vector3 deltaPosition = anchorsGlobalCenter - trackingPointsCenter;
            transform.position += deltaPosition;
          
            
        }
    }

    public void SaveLocation()
    {
        if (store != null)
        {
            for (int i = 0; i < m_anchors.Count; i++)
            {
                WorldAnchor anchor = m_anchors[i].gameObject.GetComponent<WorldAnchor>();
                if (anchor == null)
                    anchor = m_anchors[i].gameObject.AddComponent<WorldAnchor>();

                string sub_id = id + "_" + i;

                if (store.Delete(sub_id))
                {
                    Debug.Log("Deleted anchor for " + sub_id);
                }
                else
                {
                    Debug.Log("Failed to delete anchor for " + sub_id);
                }

                if (store.Save(sub_id, anchor))
                    Debug.Log("Saved anchor for " + sub_id);
                else
                    Debug.Log("Failed to save anchor for " + sub_id);
            }
        }
        else
            Debug.Log("Unable to save location, store not loaded");
    }

    private void StoreLoaded(WorldAnchorStore store)
    {
        this.store = store;
        LoadSavedLocation();
    }

}
