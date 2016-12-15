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
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

[ExecuteInEditMode]
public class HoloHud : MonoBehaviour {
    public Camera uiCamera;
    public RectTransform expandedHud;
    public RectTransform navigableArea;

    public const int MaxMargin = 2048;

    [Range(0, HoloHud.MaxMargin)]
    public int leftMargin = 0;

    [Range(0, HoloHud.MaxMargin)]
    public int rightMargin = 0;

    [Range(0, HoloHud.MaxMargin)]
    public int topMargin = 0;

    [Range(0, HoloHud.MaxMargin)]
    public int bottomMargin = 0;

    private RectTransform m_parentRect;
    public float planeDistance = 1.0f;
    private Vector3 uiPosition = new Vector3(0,0,0);

    //
    private Vector3 relativePosition;
    public Transform debugObject;
	// Use this for initialization
	void Start () {
        m_parentRect = GetComponent<RectTransform>();
        relativePosition = new Vector3(0,0,1);
    }
	
	// Update is called once per frame
	void Update () {
	    if(uiCamera != null)
        {
            m_parentRect.sizeDelta =new Vector2(uiCamera.pixelWidth, uiCamera.pixelHeight);


            if (expandedHud != null)
            {
                float hudWidth = uiCamera.pixelWidth + leftMargin + rightMargin;
                float hudHeight = uiCamera.pixelHeight + topMargin + bottomMargin;

                expandedHud.pivot = new Vector2((uiCamera.pixelWidth/2 + leftMargin) / hudWidth, (uiCamera.pixelHeight / 2 + bottomMargin) / hudHeight);
                expandedHud.anchoredPosition3D = Vector3.zero;

                expandedHud.sizeDelta = new Vector2(hudWidth, hudHeight);
            }
            if(navigableArea != null)
            {
                float navWidth = leftMargin + rightMargin;
                float navHeight = topMargin + bottomMargin;
                navigableArea.pivot = new Vector2(navWidth!=0?leftMargin / navWidth:0.5f,
                                                   navHeight!=0?bottomMargin/ navHeight:0.5f);
                navigableArea.anchoredPosition3D = Vector3.zero;

                navigableArea.sizeDelta = new Vector2(navWidth, navHeight);
            }
        }
        if (Application.isPlaying)
        {
            //keeps the ui relative to the camera without becoming a child, ignores camera rotation
            transform.position = uiCamera.transform.position + uiPosition;

            //keep at fixed relative distance
            //Vector3 distanceFromCamera = transform.position - uiCamera.transform.position;
            //transform.position = camera.transform.position + distanceFromCamera.normalized * planeDistance;
            //transform.forward = distanceFromCamera.normalized;


            var pointer = new PointerEventData(EventSystem.current);
            // convert to a 2D position
            pointer.position = new Vector2(Screen.width / 2, Screen.height / 2);//Vector2.zero;
            var raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, raycastResults);
            RaycastResult result = raycastResults.Find(x => x.gameObject == gameObject || x.gameObject.transform.IsChildOf(transform));
     
            if (result.gameObject == null)//can't find the canvas
            {
                //lock the ui to camera space
                //transform.position = uiCamera.transform.TransformPoint(relativePosition);
                transform.position = uiCamera.transform.position + uiCamera.transform.forward * planeDistance;
                //transform.position = Vector3.Slerp(transform.position, uiCamera.transform.position + uiCamera.transform.forward * uiPosition.magnitude, Time.deltaTime);
                transform.forward = transform.forward;// Vector3.Slerp(transform.forward, uiCamera.transform.forward, Time.deltaTime);
                transform.rotation = uiCamera.transform.rotation;//Quaternion.Lerp(transform.rotation, uiCamera.transform.rotation, Time.deltaTime);


            }
            else
            { //this fakes a "spherical" ui by matching the canvas forward with the camera forward 
              //problem here is that when the canvas is in frame, it won't translate if the user translates. rotation is ok for the most part
              //transform.position = Vector3.Lerp(transform.position, transform.position + camera.transform.forward * (planeDistance - result.distance), Time.deltaTime);
              //transform.forward = Vector3.Lerp(transform.forward, camera.transform.forward, Time.deltaTime);
              //transform.position = camera.transform.position + distanceFromCamera.normalized * planeDistance;

                //if this falls outside of our traversible canvas, lock the canvas
                Vector2 canvasPixel = m_parentRect.InverseTransformPoint(uiCamera.transform.position + uiCamera.transform.forward * planeDistance);
                // scale the canvas to unit size
                //Vector2 canvasPoint = canvasPixel;
                //canvasPoint.Scale(new Vector2(1.0f / m_parentRect.sizeDelta.x, 1.0f / m_parentRect.sizeDelta.y));
                ////Vector2 viewableCanvas = hudSize * m_parentRect.sizeDelta;

                //if (debugObject != null)
                //    debugObject.transform.position = m_parentRect.InverseTransformPoint(;
                float x_min = -leftMargin;
                float x_max = rightMargin;

                float y_min = -topMargin;
                float y_max = bottomMargin;
                if (canvasPixel.x < x_min || canvasPixel.x > x_max ||
                    canvasPixel.y < y_min || canvasPixel.y > y_max)
                {
                    //clamp 
                    Vector2 clampedScreenPixel = new Vector2(Mathf.Clamp(canvasPixel.x, x_min, x_max), Mathf.Clamp(canvasPixel.y, y_min, y_max));
                    Vector3 worldPoint = m_parentRect.TransformPoint(clampedScreenPixel);
                    Vector3 newPosition = worldPoint - (uiCamera.transform.position + uiCamera.transform.forward * planeDistance);
                    if (debugObject != null)
                        debugObject.transform.position = transform.position - newPosition;
                    //transform.position = Vector3.Slerp(transform.position, uiCamera.transform.position + uiCamera.transform.forward * uiPosition.magnitude, Time.deltaTime);
                    //if(newPosition.magnitude > 0.01f)
                        transform.position -= newPosition;//uiCamera.transform.TransformPoint(relativePosition);
                    transform.forward = transform.forward;// Vector3.Slerp(transform.forward, uiCamera.transform.forward, Time.deltaTime);
                    transform.rotation = uiCamera.transform.rotation;//Quaternion.Lerp(transform.rotation, uiCamera.transform.rotation, Time.deltaTime);

                }
                else
                {
                    //move the camera forward if the point being viewed 
                    transform.position = transform.position + uiCamera.transform.forward * (planeDistance - (result.distance + uiCamera.nearClipPlane));
                    transform.forward = uiCamera.transform.forward;
                    transform.rotation = uiCamera.transform.rotation;
                }


            }

            //check if the center of the camera falls off the canvas

            //transform.position = camera.transform.position + camera.transform.forward * planeDistance;
            //transform.forward = camera.transform.forward;

            //calculate scale
            //tan(theta) = o/a;
            float apparentWorldHeight = 2 * planeDistance * Mathf.Tan(Mathf.Deg2Rad * uiCamera.fieldOfView / 2);
            float scale = apparentWorldHeight / uiCamera.pixelHeight;

            ////fov of the canvas
            //float extendedCanvasWorldHeight = scale * uiCamera.pixelHeight * hudSize;
            //float thetaRads = 2 * Mathf.Atan2((extendedCanvasWorldHeight / 2), planeDistance);
            //Debug.Log( "extended canvas fov = " + Mathf.Rad2Deg * thetaRads);
            m_parentRect.localScale = Vector3.one * scale;

            uiPosition = transform.position - uiCamera.transform.position;
            relativePosition = uiCamera.transform.worldToLocalMatrix.MultiplyPoint3x4(transform.position);
        }
    }
}
