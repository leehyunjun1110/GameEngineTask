using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private static GameManager instance = null;

    [SerializeField]
    private AudioSource audioSource;

    public bool cleared = false;
    void Awake()
    {
        if (null == instance)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    public static GameManager Instance
    {
        get
        {
            if (null == instance)
            {
                return null;
            }
            return instance;
        }
    }

    public void StartNextScene()
    {
        audioSource.Play();
        if (cleared == false)
            SceneManager.LoadScene("TutorialScene");
        else
            SceneManager.LoadScene("World1");
    }

    public void StartScene()
    {
        audioSource.Play();
        SceneManager.LoadScene("StartScene");
    }

    public void EndGame()
    {
        audioSource.Play();
        Application.Quit();
    }
}
