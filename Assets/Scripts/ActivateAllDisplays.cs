using UnityEngine;

public class ActivateAllDisplays : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Camera camOver;
    public Camera mainCam;

    
    void Start()
    {
        
        // Display.displays[0] is the primary, default display and is always ON, so start at index 1.
        // Check if additional displays are available and activate each.

        for (int i = 1; i < Display.displays.Length; i++)
        {
            Display.displays[i].Activate();
            //Screen.fullScreen = false;
        }
        Debug.Log("displays connected: " + Display.displays.Length);
        //mainCam.targetDisplay = 1;
        //camOver.targetDisplay = 2;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
