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

    // 시작한 뒤 1분 ~ 2분 구간에 15초마다 발생하는 중립 NPC 추가 생성 이벤트
    public UnityEvent OnNPCSpawnEvent;

    private float remainingTime;
    private bool isClockRunning = false;

    // 1분 ~ 2분 사이에 발생하는 이벤트가 15초마다 실행되도록 해주는 카운터.
    // 1분이 경과한 시점에 바로 한 번 실행되도록 초기 값을 높게 잡아준다.
    private float timeSinceLastNPCSpawn = 999f;
    // NPC 추가 생성 이벤트가 지금까지 몇 번 발생했는지 기록하는 카운터.
    // 지금 기획안 기준으로 총 4회 발생.
    private int numNPCSpawnEvent = 0;
    private const int MAX_NPC_SPAWN_EVENT = 4;

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

            // 이벤트 1) 시작 이후 1분 ~ 2분 구간 => 15초마다 중립 NPC 추가 생성
            if (remainingTime < maximumPlaytimeInSecs - 60 && numNPCSpawnEvent < MAX_NPC_SPAWN_EVENT)
            {
                timeSinceLastNPCSpawn += Time.deltaTime;
                if (timeSinceLastNPCSpawn >= 15f)
                {
                    timeSinceLastNPCSpawn = 0f;
                    numNPCSpawnEvent++;
                    OnNPCSpawnEvent.Invoke();
                }
            }

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
