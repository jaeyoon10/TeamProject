using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Sprites;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasRenderer))]
public class UIArc : MaskableGraphic
{
    [Tooltip("아크 두께")]
    public float thickness = 20f;
    [Tooltip("아크 반지름")]
    public float radius = 100f;
    [Tooltip("시작 각도 (도, 0 = 12시)")]
    [Range(0, 360)] public float startAngle = 0f;
    [Tooltip("끝 각도 (도)")]
    [Range(0, 360)] public float endAngle = 90f;
    [Tooltip("세그먼트 수 (클수록 부드럽게)")]
    public int segments = 60;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        float ang = endAngle - startAngle;
        if (ang < 0) ang += 360f;
        int count = Mathf.CeilToInt(segments * (ang / 360f));
        float angleStep = ang / count;

        Vector2 center = Vector2.zero;
        float innerR = radius - thickness;

        // 버텍스 생성
        for (int i = 0; i <= count; i++)
        {
            float a = startAngle + angleStep * i;
            float rad = Mathf.Deg2Rad * (90f - a); // 0도 = 12시, 시계방향
            Vector2 outer = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
            Vector2 inner = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * innerR;

            // 두 개의 버텍스 추가 (inner, outer)
            vh.AddVert(inner, color, Vector2.zero);
            vh.AddVert(outer, color, Vector2.zero);
        }

        // 삼각형 인덱스 생성
        for (int i = 0; i < count; i++)
        {
            int i0 = i * 2;
            int i1 = i0 + 1;
            int i2 = i0 + 2;
            int i3 = i0 + 3;

            // 첫 삼각형
            vh.AddTriangle(i0, i2, i1);
            // 두 번째 삼각형
            vh.AddTriangle(i1, i2, i3);
        }
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        segments = Mathf.Max(4, segments);
        thickness = Mathf.Clamp(thickness, 1f, radius);
        SetVerticesDirty();
    }
#endif
}