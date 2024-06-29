using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 메인 게임에서 활동중인 유저와 AI의 신도 수 정렬과 UI 표시를 담당함.
// 게임 엔딩 화면에서 랭킹 정보에 따라 soso 엔딩과 good 엔딩이 갈림.
// Note: 1위를 한 경우 good 엔딩, 2위 이하인 경우 soso 엔딩
public class RankingSystem : MonoBehaviour
{
    // 1위부터 5위까지 화면 우측 상단에 표시해주기 위한 다섯 개의 텍스트 컴포넌트.
    // 주인공은 5위 이하더라도 5번째 칸에 랭크가 표시된다.
    // ex) 주인공이 7위인 경우 1,2,3,4,7위가 표시됨
    [SerializeField] private List<Text> rankingTexts;
    [SerializeField] private WorshipPropagationController player;

    // 열심히 종교를 퍼트리고 있는 플레이어 또는 AI의 목록
    private List<WorshipPropagationController> competitors = new();

    private void Start()
    {
        AddCompetitor(player);
    }

    // 신도 수 랭킹에 포함되는 대상이 생성되었을 때 반드시 호출해야 하는 함수.
    // 예외적으로 플레이어의 등록은 어차피 레퍼런스가 필요한 관계로 여기서 처리해준다.
    public void AddCompetitor(WorshipPropagationController competitor)
    {
        competitors.Add(competitor);
        RecalculateRank();
    }

    // 신도 수 랭킹에 포함되는 대상이 scene에서 제거될 때 반드시 호출해야 하는 함수.
    public void RemoveCompetitor(WorshipPropagationController competitor)
    {
        competitors.Remove(competitor);
        RecalculateRank();
    }

    // 신도 수에 변화가 생길 때마다 호출되는 함수.
    // 새롭게 경쟁자들을 정렬하고 UI를 업데이트함.
    public void RecalculateRank()
    {
        // 신도 순으로 정렬
        competitors.Sort((lhs, rhs) => lhs.ActiveWorshipers.Count.CompareTo(rhs.ActiveWorshipers.Count));

        // 경쟁자가 4명 이하로 떨어진 경우 순서대로 출력
        if (competitors.Count < 5)
        {
            for (int i = 0; i < 5; ++i)
            {
                if (i < competitors.Count)
                {
                    rankingTexts[i].text = $"{i+1}. {competitors[i].name}";
                }
                else
                {
                    rankingTexts[i].text = "";
                }
            }
        }
        // 경쟁자가 5명 이상인 경우 5위 표시에 플레이어가 끼어들어갈 수 있음
        else
        {
            for (int i = 0; i < 4; ++i)
            {
                rankingTexts[i].text = $"{i+1}. {competitors[i].name}";
            }

            // 플레이어가 만약 5위 이하라면 5위 표시는 플레이어의 랭킹으로 대체됨
            int playerRank = FindPlayerRank();
            if (playerRank >= 4)
            {
                rankingTexts[4].text = $"{playerRank + 1}. Player";
            }
            else
            {
                rankingTexts[4].text = $"5. {competitors[4].name}";
            }
        }

    }

    public int FindPlayerRank()
    {
        for (int i = 0; i < competitors.Count; ++i)
        {
            if (competitors[i] == player)
            {
                return i;
            }
        }
        return -1; // 플레이어가 리스트에 있는 이상 여기에 도달할 일은 없음!
    }
}
