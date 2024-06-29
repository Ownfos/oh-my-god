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
}

public class EmojiController : MonoBehaviour
{
    [SerializeField] private Sprite surprise;
    [SerializeField] private Sprite talk;
    [SerializeField] private Sprite happy;
    [SerializeField] private Sprite sad;
    [SerializeField] private Sprite celebrate;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
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

        // 1. 커지기
        transform.DOKill();
        transform.localScale = Vector3.zero;
        transform.DOScale(1f, 1f).SetEase(Ease.OutSine).OnComplete(() => {
            // 2. 1초 대기
            transform.DOScale(1f, 1f).SetEase(Ease.Linear).OnComplete(() => {
                // 3. 사라지기
                transform.DOScale(0f, 1f).SetEase(Ease.InSine);
            });
        });
    }
}
