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

    [SerializeField] private RankingSystem rankingSystem;
    [SerializeField] private TimeoutController timeoutController;
    [SerializeField] private Collider2D playerPropagationRange; // 카운트다운동안 비활성화하기 위한 레퍼런스

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
    private List<GameObject> spawnedEnemies = new List<GameObject>();

    private float detectionRadius = 30f; 

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
        Vector2 minBounds = MapCollider.bounds.min;
        Vector2 maxBounds = MapCollider.bounds.max;

        for (int i = 0; i < enemyPrefabs.Count; i++)
        {
            Vector2 spawnPosition = GetValidSpawnPosition(minBounds, maxBounds);
            //GameObject enemy = Instantiate(enemyPrefabs[i], spawnPosition, Quaternion.identity);
            // enemy.name = enemyPrefabs[i].name;
            enemyPrefabs[i].transform.position = spawnPosition;
            spawnedEnemies.Add(enemyPrefabs[i]);

            var propagationController = enemyPrefabs[i].GetComponent<WorshipPropagationController>();
            if (propagationController != null)
            {
                rankingSystem.AddCompetitor(propagationController); // RankingSystem에 등록
            }
        }
    }

    Vector2 GetValidSpawnPosition(Vector2 minBounds, Vector2 maxBounds)
    {
        Vector2 spawnPosition;
        bool validPosition = false;

        while (!validPosition)
        {
            spawnPosition = new Vector2(
                Random.Range(minBounds.x, maxBounds.x),
                Random.Range(minBounds.y, maxBounds.y)
            );

            Collider2D[] colliders = Physics2D.OverlapCircleAll(spawnPosition, detectionRadius);
            validPosition = true;

            foreach (var collider in colliders)
            {
                if (collider.gameObject.CompareTag("Player") || spawnedEnemies.Contains(collider.gameObject))
                {
                    validPosition = false;
                    break;
                }
            }

            if (validPosition)
            {
                return spawnPosition;
            }
        }

        return Vector2.zero;
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

        MoveObjectsToRandomPositions(); // 적 스폰 로직을 여기서 호출

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
                GenerateWorshipersInArea(areaPosition, 1, 5); // 각각 1 ~ 5명 집단
            }
        }

        yield break;
    }

    public void NPCSpawnEvent()
    {
        // 총 8개의 구역을 선택해 각각 한 개의 NPC 집단(구성원 2~4명)을 생성함
        List<Vector2> spawnPositions = ChooseRandomAreaPositions(8);
        foreach (var pos in spawnPositions)
        {
            GenerateWorshipersInArea(pos, 2, 4);
        }
    }

    private List<Vector2> ChooseRandomAreaPositions(int numArea)
    {
        List<Vector2> areaPositions = new();

        Vector2 minBounds = MapCollider.bounds.min;
        Vector2 maxBounds = MapCollider.bounds.max;
        Debug.Log($"맵 경계: {minBounds} - {maxBounds}");

        int numAreasX = Mathf.FloorToInt((maxBounds.x - minBounds.x) / areaSizeX);
        int numAreasY = Mathf.FloorToInt((maxBounds.y - minBounds.y) / areaSizeY);

        HashSet<int> areaIndices = new(); // 이미 선택한 영역 집합

        for (int i = 0; i < numArea; ++i)
        {
            while (true)
            {
                // 영역 구간 중에서 한 칸을 랜덤하게 선택
                int areaX = Random.Range(0, numAreasX);
                int areaY = Random.Range(0, numAreasY);
                int areaIndex = areaY * numAreasX + areaX;
                if (areaIndices.Contains(areaIndex))
                {
                    continue; // 중복되는 영역이라 다시 선택
                }

                // 여기까지 왔으면 중복되지 않는 영역을 선택했다는 뜻
                Vector2 areaPosition = new Vector2(minBounds.x + areaX * areaSizeX, minBounds.y + areaY * areaSizeY);
                areaPositions.Add(areaPosition);
                break;
            }
        }

        return areaPositions;
    } 

    void GenerateWorshipersInArea(Vector2 areaPosition, int minWorshiper, int maxWorshiper)
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

        // 랜덤한 수의 Worshiper 생성 (Range 두 번째 인자는 exclusive라서 + 1 필요)
        int worshiperCount = Random.Range(minWorshiper, maxWorshiper + 1);
        List<GameObject> groupMembers = GenerateWorshiperMembers(worshiperCount, groupObject);

        group.InitializeGroup(groupMembers);
        worshiperGroups.Add(group);
    }

    Vector2 GetRandomPositionInArea(Vector2 areaPosition)
    {
        float randomX = Random.Range(areaPosition.x, areaPosition.x + areaSizeX);
        float randomY = Random.Range(areaPosition.y, areaPosition.y + areaSizeY);
        return new Vector2(randomX, randomY);
    }

    List<GameObject> GenerateWorshiperMembers(int count, GameObject groupObject)
    {
        List<GameObject> groupMembers = new List<GameObject>();

        for (int i = 0; i < count; i++)
        {
            var spawnPosition = new Vector2()
            {
                x = groupObject.transform.position.x + Random.Range(-1f, 1f),
                y = groupObject.transform.position.y + Random.Range(-1f, 1f)
            };
            Debug.Log($"NPC 스폰 위치: {spawnPosition}");
            GameObject worshiper = Instantiate(worshiperPrefab, spawnPosition, Quaternion.identity);
            worshiper.transform.SetParent(groupObject.transform);

            // WorshiperController 스크립트 추가 및 FollowTarget 설정
            WorshiperController worshiperController = worshiper.GetComponent<WorshiperController>();
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
        // 카운트다운 동안은 포교 불가능하게 설정
        playerPropagationRange.enabled = false;

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

        // 플레이어의 포교 범위 콜라이더를 활성화
        playerPropagationRange.enabled = true;
    }

    void StartGame()
    {
        Debug.Log("Game Started");
    }
}
