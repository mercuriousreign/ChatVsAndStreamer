using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Scene s;
    public string scenename;
    public Scene sceneobj;


    public void SceneChangeFunction(string sceneid) { 
        SceneManager.LoadScene(sceneid);
    }

    public void SceneChangeFunctionObj(Scene sceneid)
    {
        SceneManager.LoadScene(sceneid.name);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Boat2") {
            SceneManager.LoadScene(scenename);
        }
    }

}
