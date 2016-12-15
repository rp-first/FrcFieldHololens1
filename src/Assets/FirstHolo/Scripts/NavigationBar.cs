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
using System.Collections.Generic;
using UnityEngine.Events;

public class NavigationBar : MonoBehaviour {

    public class NavLocation
    {
        public string name;
        public UnityAction action;
    }
    public NavigationButton defaultCell;
    public RectTransform defaultSpacer;
    public RectTransform cellContainer;
    private List<NavLocation> m_locations = new List<NavLocation>();

    private List<NavigationButton> cells = new List<NavigationButton>();
    private List<RectTransform> spacers = new List<RectTransform>();
    public NavLocation current
    {
        get
        {
            if (m_locations.Count != 0)
                return m_locations[m_locations.Count - 1];
            else
                return null;
        }
    }
    public bool pop()
    {
        if (m_locations.Count != 0)
        {
            m_locations.RemoveAt(m_locations.Count - 1);
            Redraw();
            return true;
        }
        else
            return false;
    }
    public void SetCurrentPath(string name, UnityAction function) {
        //check the current path
        if(m_locations.Count > 0)
        {
            //if @current path already do nothing
            if (m_locations[m_locations.Count - 1].name.Equals(name))
                return;
        }

        NavLocation location = m_locations.Find((x) => x.name.Equals(name));
        if (location != null)
        {
            int index = m_locations.IndexOf(location);
            m_locations.RemoveRange(index + 1, m_locations.Count - (index +1));
        }
        else {
            location = new NavLocation();
            location.name= name;
            location.action = function;
            m_locations.Add(location);
        }

        Redraw();
    }


  
    private void Redraw()
    {

        // don't draw if we don't have a cell
        if (defaultCell == null) return;

        //if (cells == null)
        //    cells = new NavigationButton[m_locations.Count];

        for (int i = 0; i < m_locations.Count; i++)
        {
            NavigationButton cell;
            if (i >= cells.Count)
                cells.Add(null);
            cell = cells[i];

            cells[i] = drawCell(ref cell, i);
            cell.transform.SetParent(cellContainer.transform, false);
            cell.transform.localScale = defaultCell.transform.localScale;
            cell.transform.localRotation = Quaternion.identity;

            //Add a spacer
            if(i < m_locations.Count-1)
            {
                RectTransform spacer;
                if (i >= spacers.Count)
                    spacers.Add(null);

                spacer = spacers[i];

                spacers[i] = drawSpacer(ref spacer, i);
                spacer.transform.SetParent(cellContainer.transform, false);
                //spacer.transform.localScale = defaultCell.transform.localScale;
                spacer.transform.localRotation = Quaternion.identity;
            }

        }
        for (int i = m_locations.Count; i < cells.Count; i++)
        {
            cells[i].gameObject.SetActive(false);
        }

        for (int i = m_locations.Count-1; i < spacers.Count; i++)
        {
            spacers[i].gameObject.SetActive(false);
        }

        //foreach (Transform child in buttonContainer.transform)
        //    GameObject.Destroy(child.gameObject);
        //foreach (NavigationButton button in m_buttons)
        //    button.transform.parent = buttonContainer;
    }

    public NavigationButton drawCell(ref NavigationButton cell, int index)
    {
        NavLocation item = m_locations[index];
        if (cell == null)
        {
       
            cell = (NavigationButton)Instantiate(defaultCell);
            cell.gameObject.SetActive(true);
        }
        else
        {
            cell.select.onClick.RemoveAllListeners();
        }

        cell.gameObject.SetActive(true);

        cell.text.text = item.name;
        cell.transform.SetParent(defaultCell.transform.parent, false);
        //cell.transform.localPosition = new Vector3(cell.transform.localPosition.x, cell.transform.localPosition.y, 0);// z value is getting warped for some reason
        //cell.transform.localScale = defaultCell.transform.localScale;

        cell.select.onClick.AddListener(() => SetCurrentPath(item.name,item.action));
        cell.select.onClick.AddListener(item.action);

        return cell;
    }

    // right now doesn't do anything special with the index
    public RectTransform drawSpacer(ref RectTransform cell, int index)
    {
        NavLocation item = m_locations[index];
        if (cell == null)
        {

            cell = (RectTransform)Instantiate(defaultSpacer);
            cell.gameObject.SetActive(true);
        }
 

        cell.gameObject.SetActive(true);

        cell.transform.SetParent(defaultSpacer.transform.parent, false);
        cell.transform.localScale = defaultSpacer.transform.localScale;

        return cell;
    }
}
