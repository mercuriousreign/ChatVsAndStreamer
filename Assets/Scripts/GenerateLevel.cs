using UnityEngine;

public class GenerateLevel : MonoBehaviour
{
    public GameObject[] rocks;
    public Vector2 size;
    public float air = 30f;
    public float loop = 30f;


    private void Awake()
    {
        for (float i = 0; i <= loop; i++) {
            int randomrock = Random.Range(0, rocks.Length);
            int randomlocationX = (int)Random.Range(0, size.x); 
            int randomlocationY = (int)Random.Range(0, size.y);
            GameObject.Instantiate(rocks[randomrock], new Vector3(randomlocationX, air, randomlocationY), Random.rotation);
        }

        // FIX: Changed x and y loop variables to floats to match size.x and size.y
        //for (float x = 0; x <= size.x; x++)
        //{
        //    for (float y = 0; y <= size.y; y++)
        //    {
        //        if (Random.Range(0, 10) > 5)
        //        {
        //            GameObject.Instantiate(rocks[Random.Range(0, rocks.Length)], new Vector3(x, air, y), Random.rotation);
        //        }
        //    }
        //}
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }
}


    

