using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TimeoutController : MonoBehaviour
{
    [SerializeField] private float maximumPlaytimeInSecs = 180f;
    [SerializeField] private Text remainingTimeText;

    public UnityEvent OnTimeout;

    private float remainingTime;
    private bool isClockRunning = false;

    private void Start()
    {
        // 최대 시간에서 정지한 상태로 대기
        remainingTime = maximumPlaytimeInSecs;
        UpdateTimerUI();

        // 테스트용으로 여기서 바로 타이머 시작.
        // 실제로는 GameController에서 카운트다운 끝나면 시작해야 함
        // StartTimer();

        // Debug.Log(SessionData.Instance.SelectedGod);
    }

    public void StartTimer()
    {
        isClockRunning = true;
    }

    private void Update()
    {
        if (isClockRunning)
        {
            remainingTime = Mathf.Max(remainingTime - Time.deltaTime, 0f);
            UpdateTimerUI();

            if (Mathf.Approximately(remainingTime, 0f))
            {
                OnTimeout.Invoke();
                isClockRunning = false;
            }
        }
    }

    private void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime - minutes * 60f);

        remainingTimeText.text = $"{minutes}:{seconds}";
    }
}
