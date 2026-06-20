using UnityEditor;
using UnityEngine;

public class RiverFlowTrigger : MonoBehaviour

{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float flowspeed = 3;

    public Vector3 direction = Vector3.forward;
    public float directionlength = 2f;
    public BoxCollider col;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnValidate()
    {
        col = this.gameObject.GetComponent<BoxCollider>();
       
    }


    void OnDrawGizmos()
    {
        // Draws a green directional arrow stemming from the object in the Scene Window
        Gizmos.color = Color.green;
        Gizmos.DrawRay(this.transform.position, direction.normalized * flowspeed);
        Gizmos.color = Color.aquamarine;
        Gizmos.DrawWireCube(this.transform.position,col.size);
        //Handles.color = Color.blue;
        //Handles.Label( this.transform.position,flowspeed.ToString());

    }

    private void OnTriggerStay(Collider other)
    {

        Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();
        rb.AddForce(direction * flowspeed);
        //print("weiee trigger " + other.gameObject.name);
        //if (other.CompareTag("waterobj"))
        //{
        //    Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();
        //    //rb.position += direction * flowspeed;
        //    rb.AddForce(direction * flowspeed);
        //    print("weiee trigger " + other.gameObject.name);
        //}
    }
}
