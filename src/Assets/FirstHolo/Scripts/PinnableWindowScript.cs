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
public class PinnableWindowScript : MonoBehaviour {
    [SerializeField]
    private bool m_pinned = true;

    private float paddingFromSurface = .1f;

    public float defaultDistance = 2f;

    public float maxHitDistance = 3f;

    private float springFactor = 10f;

    public Button pinButton;

    public Button hiddenPinButton;

    public AudioClip pinSound;

    public AudioClip unpinSound;

    private AudioSource audioSource;

    private Coroutine windowMoveRoutine;

    public float angularThreshold = 90f;

    private float m_timer;

    public float timeThreshold = 5f;
    private Vector3 m_distance;
    // Use this for initialization
    void Start () {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = this.gameObject.AddComponent<AudioSource>();

        pinned = pinned;
        //if(m_pinned)
        //    hiddenPinButton.gameObject.SetActive(false);
        //else
        //    hiddenPinButton.gameObject.SetActive(true);
    }
	
	// Update is called once per frame
	void Update () {
        //TODO: maybe make this a coroutine
	    if(!m_pinned)
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, maxHitDistance))
            {
                transform.position = Vector3.Lerp(transform.position, hit.point + hit.normal * paddingFromSurface, Time.deltaTime * springFactor);
                //transform.forward = -hit.normal;
               transform.forward = Vector3.Slerp(transform.forward, -new Vector3(hit.normal.x,0,hit.normal.z), Time.deltaTime * springFactor);
                
                //transform.up = Vector3.Slerp(transform.up, Camera.main.transform.up, Time.deltaTime * springFactor);
                m_distance = transform.position - Camera.main.transform.position;
            }
            else {

                // below here to disable the "sticky window" effect 
                //float angularOffset = Vector3.Angle(distance, Camera.main.transform.forward);
                //if (angularOffset > angularThreshold)
                //{
                //    m_timer += Time.deltaTime;
                //}


                //if (m_timer > timeThreshold)
                //{
                //    Vector3 inFrontOfCamera = Camera.main.transform.position + Camera.main.transform.forward * defaultDistance;
                //    transform.position = Vector3.Lerp(transform.position, inFrontOfCamera, Time.deltaTime * springFactor);
                //    transform.forward = Vector3.Lerp(transform.forward, Camera.main.transform.forward, Time.deltaTime * springFactor) ;

                //    distance = transform.position - Camera.main.transform.position;
                //    if (transform.position == inFrontOfCamera && transform.forward == Camera.main.transform.forward)
                //    {
                //        m_timer = 0;  
                //    }
                //}
                //else
                //{
                //    transform.position = Camera.main.transform.position + distance;
                //    transform.forward = distance;
                //}
                //end

                // comment/uncomment these lines to restore simple window/camera tracking
                transform.position = Vector3.Slerp(Camera.main.transform.position + m_distance, Camera.main.transform.position + defaultDistance * Camera.main.transform.forward, Time.deltaTime * springFactor);
                transform.rotation = Quaternion.Slerp(transform.rotation, Camera.main.transform.rotation, Time.deltaTime * springFactor);
                //make sure the window doesn't get closer than the default distance
                m_distance = transform.position - Camera.main.transform.position;
                if (m_distance.magnitude < defaultDistance)
                    transform.position += (defaultDistance - m_distance.magnitude)* m_distance.normalized;

                m_distance = transform.position - Camera.main.transform.position;
                //transform.forward = Vector3.Slerp(transform.forward, Camera.main.transform.forward, Time.deltaTime * springFactor);
                //this will keep the window obeying gravity when free floating
                //transform.LookAt(new Vector3(transform.position.x + m_distance.x, transform.position.y, transform.position.z + m_distance.z));
            }
        }
	}

    public void TogglePin()
    {
        pinned = !pinned;
    }
    public bool pinned 
    {
        set
        {
            m_pinned = value;
            if (m_pinned)
            {
                //play pin noise
                audioSource.clip = pinSound;
                audioSource.Play();

                pinButton.gameObject.SetActive(true);
                hiddenPinButton.gameObject.SetActive(false);
                m_pinned = true;

                Debug.Log("Pinned");
            }
            else {
                if (windowMoveRoutine != null)
                {
                    StopCoroutine(windowMoveRoutine);
                    windowMoveRoutine = null;
                }
                //play unpin noise
                audioSource.clip = unpinSound;
                audioSource.Play();

                m_distance = transform.position - Camera.main.transform.position;

                pinButton.gameObject.SetActive(false);
                hiddenPinButton.gameObject.SetActive(true);
                m_pinned = false;
                Debug.Log("Unpinned");
            }
        }
        get { return m_pinned; }
    }


    public void MoveWindow(Vector3 position)
    {
        if (windowMoveRoutine != null) {
            StopCoroutine(windowMoveRoutine);
            windowMoveRoutine = null;
        }
        if (m_pinned)//only move if the window is pinned, otherwise it'll try to move back to in front of the camera
            windowMoveRoutine = StartCoroutine(MoveWindowRoutine(position));
    }
    public void MoveWindow(Vector3 position, Quaternion rotation)
    {
        if (windowMoveRoutine != null)
        {
            StopCoroutine(windowMoveRoutine);
            windowMoveRoutine = null;
        }
        if (m_pinned)//only move if the window is pinned, otherwise it'll try to move back to in front of the camera
            windowMoveRoutine = StartCoroutine(MoveWindowRoutine(position, rotation));
    }
    private IEnumerator MoveWindowRoutine(Vector3 position, Quaternion rotation)
    {
        while (position != transform.position || rotation != transform.rotation)
        {
            transform.position = Vector3.Lerp(transform.position, position, Time.deltaTime * springFactor);
            transform.rotation = rotation;//Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * springFactor);
            yield return null;
        }
        m_distance = transform.position - Camera.main.transform.position;
        windowMoveRoutine = null;
    }
    private IEnumerator MoveWindowRoutine(Vector3 position)
    {
        return MoveWindowRoutine(position, transform.rotation);
    }
}
