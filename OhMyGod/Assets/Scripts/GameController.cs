using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

public class GameController : MonoBehaviour
{
    public Text countdownText;
    public FadeController fadeController;
    public float numberScaleStart = 0.5f;
    public float numberScaleEnd = 1.5f;

    private PlayerMovement playerMovement;

    public GameObject neutralWorshiperGroupPrefab; // NeutralWorshiperGroup 프리팹
    public GameObject worshiperPrefab; // 개별 NPC(Worshiper) 프리팹
    public BoxCollider2D mapCollider; // 맵의 BoxCollider2D 컴포넌트
    public int areaSize = 5; // 5x5 정사각형 구역 크기

    private List<NeutralWorshiperGroup> worshiperGroups = new List<NeutralWorshiperGroup>();

    void Start()
    {
        Debug.Log("Game Started");

        playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.SetCanMove(false); // 입력 비활성화
        }

        if (mapCollider == null || neutralWorshiperGroupPrefab == null || worshiperPrefab == null)
        {
            Debug.LogError("Map Collider 또는 Prefab이 설정되지 않았습니다.");
            return;
        }

        GenerateWorshiperGroups();
        StartCoroutine(StartSequence());
    }

    void GenerateWorshiperGroups()
    {
        // 맵의 BoxCollider2D 크기 계산
        Vector2 mapSize = mapCollider.size;
        Vector2 mapPosition = (Vector2)mapCollider.transform.position - mapCollider.size / 2;

        Debug.Log($"맵 크기 - 가로: {mapSize.x}, 세로: {mapSize.y}");

        int numAreasX = Mathf.FloorToInt(mapSize.x / areaSize);
        int numAreasY = Mathf.FloorToInt(mapSize.y / areaSize);

        for (int areaY = 0; areaY < numAreasY; areaY++)
        {
            for (int areaX = 0; areaX < numAreasX; areaX++)
            {
                Vector2 areaPosition = new Vector2(mapPosition.x + areaX * areaSize, mapPosition.y + areaY * areaSize);
                Debug.Log($"구역 {areaY * numAreasX + areaX + 1} 시작 위치: {areaPosition}");
                GenerateWorshipersInArea(areaPosition, areaX, areaY);
            }
        }
    }

    void GenerateWorshipersInArea(Vector2 areaPosition, int areaX, int areaY)
    {
        int remainingWorshipers = 5;
        int groupId = 1;

        while (remainingWorshipers > 0)
        {
            int groupSize = GetRandomGroupSize(remainingWorshipers);
            List<GameObject> groupMembers = new List<GameObject>();

            // 그룹 오브젝트 생성 및 NeutralWorshiperGroup 스크립트 추가
            GameObject groupObject = Instantiate(neutralWorshiperGroupPrefab, areaPosition + new Vector2(Random.Range(0f, areaSize), Random.Range(0f, areaSize)), Quaternion.identity);
            groupObject.name = $"NeutralWorshiperGroup_{areaX}_{areaY}_{groupId}";

            for (int i = 0; i < groupSize; i++)
            {
                Vector2 spawnPosition = groupObject.transform.position;
                Debug.Log($"NPC 스폰 위치: {spawnPosition}");
                GameObject worshiper = Instantiate(worshiperPrefab, spawnPosition, Quaternion.identity);
                worshiper.name = $"Worshiper_{areaX}_{areaY}_{groupId}_{i + 1}";

                // WorshiperController 스크립트 추가 및 FollowTarget 설정
                WorshiperController worshiperController = worshiper.GetComponent<WorshiperController>();
                if (worshiperController == null)
                {
                    worshiperController = worshiper.AddComponent<WorshiperController>();
                }
                worshiperController.FollowTarget = groupObject;
                groupMembers.Add(worshiper);
            }

            NeutralWorshiperGroup group = groupObject.GetComponent<NeutralWorshiperGroup>();
            group.InitializeGroup(groupMembers);

            worshiperGroups.Add(group);
            remainingWorshipers -= groupSize;
            groupId++;
        }
    }

    int GetRandomGroupSize(int remainingWorshipers)
    {
        if (remainingWorshipers <= 4)
        {
            return remainingWorshipers;
        }

        float randomValue = Random.value;
        if (randomValue <= 0.50f) return 1;
        if (randomValue <= 0.75f) return 2;
        if (randomValue <= 0.90f) return 3;
        return 4;
    }

    IEnumerator StartSequence()
    {
        if (fadeController == null)
        {
            Debug.LogError("fadeController가 설정되지 않았습니다.");
            yield break;
        }

        if (countdownText == null)
        {
            Debug.LogError("countdownText가 설정되지 않았습니다.");
            yield break;
        }

        Debug.Log("fadeController와 countdownText가 올바르게 설정되었습니다.");
        yield return StartCoroutine(fadeController.FadeIn());

        Debug.Log("Fade In completed. Starting countdown.");
        StartCoroutine(StartCountdown());
    }

    IEnumerator StartCountdown()
    {
        countdownText.gameObject.SetActive(true);
        Debug.Log("Countdown started.");

        for (int i = 3; i > 0; i--)
        {
            countdownText.text = i.ToString();
            countdownText.transform.localScale = new Vector3(numberScaleStart, numberScaleStart, numberScaleStart);
            countdownText.transform.DOScale(numberScaleEnd, 1f).SetEase(Ease.OutBounce);
            Debug.Log("Countdown: " + i);
            yield return new WaitForSeconds(1);
        }

        countdownText.text = "Start!";
        countdownText.transform.localScale = new Vector3(numberScaleStart, numberScaleStart, numberScaleStart);
        countdownText.transform.DOScale(numberScaleEnd, 1f).SetEase(Ease.OutBounce);
        yield return new WaitForSeconds(1);

        countdownText.gameObject.SetActive(false);
        Debug.Log("Countdown completed. Starting game.");

        if (playerMovement != null)
        {
            playerMovement.SetCanMove(true); // 입력 활성화
        }

        StartGame();
    }

    void StartGame()
    {
        Debug.Log("Game Started");
    }
}
