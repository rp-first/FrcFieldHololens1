using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimpleVideo : MonoBehaviour {

    public RawImage video;

    public void Start()
    {
        UWPServer.Publisher.Subscribe(0xBEEF, onUpdatedValue);
    }

    void onUpdatedValue(ValueItem vi)
    {
        if (video != null)
        {
            Debug.Log("Update texture");
            Texture2D src = new Texture2D(0, 0);
            src.LoadRawTextureData((byte[])vi.value);
            video.texture = src;
        }
    }
}
