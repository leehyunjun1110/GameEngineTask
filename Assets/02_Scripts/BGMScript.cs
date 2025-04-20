using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BGMScript : MonoBehaviour
{
    [SerializeField]
    private static BGMScript instance = null;

    private AudioSource audioSource;

    [Header("오디오 클립 목록")]
    public AudioClip mainMenuClip;
    public AudioClip tutorialClip;
    public AudioClip world1Clip;
    public AudioClip world2Clip;
    public AudioClip world3Clip;
    public AudioClip world4Clip;
    public AudioClip endingClip;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public static BGMScript Instance
    {
        get
        {
            return instance;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ChangeBGMByScene(scene.name);
    }

    void ChangeBGMByScene(string sceneName)
    {
        AudioClip selectedClip = null;

        switch (sceneName)
        {
            case "StartScene":
                selectedClip = mainMenuClip;
                break;

            case "World1":
                selectedClip = world1Clip;
                break;

            case "World2":
                selectedClip = world2Clip;
                break;

            case "World3":
                selectedClip = world3Clip;
                break;

            case "World4":
                selectedClip = world4Clip;
                break;

            case "End":
                selectedClip = endingClip;
                break;
        }

        if (selectedClip != null && audioSource.clip != selectedClip)
        {
            audioSource.clip = selectedClip;
            audioSource.loop = true;
            audioSource.Play();
        }
    }
}