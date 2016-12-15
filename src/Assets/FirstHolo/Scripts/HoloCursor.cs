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
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class HoloCursor : MonoBehaviour {
    public Canvas reticleCanvas;
    private Coroutine reticleMovement;
    // reticle info
    private float springFactor = 10f;
    public float reticleDistanceFromSurface = .05f;
    //private Canvas reticleCanvas;
    public float defaultReticleDistance = 4f;
    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        //update Reticle
        //CHeck UI first
        float newDistance;
        PointerEventData data = new PointerEventData(EventSystem.current);
        data.position = new Vector2(Screen.width / 2, Screen.height / 2);
        var raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, raycastResults);
        var raycast = raycastResults.Find(x => x.gameObject.layer != LayerMask.NameToLayer("HUD") && x.isValid);
        //FindFirstRaycast(raycastResults);

        float uiDistance = raycast.distance + Camera.main.nearClipPlane;
        RaycastHit hit;
        // Check to see if we have a geometry hit
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, 10))
        {
            // if we also have a UI hit, use whichever is closer
            if (raycast.isValid)
                newDistance = Mathf.Min(uiDistance, hit.distance) - reticleDistanceFromSurface;
            else
                newDistance = hit.distance - reticleDistanceFromSurface;
        }
        else if (raycast.isValid)
            newDistance = uiDistance - reticleDistanceFromSurface;
        else
            newDistance = defaultReticleDistance;
        //keep the reticle far away enough from the camera to see
        newDistance = Mathf.Max(Camera.main.nearClipPlane + .01f, newDistance);
        if (float.IsNaN(newDistance))
        { //BUG: Don't know why this happens, attempting a workaround
            Debug.LogError("Plane distance is NaN!");
            newDistance = defaultReticleDistance;
        }

        if (reticleMovement != null)
            StopCoroutine(reticleMovement);
        reticleMovement = StartCoroutine(MoveReticle(newDistance));
    }

    private IEnumerator MoveReticle(float distance)
    {
        while (reticleCanvas != null && !Mathf.Approximately(distance, reticleCanvas.planeDistance))
        {
            //lerp to the new distance, but don't let the canvas fall behind the object
            float planeDistance = Mathf.Min(distance, Mathf.Lerp(reticleCanvas.planeDistance, distance, Time.deltaTime * springFactor));
            if (float.IsNaN(planeDistance))
                Debug.LogError("Plane distance is NaN!");
            reticleCanvas.planeDistance = Mathf.Min(distance, Mathf.Lerp(reticleCanvas.planeDistance, distance, Time.deltaTime * springFactor));

            yield return null;
        }
    }
}
