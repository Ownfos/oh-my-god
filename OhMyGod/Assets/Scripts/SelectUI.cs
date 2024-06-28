using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;


public class GodSelection : MonoBehaviour
{
    public Button godGood;
    public Button godEvil;
    public Button godStrange;
    public Button godGoodSelect;
    public Button godEvilSelect;
    public Button godStrangeSelect;

    public GameObject godGoodText; // Text UI 요소
    public GameObject godEvilText; // Text UI 요소
    public GameObject godStrangeText; // Text UI 요소

    void Start()
    {
        // 신 버튼 클릭 시 텍스트 활성화/비활성화
        godGood.onClick.AddListener(() => ToggleText(godGoodText));
        godEvil.onClick.AddListener(() => ToggleText(godEvilText));
        godStrange.onClick.AddListener(() => ToggleText(godStrangeText));

        // 선택 버튼 클릭 시 신 선택 및 씬 전환
        godGoodSelect.onClick.AddListener(() => SelectGod("Good"));
        godEvilSelect.onClick.AddListener(() => SelectGod("Evil"));
        godStrangeSelect.onClick.AddListener(() => SelectGod("Strange"));
    }

    void ToggleText(GameObject textObject)
    {
        textObject.SetActive(!textObject.activeSelf); // 활성화/비활성화 토글
    }

    void SelectGod(string godName)
    {
        SessionData.Instance.SelectedGod = godName;
        SceneManager.LoadScene("GameScene"); // 게임 씬으로 전환
    }
}
