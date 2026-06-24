using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Content.Interaction;

public class BoatControllerInput : MonoBehaviour
{
    public XRKnob boatwheel;
    public XRJoystick joystick;
    public XRLever onlever;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] private InputActionProperty buttonAction;
    [SerializeField] private InputActionProperty thumbstickAction;
    void Start()
    {
        onlever.value = false;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 thumbstickValue = thumbstickAction.action.ReadValue<Vector2>();
        //print("value of thumb "+thumbstickValue);
        boatwheel.value = thumbstickValue.y;
        if (buttonAction.action.WasPressedThisFrame()) { 
            onlever.value = !onlever.value;
        }
        joystick.value = new Vector2(0, thumbstickValue.y);
        
    }
}
