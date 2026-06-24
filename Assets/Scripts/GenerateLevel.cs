using Unity.VectorGraphics;
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

    private void Awake()
    {
        for (float i = 0; i <= loop; i++) {
            int randomrock = Random.Range(0, rocks.Length);
            int randomlocationX = (int)Random.Range(0, size.x); 
            int randomlocationY = (int)Random.Range(0, size.y);
            GameObject.Instantiate(rocks[randomrock], new Vector3(randomlocationX, air, randomlocationY), Random.rotation);
            if (rocks[randomrock].name == "CubePoints")
            {
                toscore++;
            }

        }

        scoring.text = "Green cubes to get is " + toscore.ToString();
    }

    public void addscore() {
        toscore--;
        scoring.text = "Green cubes to get is " + toscore.ToString();
        if (toscore <= 0) {
            SceneManager.LoadScene("You Win");
        }

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }
}


    

