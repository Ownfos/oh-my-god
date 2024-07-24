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

    [SerializeField] private List<Transform> spawnPoints; // 유니티 에디터에서 지정할 수 있도록 설정
    [SerializeField] private RankingSystem rankingSystem;
    [SerializeField] private TimeoutController timeoutController;

    private PlayerController playerController;

    public GameObject worshiperPrefab; // 개별 NPC(Worshiper) 프리팹
    public GameObject neutralWorshiperGroupPrefab; // 집단(NeutralWorshiperGroup) 프리팹
    [SerializeField] private BoxCollider2D mapCollider; // 맵의 BoxCollider2D 컴포넌트
    public int areaSizeX = 18; // 18x12 직사각형 구역 크기 (가로)
    public int areaSizeY = 12; // 18x12 직사각형 구역 크기 (세로)

    private List<NeutralWorshiperGroup> worshiperGroups = new List<NeutralWorshiperGroup>();

    public BoxCollider2D MapCollider { get => mapCollider; set => mapCollider = value; }

    public GameObject jimmyPrefab;
    public GameObject peterPrefab;
    public GameObject mustardPrefab;
    public GameObject malonePrefab;
    public GameObject sandraPrefab;
    public GameObject amyPrefab;
    public GameObject juliaPrefab;
    public GameObject elinaPrefab;

    private List<GameObject> enemyPrefabs;

    void Start()
    {
        Debug.Log("Game Started");

        BGMController.Instance.SwitchToMainGameBGM();

        enemyPrefabs = new List<GameObject>
        {
            jimmyPrefab,
            peterPrefab,
            mustardPrefab,
            malonePrefab,
            sandraPrefab,
            amyPrefab,
            juliaPrefab,
            elinaPrefab
        };

        MoveObjectsToRandomPositions();

        playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            playerController.SetCanMove(false); // 플레이어 이동 고정
        }

        if (MapCollider == null || worshiperPrefab == null || neutralWorshiperGroupPrefab == null)
        {
            Debug.LogError("Map Collider 또는 Prefab이 설정되지 않았습니다.");
            return;
        }

        StartCoroutine(StartSequence());
    }

    void MoveObjectsToRandomPositions()
    {
        if (spawnPoints.Count < 9)
        {
            Debug.LogError("스폰 포인트가 충분하지 않습니다.");
            return;
        }

        // 스폰 포인트를 랜덤하게 섞기
        List<Transform> shuffledSpawnPoints = new List<Transform>(spawnPoints);
        Shuffle(shuffledSpawnPoints);

        // 8명의 적을 생성하여 스폰 포인트에 배치
        for (int i = 0; i < enemyPrefabs.Count; i++)
        {
            GameObject enemy = Instantiate(enemyPrefabs[i], shuffledSpawnPoints[i].position, Quaternion.identity);
            enemy.name = enemyPrefabs[i].name;

            var propagationController = enemy.GetComponent<WorshipPropagationController>();
            if (propagationController != null)
            {
                rankingSystem.AddCompetitor(propagationController); // RankingSystem에 등록
            }
        }
    }

    void Shuffle<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    IEnumerator StartSequence()
    {
        // A와 B를 동시 진행
        Coroutine npcGeneration = StartCoroutine(GenerateWorshiperGroups());
        Coroutine countdown = StartCoroutine(FadeAndCountdown());

        yield return npcGeneration; // NPC 생성 완료 대기
        yield return countdown; // 카운트다운 완료 대기

        // 플레이어 이동 허용
        if (playerController != null)
        {
            playerController.SetCanMove(true); // 플레이어 이동 허용
        }

        StartGame();
    }

    IEnumerator GenerateWorshiperGroups()
    {
        Vector2 minBounds = MapCollider.bounds.min;
        Vector2 maxBounds = MapCollider.bounds.max;
        Debug.Log($"맵 경계: {minBounds} - {maxBounds}");

        int numAreasX = Mathf.FloorToInt((maxBounds.x - minBounds.x) / areaSizeX);
        int numAreasY = Mathf.FloorToInt((maxBounds.y - minBounds.y) / areaSizeY);

        for (int areaY = 0; areaY < numAreasY; areaY++)
        {
            for (int areaX = 0; areaX < numAreasX; areaX++)
            {
                Vector2 areaPosition = new Vector2(minBounds.x + areaX * areaSizeX, minBounds.y + areaY * areaSizeY);
                Debug.Log($"구역 {areaY * numAreasX + areaX + 1} 시작 위치: {areaPosition}");
                GenerateWorshipersInArea(areaPosition);
            }
        }

        yield break;
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
        float randomX = Random.Range(areaPosition.x, areaPosition.x + areaSizeX);
        float randomY = Random.Range(areaPosition.y, areaPosition.y + areaSizeY);
        return new Vector2(randomX, randomY);
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

            // Rigidbody2D의 중력 영향 제거
            Rigidbody2D rb = worshiper.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = 0; // 중력 영향 제거
            }

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

    IEnumerator FadeAndCountdown()
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
        yield return StartCoroutine(StartCountdown());
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

        // 게임이 시작되었으니 제한시간도 줄어들기 시작해야 함
        timeoutController.StartTimer();
    }

    void StartGame()
    {
        Debug.Log("Game Started");
    }
}
