using UnityEngine;
using UnityEngine.UI; // Image 컴포넌트를 사용하기 위해 추가해야 합니다.
using UnityEngine.EventSystems;

// 이 스크립트가 Image 컴포넌트를 가진 게임 오브젝트에만 붙도록 강제합니다.
[RequireComponent(typeof(Image))]
public class CharacterHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Image image;
    private Color originalColor = new Color(133f / 255f, 133f / 255f, 133f / 255f);
    private Color hoverColor = Color.white; // new Color(1f, 1f, 1f)와 동일합니다.

    private void Start()
    {
        // 스크립트가 붙어있는 게임 오브젝트에서 Image 컴포넌트를 가져옵니다.
        image = GetComponent<Image>();

        // 시작할 때 기본 색상으로 설정합니다.
        image.color = originalColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 마우스 커서를 올렸을 때 색상을 변경합니다.
        image.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 마우스 커서가 벗어났을 때 원래 색상으로 되돌립니다.
        image.color = originalColor;
    }
}