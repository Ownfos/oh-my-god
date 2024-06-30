using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ArrowButtonMinigame : MonoBehaviour
{
    [Header("Assets")]
    [SerializeField] private Sprite arrowUp;
    [SerializeField] private Sprite arrowDown;
    [SerializeField] private Sprite arrowRight;
    [SerializeField] private Sprite arrowLeft;
    [SerializeField] private AudioSource arrowSuccessSound;
    [SerializeField] private AudioSource arrowFailSound;

    [Header("Visualization Options")]
    [SerializeField] private Color defaultColor;
    [SerializeField] private Color correctColor;
    [SerializeField] private Color wrongColor;

    [Header("Objects to Control")]
    [SerializeField] private Image arrow1;
    [SerializeField] private Image arrow2;
    [SerializeField] private Image arrow3;
    [SerializeField] private Image arrow4;

    public UnityEvent OnComplete; // 미니게임 성공 여부를 이벤트로 알려줌

    // 각 화살표의 정답 여부는 곧 색과 직결됨
    public bool IsArrow1Correct { get => arrow1.color == correctColor; }
    public bool IsArrow2Correct { get => arrow2.color == correctColor; }
    public bool IsArrow3Correct { get => arrow3.color == correctColor; }
    public bool IsArrow4Correct { get => arrow4.color == correctColor; }
    public bool IsAllCorrect { get => IsArrow1Correct && IsArrow2Correct && IsArrow3Correct && IsArrow4Correct; }

    public bool IsMinigameActive { get => nextArrowIndex >= 0 && nextArrowIndex <= 3; }

    // 0~3 사이의 값인 경우 arrowN의 위치를 예측하는 상황임.
    // 반대로 그 이외의 범위인 경우는 미니게임을 진행하지 않는 상황.
    private int nextArrowIndex = 4;

    private InputActions inputActions;

    private void Awake()
    {
        inputActions = new();
        inputActions.Minigame.Enable();

        inputActions.Minigame.W.performed += (context) => HandleInput(arrowUp);
        inputActions.Minigame.S.performed += (context) => HandleInput(arrowDown);
        inputActions.Minigame.D.performed += (context) => HandleInput(arrowRight);
        inputActions.Minigame.A.performed += (context) => HandleInput(arrowLeft);

        arrow1.transform.localScale = Vector3.zero;
        arrow2.transform.localScale = Vector3.zero;
        arrow3.transform.localScale = Vector3.zero;
        arrow4.transform.localScale = Vector3.zero;
    }

    private void HandleInput(Sprite input)
    {
        Image arrowRenderer = GetNextArrow();
        if (arrowRenderer == null)
        {
            return; // 4개의 화살표를 맞추는 상태가 아님...
        }

        if (arrowRenderer.sprite == input)
        {
            arrowRenderer.color = correctColor;
            arrowRenderer.transform.DOShakeScale(1f, 0.3f);
        }
        else
        {
            arrowRenderer.color = wrongColor;
            arrowRenderer.transform.DOShakeRotation(1f, 30f);
        }

        nextArrowIndex++;
        if (nextArrowIndex >= 4)
        {
            OnComplete.Invoke();
            PlayHideAnimation();


            if (IsAllCorrect)
            {
                arrowSuccessSound.Play();
            }
            else
            {
                arrowFailSound.Play();
            }
        }
    }

    private Image GetNextArrow()
    {
        if (nextArrowIndex == 0)
        {
            return arrow1;
        }
        else if (nextArrowIndex == 1)
        {
            return arrow2;
        }
        else if (nextArrowIndex == 2)
        {
            return arrow3;
        }
        else if (nextArrowIndex == 3)
        {
            return arrow4;
        }
        else
        {
            return null;
        }
    }

    void Start()
    {
        // 테스트 용도로 바로 게임 시작함!
        // StartNewMinigame();
    }

    public void StartNewMinigame()
    {
        RandomizeArrows();
        PlayPopupAnimation();
    }

    private void RandomizeArrows()
    {
        arrow1.sprite = SelectRandomSprite();
        arrow2.sprite = SelectRandomSprite();
        arrow3.sprite = SelectRandomSprite();
        arrow4.sprite = SelectRandomSprite();

        arrow1.color = defaultColor;
        arrow2.color = defaultColor;
        arrow3.color = defaultColor;
        arrow4.color = defaultColor;

        // 첫 번째 화살표부터 맞추기 시작!
        nextArrowIndex = 0;
    }

    private Sprite SelectRandomSprite()
    {
        float rv = Random.Range(0f, 1f);
        if (rv <= 0.25f) return arrowUp;
        if (rv <= 0.50f) return arrowRight;
        if (rv <= 0.75f) return arrowLeft;
        return arrowDown;
    }

    private void PlayPopupAnimation()
    {
        arrow1.transform.DOKill();
        arrow2.transform.DOKill();
        arrow3.transform.DOKill();
        arrow4.transform.DOKill();

        arrow1.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBounce);
        arrow2.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBounce);
        arrow3.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBounce);
        arrow4.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBounce);
    }

    private void PlayHideAnimation()
    {
        arrow1.transform.DOKill();
        arrow2.transform.DOKill();
        arrow3.transform.DOKill();
        arrow4.transform.DOKill();

        arrow1.transform.DOScale(0f, 0.5f).SetEase(Ease.OutSine);
        arrow2.transform.DOScale(0f, 0.5f).SetEase(Ease.OutSine);
        arrow3.transform.DOScale(0f, 0.5f).SetEase(Ease.OutSine);
        arrow4.transform.DOScale(0f, 0.5f).SetEase(Ease.OutSine);
    }
}
