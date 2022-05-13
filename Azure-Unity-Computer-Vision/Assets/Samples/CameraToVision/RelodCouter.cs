using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class RelodCouter : MonoBehaviour
{
    int count = 0;
    // Start is called before the first frame update
    void Start()
    {
        print($"this scene has been reloded {count++} times");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ResetScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
