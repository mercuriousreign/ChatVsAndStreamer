using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Content.Interaction;

public class BoatControlJoyStick : MonoBehaviour
{
    
    public XRLever onlever;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] private InputActionProperty buttonAction;
    [SerializeField] private InputActionProperty thumbstickAction;

    public Rigidbody rb;
    public GenerateLevel level;

    public float maxaccel = 5f;
    public float maxturn = 10f;

    void Start()
    {
        onlever.value = false;
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 thumbstickValue = thumbstickAction.action.ReadValue<Vector2>();
        //print("value of thumb "+thumbstickValue);
        
        if (buttonAction.action.WasPressedThisFrame())
        {
            onlever.value = !onlever.value;
        }

        if (onlever.value) {
            //rb.AddTorque(Vector3.up * (thumbstickValue.x * maxturn), ForceMode.Acceleration);
            rb.AddForce(Vector3.forward * (thumbstickValue * maxaccel), ForceMode.VelocityChange);
        }

    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "CubePoints")
        {
            level.addscore();
            Destroy(collision.gameObject);
        }
    }

}
