using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Content.Interaction;

public class BoatControlJoyStick : MonoBehaviour
{
    
    public XRLever onlever;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] private InputActionProperty buttonAction;
    [SerializeField] private InputActionProperty buttonAction2;

    [SerializeField] private InputActionProperty thumbstickAction;

    public Rigidbody rb;
    public GenerateLevel level;
    public BoatController controller;
    public TextMeshProUGUI audimpact;

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
        
        
        if (buttonAction.action.WasPressedThisFrame())
        {
            onlever.value = !onlever.value;
        }

        if (buttonAction2.action.WasPressedThisFrame())
        {
            controller.enabled = !controller.enabled;
            audimpact.text = "Stream audience impact is: "+ controller.enabled.ToString();
        }

        if (onlever.value) {
            rb.AddTorque(Vector3.up * (thumbstickValue.x * maxturn), ForceMode.Acceleration);
            rb.AddRelativeForce(Vector3.forward * (thumbstickValue.y * maxaccel), ForceMode.Acceleration); //used force it was ok
            
        }

    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name.Contains("CubePoints"))
        {
            level.addscore();
            Destroy(collision.gameObject);
        }
    }

}
