using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Sprites;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasRenderer))]
public class UIArc : MaskableGraphic
{
    [Tooltip("��ũ �β�")]
    public float thickness = 20f;
    [Tooltip("��ũ ������")]
    public float radius = 100f;
    [Tooltip("���� ���� (��, 0 = 12��)")]
    [Range(0, 360)] public float startAngle = 0f;
    [Tooltip("�� ���� (��)")]
    [Range(0, 360)] public float endAngle = 90f;
    [Tooltip("���׸�Ʈ �� (Ŭ���� �ε巴��)")]
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

        // ���ؽ� ����
        for (int i = 0; i <= count; i++)
        {
            float a = startAngle + angleStep * i;
            float rad = Mathf.Deg2Rad * (90f - a); // 0�� = 12��, �ð����
            Vector2 outer = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
            Vector2 inner = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * innerR;

            // �� ���� ���ؽ� �߰� (inner, outer)
            vh.AddVert(inner, color, Vector2.zero);
            vh.AddVert(outer, color, Vector2.zero);
        }

        // �ﰢ�� �ε��� ����
        for (int i = 0; i < count; i++)
        {
            int i0 = i * 2;
            int i1 = i0 + 1;
            int i2 = i0 + 2;
            int i3 = i0 + 3;

            // ù �ﰢ��
            vh.AddTriangle(i0, i2, i1);
            // �� ��° �ﰢ��
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