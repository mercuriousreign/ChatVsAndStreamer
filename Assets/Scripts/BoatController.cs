using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Content.Interaction;

public class BoatController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Rigidbody rb;
    public Slider accelerator;
    public Slider torque;
    public GameObject wheel;
    public Toggle power;
    public GameObject powerobj;

    public float maxaccel = 5f;
    public float maxturn = 10f;

    public bool powerstatus = false;

    public XRKnob boatwheel;
    public XRJoystick joystick;
    public XRPushButton onbutton;
    public XRLever onlever;
    

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
    }

    // Update is called once per frame
    void Update()
    {
        powerstatus = onlever.value;
        //if (power.isOn)
        //{
        //    rb.AddTorque(Vector3.up * torque.value);

        //    rb.AddForce(transform.forward * accelerator.value);
        //}
        //if (power.isOn)
        //{
        //    rb.AddRelativeTorque(Vector3.up * torque.value);
        //    rb.AddForce(transform.forward * accelerator.value);
        //}

        //if (power.isOn) { 
        //    rb.AddTorque(Vector3.up * wheel.transform.rotation.eulerAngles.x, 0);

        //}

        float processedInput = (boatwheel.value * 2f) - 1f;

        if (powerstatus) {
            rb.AddTorque(Vector3.up * (processedInput * maxturn), ForceMode.Acceleration);
            rb.AddForce(Vector3.forward * (joystick.value.y * maxaccel),ForceMode.VelocityChange);
            print ("Joystick value"+joystick.value.y);
            
            print("actual value "+Vector3.forward * (joystick.value.y * maxaccel));
        }




    }

    public void changePower() {
        powerstatus = !powerstatus;
        //power.isOn = powerstatus;
        //onbutton.toggleValue = powerstatus;
        onlever.value = powerstatus;
    }
}
