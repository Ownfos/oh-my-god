using System.Collections.Generic;
using UnityEngine;

// 한 명 이상의 민간인 무리의 포교 처리를 담당하는 클래스
public class NeutralWorshiperGroup : MonoBehaviour
{
    private WorshiperController[] worshipers;

    private void Start()
    {
        // 내 gameobject의 자식으로 설정된 모든 worshiper
        // 오브젝트를 확인하고 목록으로 정리해둔다.
        //
        // 나중에 민간인 집단 포교 처리에 유용하게 사용될 것...
        worshipers = GetComponentsInChildren<WorshiperController>();
    }
}
