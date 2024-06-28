using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class GodSelection : MonoBehaviour
{
    public Button godGood;
    public Button godEvil;
    public Button godStrange;
    public Button confirmButton;

    public GameObject godGoodText; 
    public GameObject godEvilText; 
    public GameObject godStrangeText; 

    void Start()
    {
        godGood.onClick.AddListener(() => SelectGod("Good"));
        godEvil.onClick.AddListener(() => SelectGod("Evil"));
        godStrange.onClick.AddListener(() => SelectGod("Strange"));
        confirmButton.onClick.AddListener(ConfirmSelection);

        // EventTrigger 설정
        AddEventTrigger(godGood.gameObject, OnGodGoodEnter, EventTriggerType.PointerEnter);
        AddEventTrigger(godGood.gameObject, OnGodGoodExit, EventTriggerType.PointerExit);
        AddEventTrigger(godEvil.gameObject, OnGodEvilEnter, EventTriggerType.PointerEnter);
        AddEventTrigger(godEvil.gameObject, OnGodEvilExit, EventTriggerType.PointerExit);
        AddEventTrigger(godStrange.gameObject, OnGodStrangeEnter, EventTriggerType.PointerEnter);
        AddEventTrigger(godStrange.gameObject, OnGodStrangeExit, EventTriggerType.PointerExit);
    }

    void SelectGod(string godName)
    {
        SessionData.Instance.SelectedGod = godName;
    }

    void ConfirmSelection()
    {
        SceneManager.LoadScene("GameScene");
    }

    void AddEventTrigger(GameObject obj, UnityEngine.Events.UnityAction<BaseEventData> action, EventTriggerType triggerType)
    {
        EventTrigger trigger = obj.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = obj.AddComponent<EventTrigger>();
        }

        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = triggerType };
        entry.callback.AddListener(action);
        trigger.triggers.Add(entry);
    }

    // GodGood 이벤트 핸들러
    void OnGodGoodEnter(BaseEventData data)
    {
        godGoodText.SetActive(true);
    }

    void OnGodGoodExit(BaseEventData data)
    {
        godGoodText.SetActive(false);
    }

    // GodEvil 이벤트 핸들러
    void OnGodEvilEnter(BaseEventData data)
    {
        godEvilText.SetActive(true);
    }

    void OnGodEvilExit(BaseEventData data)
    {
        godEvilText.SetActive(false);
    }

    // GodStrange 이벤트 핸들러
    void OnGodStrangeEnter(BaseEventData data)
    {
        godStrangeText.SetActive(true);
    }

    void OnGodStrangeExit(BaseEventData data)
    {
        godStrangeText.SetActive(false);
    }
}
