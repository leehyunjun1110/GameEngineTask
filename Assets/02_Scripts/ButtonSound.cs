using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonSound : MonoBehaviour
{
    public void StartNextScene()
    {
        GameManager.Instance.StartNextScene();
    }
    public void StartScene()
    {
        GameManager.Instance.StartScene();
    }

    public void JustSoundPlay()
    {
        GameManager.Instance.PlayAudio();
    }
    public void EndGame()
    {
        GameManager.Instance.EndGame();
    }
}
