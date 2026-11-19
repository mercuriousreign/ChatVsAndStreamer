using Unity.VectorGraphics;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GenerateLevel : MonoBehaviour
{
    public GameObject[] rocks;
    public Vector2 size;
    public float air = 30f;
    public float loop = 30f;
    public int toscore =0;
    public TextMeshProUGUI scoring;
    public GameObject map;
    public StreamerBotListener listener;
    public StreamerBotMovement move;
    public List<GameObject> spawned = new List<GameObject>();
    string voteStatus = "";

    private void Awake()
    {
        if (spawned == null)
            spawned = new List<GameObject>();

        for (float i = 0; i <= loop; i++) {
            int randomrock = Random.Range(0, rocks.Length);
            int randomlocationX = (int)Random.Range(0, size.x); 
            int randomlocationY = (int)Random.Range(0, size.y);
            GameObject placed = Instantiate(rocks[randomrock], new Vector3(randomlocationX, air, randomlocationY), Random.rotation);
            spawned.Add(placed);
            if (rocks[randomrock].name == "CubePoints")
            {
                toscore++;
            }

        }

        WriteScore();
    }

    public void addscore() {
        toscore--;
        WriteScore();
        if (toscore <= 0) {
            SceneManager.LoadScene("You Win");
        }

    }

    public void SetVoteStatus(string label, float secondsRemaining)
    {
        if (string.IsNullOrEmpty(label))
            voteStatus = "";
        else if (secondsRemaining > 0f)
            voteStatus = label + " " + Mathf.CeilToInt(secondsRemaining) + "s";
        else
            voteStatus = label;

        WriteScore();
    }

    void WriteScore()
    {
        if (scoring == null)
            return;

        scoring.text = "Green cubes to get is " + toscore.ToString();
        if (!string.IsNullOrEmpty(voteStatus))
            scoring.text += "\n" + voteStatus;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }
}


    

