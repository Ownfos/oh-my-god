using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeController : MonoBehaviour
{
    public Image fadeImage;
    public float fadeDuration = 2.0f;

    void Start()
    {
        // 초기화: 이미지가 완전히 불투명하게 설정
        fadeImage.color = new Color(0, 0, 0, 1);
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        Color color = fadeImage.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(1.0f - (elapsedTime / fadeDuration));
            fadeImage.color = color;
            yield return null;
        }

        fadeImage.gameObject.SetActive(false); // 페이드인 완료 후 이미지 비활성화
    }
}
