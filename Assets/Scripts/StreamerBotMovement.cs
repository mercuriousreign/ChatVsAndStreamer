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
    BoatVoteEffect voteEffect;

    BoatVoteEffect ResolveVoteEffect()
    {
        if (voteEffect != null)
            return voteEffect;

        BoatController boat = null;
        if (rb != null)
        {
            BoatController onHull = rb.GetComponent<BoatController>();
            if (onHull != null && onHull.isActiveAndEnabled)
                boat = onHull;
        }

        if (boat == null)
            boat = FindAnyObjectByType<BoatController>();

        if (boat == null)
        {
            Debug.LogWarning("[StreamerBotMovement] No BoatController for vote effects.");
            return null;
        }

        voteEffect = boat.GetComponent<BoatVoteEffect>();
        if (voteEffect == null)
            voteEffect = boat.gameObject.AddComponent<BoatVoteEffect>();
        return voteEffect;
    }

    // This method handles your incoming UDP string
    public void ParseUDPMessage(string message)
	{
        Debug.Log(message);
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
            string msg = fragment[1];
            if (msg == "on")
            {
                BoatVoteEffect active = ResolveVoteEffect();
                if (active != null && active.BlocksPowerOn)
                    return;
            }

            switch (msg)
            {
                case "on":
                    onlever.value = true;
                    break;
                case "off":
                    onlever.value = false;
                    break;
            }
        }


        if (message.StartsWith("!vote")) {
            string msg = message.Substring(5).Trim().ToLowerInvariant().Replace(" ", "");
            if (msg == "kill")
            {
                this.gameObject.SetActive(false);
                return;
            }

            BoatVoteEffect effect = ResolveVoteEffect();
            if (effect != null)
                effect.Apply(msg);
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
