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

public class RectTransformScaler : MonoBehaviour {
    public RectTransform rect;
    public Canvas canvas;
    public Text width, height, canvasDist;
    public float speed = 50f;
    private bool m_incrementWidth = false;
    private bool m_decrementWidth = false;
    private bool m_incrementHeight = false;
    private bool m_decrementHeight = false;

    private bool m_incrementPlaneDist = false;
    private bool m_decrementPlaneDist = false;
    public bool incrementWidth
    {
        get { return m_incrementWidth; }
        set { m_incrementWidth = value; }
    }
    public bool decrementWidth
    {
        get { return m_decrementWidth; }
        set { m_decrementWidth = value; }
    }
    public bool incrementHeight
    {
        get { return m_incrementHeight; }
        set { m_incrementHeight = value; }
    }
    public bool decrementHeight
    {
        get { return m_decrementHeight; }
        set { m_decrementHeight = value; }
    }

    public bool incrementPlaneDist
    {
        get { return m_incrementPlaneDist; }
        set { m_incrementPlaneDist = value; }
    }
    public bool decrementPlaneDist
    {
        get { return m_decrementPlaneDist; }
        set { m_decrementPlaneDist = value; }
    }
    // Use this for initialization
    void Start () {
        if (width != null)
            width.text = "width = " + rect.sizeDelta.x;
        if (height != null)
            height.text = "height = " + rect.sizeDelta.y;
    }
	
	void Update()
    {
        if(m_incrementWidth)
        {
            IncreaseWidth(Time.deltaTime * speed);
        }
        if (m_decrementWidth)
        {
            IncreaseWidth(-Time.deltaTime * speed);
        }
        if (m_incrementHeight)
        {
            IncreaseHeight(Time.deltaTime * speed);
        }
        if (m_decrementHeight)
        {
            IncreaseHeight(-Time.deltaTime * speed);
        }

        if (m_incrementPlaneDist)
        {
            IncreasePlaneDist(Time.deltaTime * speed);
        }
        if (m_decrementPlaneDist)
        {
            IncreasePlaneDist(-Time.deltaTime * speed);
        }
    }

    // Update is called once per frame
    public void IncreaseWidth(float amount)
    {
        rect.sizeDelta = new Vector2(rect.sizeDelta.x + amount, rect.sizeDelta.y);
        if (width != null)
            width.text = "width = " + rect.sizeDelta.x;
    }

    public void IncreaseHeight(float amount)
    {
        rect.sizeDelta = new Vector2(rect.sizeDelta.x, rect.sizeDelta.y+amount);
        if (height != null)
            height.text = "height = " + rect.sizeDelta.y;
    }

    public void IncreasePlaneDist(float amount)
    {
        canvas.planeDistance += amount;
        if (height != null)
            canvasDist.text = "canvasDist = " + canvas.planeDistance;
    }
}
