using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public enum EmojiType
{
    Surprise,
    Talk,
    Happy,
    Sad,
    Celebrate,
    NoComment,
    Annoyed,
}

public class EmojiController : MonoBehaviour
{
    [SerializeField] private Sprite surprise;
    [SerializeField] private Sprite talk;
    [SerializeField] private Sprite happy;
    [SerializeField] private Sprite sad;
    [SerializeField] private Sprite celebrate;
    [SerializeField] private Sprite noComment;
    [SerializeField] private Sprite annoyed;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }

    public void PopupEmoji(EmojiType type)
    {
        if (type == EmojiType.Surprise)
        {
            spriteRenderer.sprite = surprise;
        }
        else if (type == EmojiType.Talk)
        {
            spriteRenderer.sprite = talk;
        }
        else if (type == EmojiType.Sad)
        {
            spriteRenderer.sprite = sad;
        }
        else if (type == EmojiType.Happy)
        {
            spriteRenderer.sprite = happy;
        }
        else if (type == EmojiType.Celebrate)
        {
            spriteRenderer.sprite = celebrate;
        }
        else if (type == EmojiType.NoComment)
        {
            spriteRenderer.sprite = noComment;
        }
        else if (type == EmojiType.Annoyed)
        {
            spriteRenderer.sprite = annoyed;
        }

        // 1. 커지기
        transform.DOKill();
        transform.localScale = Vector3.zero;
        transform.DOScale(1f, 0.5f).SetEase(Ease.OutBounce).OnComplete(() => {
            // 2. 1초 대기
            transform.DOScale(1f, 0.5f).SetEase(Ease.Linear).OnComplete(() => {
                // 3. 사라지기
                transform.DOScale(0f, 0.3f).SetEase(Ease.InSine);
            });
        });
    }
}
