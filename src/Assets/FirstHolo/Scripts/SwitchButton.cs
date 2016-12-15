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

public class SwitchButton : Button {

    public bool toggleState
    {
        set
        {
            m_toggleState = value;

            if(m_toggleState)
               ((Image)targetGraphic).sprite = onIcon;
            else
                ((Image)targetGraphic).sprite = offIcon;
        }
        get
        {
            return m_toggleState;
        }
    }

    [SerializeField]
    private bool m_toggleState = false;

    public Sprite offIcon;
    public Sprite onIcon;

   // public Image icon;
}
