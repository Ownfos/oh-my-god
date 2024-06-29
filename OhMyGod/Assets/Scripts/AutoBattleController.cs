using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoBattleController : MonoBehaviour
{
    [SerializeField] private float battleDuration = 15f; // 전투 시간
    [SerializeField] private float postBattleCooldown = 5f; // 전투 종료 후 쿨다운 시간
    [SerializeField] private LayerMask aiLayerMask; // AI 레이어 마스크

    private bool isBattleActive = false;
    private float battleCooldownTimer = 0f;
    private WorshipPropagationController leftTeam;
    private WorshipPropagationController rightTeam;

    private void Update()
    {
        if (!isBattleActive)
        {
            if (battleCooldownTimer > 0f)
            {
                battleCooldownTimer -= Time.deltaTime;
            }
            else
            {
                DetectAndStartBattle();
            }
        }
    }

    private void DetectAndStartBattle()
    {
        if (leftTeam == null || rightTeam == null) return;

        float detectionRadius = Mathf.Max(leftTeam.GetPropagationRange().transform.localScale.x, rightTeam.GetPropagationRange().transform.localScale.x) * 0.5f;
        Collider2D[] detectedAIs = Physics2D.OverlapCircleAll(transform.position, detectionRadius, aiLayerMask);
        if (detectedAIs.Length < 2) return;

        DummyEnemyEvil evilAI = null;
        DummyEnemyStrange strangeAI = null;

        foreach (Collider2D collider in detectedAIs)
        {
            if (evilAI == null) evilAI = collider.GetComponent<DummyEnemyEvil>();
            if (strangeAI == null) strangeAI = collider.GetComponent<DummyEnemyStrange>();

            if (evilAI != null && strangeAI != null)
            {
                StartCoroutine(ExecuteAutoBattle(evilAI, strangeAI));
                break;
            }
        }
    }

    private IEnumerator ExecuteAutoBattle(DummyEnemyEvil evilAI, DummyEnemyStrange strangeAI)
    {
        isBattleActive = true;
        leftTeam = evilAI.GetComponent<WorshipPropagationController>();
        rightTeam = strangeAI.GetComponent<WorshipPropagationController>();

        // 포교 스크립트 비활성화 및 움직임 정지
        leftTeam.enabled = false;
        rightTeam.enabled = false;
        evilAI.StopMovement();
        strangeAI.StopMovement();

        // PlayerController 비활성화
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().enabled = false;

        yield return new WaitForSeconds(battleDuration);

        // 50% 확률로 승자 결정
        bool leftWins = Random.value > 0.5f;

        if (leftWins)
        {
            ProcessBattleResult(leftTeam, rightTeam);
        }
        else
        {
            ProcessBattleResult(rightTeam, leftTeam);
        }

        yield return new WaitForSeconds(postBattleCooldown);

        // 포교 스크립트 활성화 및 움직임 재개
        leftTeam.enabled = true;
        rightTeam.enabled = true;
        evilAI.ResumeMovement();
        strangeAI.ResumeMovement();

        // PlayerController 활성화
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().enabled = true;

        isBattleActive = false;
        battleCooldownTimer = postBattleCooldown;
    }

    private void ProcessBattleResult(WorshipPropagationController winner, WorshipPropagationController loser)
    {
        int numWorshipersToTransfer = loser.ActiveWorshipers.Count / 2;
        for (int i = 0; i < numWorshipersToTransfer; i++)
        {
            if (loser.ActiveWorshipers.Count > 0)
            {
                var worshiper = loser.ActiveWorshipers[loser.ActiveWorshipers.Count - 1];
                winner.AddWorshiper(worshiper);
                loser.ActiveWorshipers.Remove(worshiper);
            }
        }

        // 보호 기간 부여
        loser.GiveProtectionPeriod();
    }
}

public class DummyEnemyEvil : MonoBehaviour
{
    // 이동 중지 및 재개 메서드 구현
    public void StopMovement() { /* 이동 중지 로직 */ }
    public void ResumeMovement() { /* 이동 재개 로직 */ }
}

public class DummyEnemyStrange : MonoBehaviour
{
    // 이동 중지 및 재개 메서드 구현
    public void StopMovement() { /* 이동 중지 로직 */ }
    public void ResumeMovement() { /* 이동 재개 로직 */ }
}
