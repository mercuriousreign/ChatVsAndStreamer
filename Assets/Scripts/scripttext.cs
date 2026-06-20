using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class scripttext : MonoBehaviour


{
    public TextMeshProUGUI text;
    public Slider slide;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        text.text = slide.value.ToString();
    }
}
