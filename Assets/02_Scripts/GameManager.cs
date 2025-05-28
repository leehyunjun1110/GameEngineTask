using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private static GameManager instance = null;

    [SerializeField]
    private AudioSource audioSource;

    public TMP_InputField inputField;

    public int stageNumber = 1; // tutorialScene Number

    public int score;

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

    public void SetCurrentScore(int stage)
    {
        if (stage == stageNumber)
            score++;
        else
        {
            if (stage == -1)
            {
                score = 0;
            }
            else
            {
                score = 1;
            }
            stageNumber = SceneManager.GetActiveScene().buildIndex;
        }
    }

    public void GetCurrentScore()
    {
        Debug.Log($"저장됨 {stageNumber}, {score}");
        StageResultSaver.SaveStage(stageNumber, score);
    }

    public void StartNextScene()
    {
        audioSource.Play();
        string playerName = inputField.text;
        if (string.IsNullOrEmpty(playerName)) 
        {
            Debug.Log("플레이어 이름을 입력하세요");
            return;
        }

        PlayerPrefs.SetString("PlayerName", playerName);
        PlayerPrefs.Save();

        Debug.Log($"플레이어 이름 저장 됨 : {playerName}");

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

    public void PlayAudio()
    {
        audioSource.Play();
    }

    public void EndGame()
    {
        audioSource.Play();
        Application.Quit();
    }
}
