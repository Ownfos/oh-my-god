using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class GameController : MonoBehaviour
{
    public Text countdownText;
    public FadeController fadeController;
    public float numberScaleStart = 0.5f;
    public float numberScaleEnd = 1.5f;

    private PlayerMovement playerMovement;

    public GameObject worshiperPrefab; // 개별 NPC(Worshiper) 프리팹
    public GameObject neutralWorshiperGroupPrefab; // 집단(NeutralWorshiperGroup) 프리팹
    public BoxCollider2D mapCollider; // 맵의 BoxCollider2D 컴포넌트
    public int areaSizeX = 18; // 18x12 직사각형 구역 크기 (가로)
    public int areaSizeY = 12; // 18x12 직사각형 구역 크기 (세로)

    private List<NeutralWorshiperGroup> worshiperGroups = new List<NeutralWorshiperGroup>();

    void Start()
    {
        Debug.Log("Game Started");

        playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.SetCanMove(false); // 입력 비활성화
        }

        if (mapCollider == null || worshiperPrefab == null || neutralWorshiperGroupPrefab == null)
        {
            Debug.LogError("Map Collider 또는 Prefab이 설정되지 않았습니다.");
            return;
        }

        GenerateWorshiperGroups();
        StartCoroutine(StartSequence());
    }

    void GenerateWorshiperGroups()
    {
        Vector2 initialPosition = new Vector2(-7, -125); // 첫 구역 시작 위치를 고정
        Debug.Log($"첫 구역 시작 위치: {initialPosition}");

        int numAreasX = Mathf.FloorToInt(180 / areaSizeX); // 맵의 가로 크기 180
        int numAreasY = Mathf.FloorToInt(120 / areaSizeY); // 맵의 세로 크기 120

        for (int areaY = 0; areaY < numAreasY; areaY++)
        {
            for (int areaX = 0; areaX < numAreasX; areaX++)
            {
                Vector2 areaPosition = new Vector2(initialPosition.x + areaX * areaSizeX, initialPosition.y + areaY * areaSizeY);
                Debug.Log($"구역 {areaY * numAreasX + areaX + 1} 시작 위치: {areaPosition}");
                GenerateWorshipersInArea(areaPosition);
            }
        }
    }

    void GenerateWorshipersInArea(Vector2 areaPosition)
    {
        // 한 영역 내에 NeutralWorshiperGroup 생성
        GameObject groupObject = Instantiate(neutralWorshiperGroupPrefab, GetRandomPositionInArea(areaPosition), Quaternion.identity);
        groupObject.name = $"NeutralWorshiperGroup_{areaPosition.x}_{areaPosition.y}";

        NeutralWorshiperGroup group = groupObject.GetComponent<NeutralWorshiperGroup>();
        if (group == null)
        {
            Debug.LogError("NeutralWorshiperGroup 컴포넌트를 찾을 수 없습니다.");
            return;
        }

        // 랜덤한 수의 Worshiper 생성 (1-5)
        int worshiperCount = Random.Range(1, 6);
        List<GameObject> groupMembers = GenerateWorshiperMembers(areaPosition, worshiperCount, groupObject);

        group.InitializeGroup(groupMembers);
        worshiperGroups.Add(group);
    }

    Vector2 GetRandomPositionInArea(Vector2 areaPosition)
    {
        return areaPosition + new Vector2(Random.Range(0f, areaSizeX), Random.Range(0f, areaSizeY));
    }

    List<GameObject> GenerateWorshiperMembers(Vector2 areaPosition, int count, GameObject groupObject)
    {
        List<GameObject> groupMembers = new List<GameObject>();

        for (int i = 0; i < count; i++)
        {
            Vector2 spawnPosition = GetRandomPositionInArea(areaPosition);
            Debug.Log($"NPC 스폰 위치: {spawnPosition}");
            GameObject worshiper = Instantiate(worshiperPrefab, spawnPosition, Quaternion.identity);
            worshiper.transform.SetParent(groupObject.transform); // 그룹 오브젝트의 child로 설정

            // WorshiperController 스크립트 추가 및 FollowTarget 설정
            WorshiperController worshiperController = worshiper.GetComponent<WorshiperController>();
            if (worshiperController == null)
            {
                worshiperController = worshiper.AddComponent<WorshiperController>();
            }
            worshiperController.FollowTarget = groupObject; // FollowTarget 설정

            groupMembers.Add(worshiper);
        }

        return groupMembers;
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
