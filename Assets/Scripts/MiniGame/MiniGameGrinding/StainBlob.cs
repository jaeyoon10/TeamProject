using UnityEngine;
using UnityEngine.UI;

public class StainBlob : MonoBehaviour
{
    [Range(0.1f, 2f)] public float hp = 1f;     // 문지르면 줄어드는 체력
    public float eraseSpeed = 1.5f;             // 브러시 안에 있을 때 초당 깎이는 양

    Image _img;
    float _maxHp;

    void Awake()
    {
        _img = GetComponent<Image>();
        _maxHp = hp;
        UpdateVisual();
    }

    // 브러시가 닿아있을 때 매 프레임 호출됨
    public void WipeTick(float dt)
    {
        if (hp <= 0f) return;
        hp -= eraseSpeed * dt;
        if (hp < 0f) hp = 0f;
        UpdateVisual();
        if (hp <= 0f) gameObject.SetActive(false);
    }

    void UpdateVisual()
    {
        if (_img == null) return;
        var c = _img.color;
        // 남은 hp 비율로 투명도 조절
        c.a = Mathf.InverseLerp(0f, _maxHp, hp);
        _img.color = c;
    }

    public bool IsCleared => hp <= 0f;
}
