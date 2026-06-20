using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Content.Interaction;

public class StreamerBotMovement : MonoBehaviour
{
    // Adjust speed in the Unity Inspector
	public float moveSpeed = 5.0f; 
	public Rigidbody rb;
    public Slider accelerator;
    public Slider torque;
    public Toggle power;

    public XRKnob boatwheel;
    public XRJoystick joystick;
    //public XRPushButton onbutton;
    public XRLever onlever;

    // This method handles your incoming UDP string
    public void ParseUDPMessage(string message)
	{
        string[] fragment = message.Split(" ");
        Debug.Log(fragment+" "+fragment[0] + " " + fragment[1]);
        
        if (message.StartsWith("!accel")){
            //string number = message.Replace("accel:","").Trim().ToLower();
            string number = fragment[1];
            if (float.TryParse(number, out float result))
            {
                Debug.Log("success accel!"+ result.ToString());
                accelerator.value = result;

                joystick.value = new Vector2 (0, result);
            }
        }

        if (message.StartsWith("!turn")){
            //string number = message.Replace("turn:","").Trim().ToLower();
            string number = fragment[1];
            if (float.TryParse(number, out float result)) {
                Debug.Log("success turn!" + result.ToString());
                torque.value = result;

                boatwheel.value = result;
            }
            
        }
        if (message.StartsWith("!power"))
        {

            //string msg = message.Replace("turn:","").Trim().ToLower();
            //bool b = message.Contains("on");

            //power.isOn = b;
            //Debug.Log("success?");
            string msg = fragment[1];
            switch (msg)
            {
                case "on":
                    //power.isOn = true;
                    //onbutton.toggleValue = true;
                    onlever.value = true;
                    break;
                case "off":
                    //power.isOn = false;
                    //onbutton.toggleValue = false;
                    onlever.value = false;
                    break;


            }

        }

		// if (message.StartsWith("jump")){
		// 	rb.AddForce(Vector3.left * moveSpeed);
		// 	//transform.Translate(Vector3.left * moveSpeed);
		// 	//transform.Rotate(Vector3.left * moveSpeed);
		// }
        // // Check if the message is a movement command
        // if (message.StartsWith("move:"))
        // {
        //     // Remove "move:" to get just the direction (e.g., "left", "right")
        //     string direction = message.Replace("move:", "").Trim().ToLower();

        //     ExecuteMovement(direction);
        // }
    }

    private void ExecuteMovement(string direction)
    {
        switch (direction)
        {
            case "left":
                transform.Translate(Vector3.left * moveSpeed);
                break;
            case "right":
                transform.Translate(Vector3.right * moveSpeed);
                break;
            case "up":
                transform.Translate(Vector3.forward * moveSpeed); // Use Vector3.up for 2D games
                break;
            case "down":
                transform.Translate(Vector3.back * moveSpeed);    // Use Vector3.down for 2D games
                break;
            default:
                // Ignores exclamation commands that aren't directions (like !hello)
                break;
        }
    }
}
