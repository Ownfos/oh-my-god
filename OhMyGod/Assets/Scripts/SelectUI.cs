using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GodSelection : MonoBehaviour
{
    public Button godGood;
    public Button godEvil;
    public Button godStrange;
    public Button confirmButton;

    void Start()
    {
        godGood.onClick.AddListener(() => SelectGod("Good"));
        godEvil.onClick.AddListener(() => SelectGod("Evil"));
        godStrange.onClick.AddListener(() => SelectGod("Strange"));
        confirmButton.onClick.AddListener(ConfirmSelection);
    }

    void SelectGod(string godName)
    {
        SessionData.Instance.SelectedGod = godName;
    }

    void ConfirmSelection()
    {
        SceneManager.LoadScene("GameScene");
    }
}
