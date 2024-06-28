using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SessionData : MonoBehaviour
{
    public static SessionData Instance { get; private set; }
    public string SelectedGod { get; set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}