using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonsScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleLayer(string layer)
    {
        Camera.main.cullingMask ^= 1 << LayerMask.NameToLayer(layer);
    }

    public void SendPicToPhone()
    {
       string num = "7323310002";
       Application.OpenURL("sms:" + num);
    }
}
