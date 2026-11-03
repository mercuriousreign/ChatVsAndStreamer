using UnityEngine;
using UnityEngine.InputSystem;

public class BoatControllerKB : MonoBehaviour
{
    public Rigidbody rb;
    public float maxaccel = 5f;
    public float maxturn = 10f;
    public bool power = false;

    [SerializeField] InputActionReference move;
    [SerializeField] InputActionReference jump;

    void OnEnable()
    {
        move.action.Enable();
        jump.action.Enable();
    }

    void OnDisable()
    {
        move.action.Disable();
        jump.action.Disable();
    }

    void Update()
    {
        if (jump.action.WasPressedThisFrame())
            power = !power;

        if (!power)
            return;

        Vector2 moveInput = move.action.ReadValue<Vector2>();
        rb.AddTorque(Vector3.up * (moveInput.x * maxturn), ForceMode.Acceleration);
        rb.AddRelativeForce(Vector3.forward * (moveInput.y * maxaccel), ForceMode.Acceleration);
    }
}