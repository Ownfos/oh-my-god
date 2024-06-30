using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoBattleController : MonoBehaviour
{
    [SerializeField] private float battleDuration = 15f; // 전투 시간
    [SerializeField] private float postBattleCooldown = 5f; // 전투 종료 후 쿨다운 시간

    public bool isAutoBattleActive = false; // 전투 상태를 나타내는 변수
    private float battleCooldownTimer = 0f;
    private WorshipPropagationController leftTeam;
    private WorshipPropagationController rightTeam;

    [SerializeField] private GameObject dummyEnemyLeft;
    [SerializeField] private GameObject dummyEnemyRight;
    [SerializeField] private RankingSystem rankingSystem;

    private void Start()
    {
        InitializeTeams();

        // Ensure the propagation ranges have triggers enabled and correct tags
        SetPropagationRangeSettings(dummyEnemyLeft);
        SetPropagationRangeSettings(dummyEnemyRight);
    }

    private void Update()
    {
        if (!isAutoBattleActive && battleCooldownTimer > 0f)
        {
            battleCooldownTimer -= Time.deltaTime;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isAutoBattleActive) return;

        if (leftTeam == null || rightTeam == null)
        {
            Debug.LogError("One of the teams is null during OnTriggerEnter2D.");
            return;
        }

        // Check if the collision is between the left and right propagation ranges
        if ((other.gameObject == dummyEnemyLeft.transform.Find("Circle").gameObject && other.CompareTag("PropagationRange")) ||
            (other.gameObject == dummyEnemyRight.transform.Find("Circle").gameObject && other.CompareTag("PropagationRange")))
        {
            Debug.Log("Starting auto battle.");
            StartCoroutine(ExecuteAutoBattle());
        }
        else
        {
            Debug.Log("Collision detected but not between the expected propagation ranges.");
        }
    }

    private IEnumerator ExecuteAutoBattle()
    {
        Debug.Log("Executing auto battle.");
        isAutoBattleActive = true;

        // 포교 스크립트 비활성화
        leftTeam.enabled = false;
        rightTeam.enabled = false;

        yield return new WaitForSeconds(battleDuration);

        // 50% 확률로 승자 결정
        bool leftWins = Random.value > 0.5f;
        Debug.Log(leftWins ? "Left team wins." : "Right team wins.");

        if (leftWins)
        {
            ProcessBattleResult(leftTeam, rightTeam);
        }
        else
        {
            ProcessBattleResult(rightTeam, leftTeam);
        }

        yield return new WaitForSeconds(postBattleCooldown);

        // 포교 스크립트 활성화
        leftTeam.enabled = true;
        rightTeam.enabled = true;

        isAutoBattleActive = false;
        battleCooldownTimer = postBattleCooldown;
        Debug.Log("Auto battle ended.");
    }

    private void ProcessBattleResult(WorshipPropagationController winner, WorshipPropagationController loser)
    {
        Debug.Log($"Processing battle result. Winner: {winner.name}, Loser: {loser.name}");
        int numWorshipersToTransfer = loser.ActiveWorshipers.Count / 2;
        Debug.Log($"Transferring {numWorshipersToTransfer} worshipers from loser to winner.");

        for (int i = 0; i < numWorshipersToTransfer; i++)
        {
            if (loser.ActiveWorshipers.Count > 0)
            {
                var worshiper = loser.ActiveWorshipers[loser.ActiveWorshipers.Count - 1];
                winner.AddWorshiper(worshiper);
                loser.ActiveWorshipers.Remove(worshiper);
                Debug.Log($"Transferred worshiper {worshiper.name}.");
            }
        }

        // 신도 수가 3명 이하인 경우 패배한 팀의 부모 오브젝트 삭제
        if (loser.ActiveWorshipers.Count <= 3)
        {
            Debug.Log($"Loser team {loser.name} has 3 or fewer worshipers. Destroying the parent object.");
            rankingSystem.RemoveCompetitor(loser);
            Destroy(loser.gameObject);
        }

        // 보호 기간 부여
        loser.GiveProtectionPeriod();
        Debug.Log($"Protection period given to loser: {loser.name}");
    }

    private void InitializeTeams()
    {
        if (dummyEnemyLeft != null)
        {
            leftTeam = dummyEnemyLeft.GetComponent<WorshipPropagationController>();
            if (leftTeam == null)
            {
                Debug.LogError("dummyEnemyLeft does not have a WorshipPropagationController component.");
            }
        }
        else
        {
            Debug.LogError("dummyEnemyLeft is not assigned.");
        }

        if (dummyEnemyRight != null)
        {
            rightTeam = dummyEnemyRight.GetComponent<WorshipPropagationController>();
            if (rightTeam == null)
            {
                Debug.LogError("dummyEnemyRight does not have a WorshipPropagationController component.");
            }
        }
        else
        {
            Debug.LogError("dummyEnemyRight is not assigned.");
        }
    }

    private void SetPropagationRangeSettings(GameObject enemy)
    {
        var collider = enemy.transform.Find("Circle")?.GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
            collider.gameObject.tag = "PropagationRange";
        }
    }
}
