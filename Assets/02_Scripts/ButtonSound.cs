using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonSound : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartNextScene()
    {
        GameManager.Instance.StartNextScene();
    }
    public void StartScene()
    {
        GameManager.Instance.StartScene();
    }

    public void EndGame()
    {
        GameManager.Instance.EndGame();
    }
}
