using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Vector3 originalScale;

    private void Start()
    {
        originalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = originalScale * 1.1f; // »ìÂ¦ Ä¿Áü
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = originalScale; // ¿ø·¡´ë·Î
    }
}
