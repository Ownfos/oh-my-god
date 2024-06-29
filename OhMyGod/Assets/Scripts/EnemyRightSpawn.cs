using UnityEngine;

public class EnemyRightSpawn : MonoBehaviour
{
    [SerializeField] private Sprite goodGodWorshiper;
    [SerializeField] private Sprite evilGodWorshiper;
    [SerializeField] private Sprite strangeGodWorshiper;
    [SerializeField] private Sprite goodGod;
    [SerializeField] private Sprite evilGod;
    [SerializeField] private Sprite strangeGod;

    private SpriteRenderer spriteRenderer;
    private WorshipPropagationController propagationController;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        propagationController = GetComponent<WorshipPropagationController>();
    }

    private void Start()
    {
        string godName = SessionData.Instance.SelectedGod;

        if (godName == "Strange")
        {
            SetEnemyAttributes(evilGodWorshiper, evilGodWorshiper, evilGod);
        }
        else if (godName == "Good")
        {
            SetEnemyAttributes(strangeGodWorshiper, strangeGodWorshiper, strangeGod);
        }
        else if (godName == "Evil")
        {
            SetEnemyAttributes(goodGodWorshiper, goodGodWorshiper, goodGod);
        }
    }

    private void SetEnemyAttributes(Sprite enemySprite, Sprite worshiperSprite, Sprite godSprite)
    {
        spriteRenderer.sprite = enemySprite;
        propagationController.WorshiperSprite = worshiperSprite;
        propagationController.GodSprite = godSprite;
    }
}
