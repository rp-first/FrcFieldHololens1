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
using System;

public class PopupNotification : MonoBehaviour {
    [Serializable]
    public enum DIALOG_TYPE { SUCCESS, FAILURE, WARNING }
    public Text text;
    public Image icon;
    public Sprite success;
    public Sprite failure;
    public Sprite warning;
    public float duration= 3f;
    public DIALOG_TYPE type;
    private Coroutine m_HideRoutine = null;
    public void ShowDialog()
    {
        switch (type)
        {
            case DIALOG_TYPE.FAILURE:
                icon.sprite = failure;
                break;
            case DIALOG_TYPE.SUCCESS:
                icon.sprite = success;
                break;
            case DIALOG_TYPE.WARNING:
                icon.sprite = warning;
                break;
        }
        this.gameObject.SetActive(true);
        if (m_HideRoutine != null)
            StopCoroutine(m_HideRoutine);
        m_HideRoutine = StartCoroutine(HideAfterDuration(duration));
    }

    private IEnumerator HideAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        this.gameObject.SetActive(false);
        m_HideRoutine = null;
    }
}
