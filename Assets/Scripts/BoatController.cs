using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Content.Interaction;

//This is an diagetic controller for Boat
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

    public GenerateLevel level;
    BoatVoteEffect voteEffect;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        voteEffect = GetComponent<BoatVoteEffect>();
    }

    // Update is called once per frame
    void Update()
    {
        if (voteEffect == null)
            voteEffect = GetComponent<BoatVoteEffect>();

        powerstatus = onlever.value;
        if (voteEffect != null && voteEffect.BlocksPowerOn)
            powerstatus = false;

        float processedInput = (boatwheel.value * 2f) - 1f;
        if (voteEffect != null && voteEffect.InvertSteering)
            processedInput = -processedInput;

        if (powerstatus) {
            rb.AddTorque(Vector3.up * (processedInput * maxturn), ForceMode.Acceleration);
            rb.AddForce(Vector3.forward * (joystick.value.y * maxaccel),ForceMode.VelocityChange);
        }
    }

    public void changePower() {
        powerstatus = !powerstatus;
        //power.isOn = powerstatus;
        //onbutton.toggleValue = powerstatus;
        onlever.value = powerstatus;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name.Contains("CubePoints")) {
            level.addscore();
            Destroy(collision.gameObject);
        }
    }

}
