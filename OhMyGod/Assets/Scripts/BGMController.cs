using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMController : MonoBehaviour
{
    [SerializeField] private AudioClip titleBgm;
    [SerializeField] private AudioClip goodGodBgm;
    [SerializeField] private AudioClip evilGodBgm;
    [SerializeField] private AudioClip weirdGodBgm;
    [SerializeField] private AudioClip gameClearBgm;

    public static BGMController Instance { get; private set; }

    [SerializeField] private AudioSource mainBGM;
    [SerializeField] private AudioSource battleBGM;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Instance.SwitchToTitleBGM();
            Destroy(gameObject);
        }
    }

    public void SwitchToTitleBGM()
    {
        battleBGM.Stop();

        mainBGM.Stop();
        mainBGM.clip = titleBgm;
        mainBGM.Play();
    }

    public void SwitchToMainGameBGM()
    {
        mainBGM.Stop();

        string godName = SessionData.Instance.SelectedGod;
        if (godName == "Good")
        {
            mainBGM.clip = goodGodBgm;
        }
        else if (godName == "Evil")
        {
            mainBGM.clip = evilGodBgm;
        }
        else
        {
            mainBGM.clip = weirdGodBgm;
        }

        mainBGM.Play();
    }

    public void SwitchToGameClearBGM()
    {
        battleBGM.Stop();

        mainBGM.Stop();
        mainBGM.clip = gameClearBgm;
        mainBGM.Play();
    }

    public void StartBattleBGM()
    {
        mainBGM.Pause();
        battleBGM.Stop();
        battleBGM.Play();
    }

    public void ResumeMainGameBGM()
    {
        battleBGM.Stop();
        mainBGM.Play();
    }
}
