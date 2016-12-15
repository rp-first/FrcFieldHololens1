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

public class UIObjectTracker : MonoBehaviour {
    //this is your object that you want to have the UI element hovering over
    public Transform TrackedObject;
    [SerializeField]
    private bool m_hideWhenInFrame = false;
    public bool hideWhenInFrame {
        get { return m_hideWhenInFrame; }
        set { m_hideWhenInFrame = value; }
    }
 
    public Image image;
    public Sprite selected;
    public Sprite pointer;

    public float distanceFromObject = .1f;
    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (TrackedObject != null)
        {
            //first you need the RectTransform component of your canvas
            RectTransform CanvasRect = GetComponent<RectTransform>();
            Canvas canvas = GetComponent<Canvas>();
            //then you calculate the position of the UI element
            //0,0 for the canvas is at the center of the screen, whereas WorldToViewPortPoint treats the lower left corner as 0,0. Because of this, you need to subtract the height / width of the canvas * 0.5 to get the correct position.

            //// if the object has a collider, we want to draw on its surface, not its origin
            // 

            Vector3 objDirection = TrackedObject.transform.position - Camera.main.transform.position;
            Vector3 ViewportPosition = Vector3.zero;
            //RaycastHit hit;
            //if (Physics.Raycast(Camera.main.transform.position, objDirection, out hit, 10))
            float cursorSize;
            cursorSize = Mathf.Max(image.rectTransform.rect.width * image.rectTransform.lossyScale.x, 
                                    image.rectTransform.rect.height * image.rectTransform.lossyScale.y) / 2;
            RaycastHit[] hits = Physics.SphereCastAll(Camera.main.transform.position, cursorSize, objDirection, 10);
            //if (Physics.SphereCast(Camera.main.transform.position,1, objDirection, out hit, 10))
            RaycastHit hit = System.Array.Find(hits, x => x.collider.gameObject == TrackedObject.gameObject);
            //Debug.Log("point: " + hit.point + " cursor: " +cursorSize);
            if (hit.collider != null)
                ViewportPosition = Camera.main.WorldToViewportPoint(hit.point - Camera.main.transform.forward * (cursorSize + distanceFromObject));
            else
                ViewportPosition = Camera.main.WorldToViewportPoint(TrackedObject.transform.position - Camera.main.transform.forward * (cursorSize + distanceFromObject));
            //if (hits.Length > 0)
            //{
            //    //print("There is something in front of the object!");
            //    foreach (RaycastHit hit in hits)
            //        if (hit.collider.gameObject == WorldObject)
            //        {
            //            ViewportPosition = Camera.main.WorldToViewportPoint(hit.point);
            //            break;
            //        }
            //}
            //////
            //else
            //    ViewportPosition = Camera.main.WorldToViewportPoint(WorldObject.transform.position);
            if (m_hideWhenInFrame)
                image.gameObject.SetActive(false);
            else {
                image.gameObject.SetActive(true);

            }

            if (ViewportPosition.z < 0) //behind camera
            {
                ViewportPosition.x = 1.0f - ViewportPosition.x;
                ViewportPosition.y = 0;
            }

            //keep the view on screen
            if (ViewportPosition.x <= 0 || ViewportPosition.x >= 1 ||
               ViewportPosition.y <= 0 || ViewportPosition.y >= 1)
            {
                Vector2 viewport2d = ViewportPosition;
                // transform into coordinate space with center of screen as origin and screen of unit width and height
                viewport2d -= new Vector2(.5f, .5f);
                viewport2d *= 2f;
                // bring things back onto the screen
                if (Mathf.Abs(viewport2d.x) > Mathf.Abs(viewport2d.y))
                    viewport2d /= Mathf.Abs(viewport2d.x);
                else
                    viewport2d /= Mathf.Abs(viewport2d.y);

                float rotation = Mathf.Rad2Deg * Mathf.Atan2(viewport2d.y, viewport2d.x);

                //Debug.Log(rotation);
                image.rectTransform.localRotation = Quaternion.Euler(0, 0, rotation);


                // transform back into standard screen space
                viewport2d /= 2f;
                viewport2d += new Vector2(.5f, .5f);

                ViewportPosition.x = viewport2d.x;
                ViewportPosition.y = viewport2d.y;
                //ViewportPosition.z = Mathf.Max(ViewportPosition.z,defaultCanvasDistance);
                image.sprite = pointer;
                if (m_hideWhenInFrame)
                    image.gameObject.SetActive(true);

            }
            else
                image.sprite = selected;
            //ViewportPosition.x = Mathf.Clamp(ViewportPosition.x, UI_Element.sizeDelta.x / CanvasRect.sizeDelta.x, 1.0f - UI_Element.sizeDelta.x / CanvasRect.sizeDelta.x);
            //ViewportPosition.y = Mathf.Clamp(ViewportPosition.y, UI_Element.sizeDelta.y / CanvasRect.sizeDelta.y, 1.0f - UI_Element.sizeDelta.y / CanvasRect.sizeDelta.y);

            // Place pointer/directional arrow at edge of screen
            //Vector3 WorldObject_ScreenPosition = new Vector3(
            //((ViewportPosition.x * CanvasRect.sizeDelta.x) - (CanvasRect.sizeDelta.x * 0.5f)),
            //((ViewportPosition.y * CanvasRect.sizeDelta.y) - (CanvasRect.sizeDelta.y * 0.5f)),
            // 0);

            // Place pointer/directional arrow at the circle around center of the screen. The radius can be
            // set in Unity or default 100f scale
            //Vector3 WorldObject_ScreenPosition = new Vector3(
            //((ViewportPosition.x * pointerDistanceFromCursor) - (pointerDistanceFromCursor * 0.5f)),
            //((ViewportPosition.y * pointerDistanceFromCursor) - (pointerDistanceFromCursor * 0.5f)),
            // 0);

            //// Place pointer/directional arrow at center of screeen
            ////Vector3 WorldObject_ScreenPosition = new Vector3((0.5f), (0.5f), 0);

            //WorldObject_ScreenPosition.x = Mathf.Lerp(UI_Element.anchoredPosition.x, WorldObject_ScreenPosition.x, SpringFactor * Time.deltaTime);
            //WorldObject_ScreenPosition.y = Mathf.Lerp(UI_Element.anchoredPosition.y, WorldObject_ScreenPosition.y, SpringFactor * Time.deltaTime);
            ////now you can set the position of the ui element
            //UI_Element.anchoredPosition = WorldObject_ScreenPosition;
            ////UI_Element.anchoredPosition3D = WorldObject_ScreenPosition;
            //canvas.planeDistance = Mathf.Clamp(ViewportPosition.z, minDist, maxDist);
        }
        else
            image.gameObject.SetActive(false);
    }

    public void SetTrackedObject(Transform newObject)
    {
        TrackedObject = newObject;
    }

    public void Disable()
    {
        TrackedObject = null;
    }
}
