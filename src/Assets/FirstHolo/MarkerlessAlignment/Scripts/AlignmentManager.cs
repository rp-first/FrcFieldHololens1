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
using UnityEngine.VR.WSA.Input;
using System;
using UnityEngine.VR.WSA;
using UnityEngine.Events;
using HoloToolkit.Unity.InputModule;

public class AlignmentManager : MonoBehaviour, IInputHandler, IManipulationHandler
{
    public float delta = 0.1f;
    
    public UnityEngine.VR.WSA.SpatialMappingRenderer spatialRenderer;
    public UnityEngine.VR.WSA.SpatialMappingCollider spatialCollider;
    public AudioSource audioSource;
    public AudioClip clip;
    public GameObject trackingBeacon;
    private enum CoroutineState { Inactive, Active, Reset, Cancelled };
    private CoroutineState coroutineState = CoroutineState.Inactive;
    private GameObject beaconCursor;
    private Vector3 originalHandPosition;
    struct HandState
    {
        public Vector3 position;
        public bool dragging;
        public bool pressed;
        public bool released;
        public uint id;
    }
    private HandState handState;
    private Vector3 originalBeaconPosition;
    private bool aligningAnchor = false;

    private GameObject cursor;

    public AlignmentStateManager stateManager;
    public void Align(TrackableLocation location)
    {
        // hide cursor
        var cc = FindObjectOfType<HoloToolkit.Unity.InputModule.Cursor>();
        if (cc)
        {
            cursor = cc.gameObject;
            cursor.SetActive(false);
        }

        // add input capture
        if(InputManager.IsInitialized)
        {
            InputManager.Instance.PushFallbackInputHandler(gameObject);
        }

        Debug.Log("Aligning " + location.name);
        if (spatialRenderer)
        {
            spatialRenderer.enabled = true;
            spatialRenderer.freezeUpdates = false;
        }
        if (spatialCollider)
            spatialCollider.freezeUpdates = false;

        // for adjustment after alignment
        location.ResetOffset();

        StartCoroutine(locate(location));
    }
    public void CancelAlignment()
    {
        coroutineState = CoroutineState.Cancelled;

        Cleanup();
    }
    public void ToggleAlignment(TrackableLocation location)
    {
        if(coroutineState != CoroutineState.Inactive)
        {
            CancelAlignment();
        }
        else
        {
            Align(location);
        }
    }
    IEnumerator locate(TrackableLocation location)
    {
        coroutineState = CoroutineState.Active;
        location.gameObject.SetActive(false);
        if(stateManager != null ) stateManager.AlignmentStartedEvent.Invoke();
        float[] distances, originalDistances;

        Transform[] points = new Transform[location.TrackingPoints.Count];
        // Disable WorldAnchor if it exists
        location.anchored = false;

        Quaternion originalRotation = location.transform.rotation;
        beaconCursor = GameObject.Instantiate<GameObject>(trackingBeacon);

        Vector3 initialHandPosition = Vector3.zero;
        if (stateManager != null && 0 < location.TrackingPoints.Count) stateManager.PointStartedEvent.Invoke(location.TrackingPoints[0].name + " started");
        for (int i = 0; i < points.Length;)
        {
            if(coroutineState == CoroutineState.Cancelled)
            {
                Destroy(beaconCursor);
                //destroy beacons
                foreach (Transform beacon in points)
                {
                    if (beacon != null)
                        Destroy(beacon.gameObject);
                }
                coroutineState = CoroutineState.Inactive;
                location.gameObject.SetActive(true);
                if (stateManager != null) stateManager.AlignmentEndedEvent.Invoke();
                yield break;
            }

            //show the beacon

            //UpdateHandState(ref handState);
            bool hit = false;
            RaycastHit gazeHit;
            hit = Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out gazeHit, 10);

            if (!handState.dragging && !handState.released)
            {
                if (hit)
                {
                    beaconCursor.SetActive(true);
                    beaconCursor.transform.position = gazeHit.point;
                    beaconCursor.transform.up = gazeHit.normal;
                }
                else
                {
                    beaconCursor.SetActive(false);
                }
            }

            if (handState.pressed  && hit)
            {
                originalBeaconPosition = beaconCursor.transform.position;
                originalHandPosition = handState.position;

                aligningAnchor = true;
            }

            else if(handState.dragging && aligningAnchor)
            {//if pressed we instead want to move it relative to the hand position
                beaconCursor.SetActive(true);
                beaconCursor.transform.position = originalBeaconPosition + (handState.position);// - originalHandPosition)   ;
             
            }
                

            if (handState.released && aligningAnchor) 
            {
                aligningAnchor = false;
                //Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

                //raycast from click
                //Ray mousePoint = Camera.main.ScreenPointToRay(Input.mousePosition);
                //RaycastHit hit;
                //if (Physics.Raycast(ray, out hit, 10))
                //{
                print("hit[" + i + "] = " + beaconCursor.transform.position);
                   
                    if (stateManager != null) stateManager.PointEndedEvent.Invoke(location.TrackingPoints[i].name + " completed");
                if (trackingBeacon != null)
                {
                    points[i] = GameObject.Instantiate<GameObject>(beaconCursor).transform;
                }
                else
                {
                    points[i] = new GameObject("WAnchor_" + i).transform;
                    points[i].position = beaconCursor.transform.position;
                }
                i++;
                if (stateManager != null && i < location.TrackingPoints.Count) stateManager.PointStartedEvent.Invoke(location.TrackingPoints[i].name + " started");

                audioSource.clip = clip;
                    audioSource.Play();
                
                }
            //}

            // should reset
            handState.pressed = false;
            handState.released = false;
            yield return null;
        }
        Destroy(beaconCursor);

        //set these anchors on the trackable location
        for (int i = 0; i < points.Length; i++)
        {
            GameObject locationAnchorObject;
            if (i >= location.Anchors.Count)
            {
                location.Anchors.Add( new GameObject("WAnchor_"+i));
            }
            locationAnchorObject = location.Anchors[i];
            WorldAnchor locationAnchor = locationAnchorObject.GetComponent<WorldAnchor>();
            if(locationAnchor)
                DestroyImmediate(locationAnchor);

            locationAnchorObject.transform.position = points[i].position;
            locationAnchor = locationAnchorObject.AddComponent<WorldAnchor>();
        }
        location.anchored = true;



        if (spatialRenderer)
        {
            spatialRenderer.enabled = false;
            spatialRenderer.freezeUpdates = true;
        }
        if (spatialCollider)
            spatialCollider.freezeUpdates = true;

        //destroy beacons
        foreach (Transform beacon in points)
        {
            if(beacon != null)
                Destroy(beacon.gameObject);
        }
        if (stateManager != null) stateManager.AlignmentEndedEvent.Invoke();
        location.gameObject.SetActive(true);
        location.SaveLocation();
        coroutineState = CoroutineState.Inactive;

        // should reset
        Cleanup();
    }

    private Quaternion getRotation(Vector3[] obj1, Vector3[] obj2)
    {
        Vector3 rotation = Vector3.zero;
        Quaternion quatRotation = new Quaternion();
        Vector3 obj1Center = MathUtil.getCenter(obj1);
        Vector3 obj2Center = MathUtil.getCenter(obj2);
        Vector4 cumulative = new Vector4();
        Quaternion rot0 = Quaternion.identity;
        for (int i = 0; i < obj1.Length; i++)
        {
            //move both objects to 0,0

            Vector3 zeroedStart = obj1[i] - obj1Center;
            Vector3 zeroedEnd = obj2[i] - obj2Center;
            //Quaternion rot = Quaternion.FromToRotation(zeroedStart, zeroedEnd);
            float angle = Vector3.Angle(zeroedStart, zeroedEnd);
            Vector3 cp = Vector3.Cross(zeroedStart, zeroedEnd);
            Quaternion rot = Quaternion.AngleAxis(angle, cp);
            //Quaternion rot = Quaternion.(zeroedStart, zeroedEnd);

            if (i == 0)
                rot0 = rot;

            rotation += rot.eulerAngles;

            quatRotation = MathUtil.AverageQuaternion(ref cumulative, rot, rot0, i);
        }
        rotation /= obj1.Length;
        
        return quatRotation;
    }

    private void Cleanup()
    {
        // reshow the cursor
        if (cursor) cursor.SetActive(true);

        // remove the input capture
        if (InputManager.IsInitialized)
        {
            InputManager.Instance.PopFallbackInputHandler();
        }
    }

    public void OnInputUp(InputEventData eventData)
    {
        handState.dragging = false;
        handState.released = true;
    }

    public void OnInputDown(InputEventData eventData)
    {
        handState.pressed = true;
    }

    public void OnManipulationStarted(ManipulationEventData eventData)
    {
    }

    public void OnManipulationUpdated(ManipulationEventData eventData)
    {
        handState.dragging = true;
        handState.position = eventData.CumulativeDelta;
    }

    public void OnManipulationCompleted(ManipulationEventData eventData)
    {
        handState.dragging = false;
        handState.released = true;
    }

    public void OnManipulationCanceled(ManipulationEventData eventData)
    {
        handState.dragging = false;
        handState.released = true;
    }
}
