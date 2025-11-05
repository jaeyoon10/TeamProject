using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "CustomerType", menuName = "Game/Customer Type")]
public class CustomerType : ScriptableObject
{
    [Header("손님 이름 / 직업")]
    public string customerTitle;

    [Header("표기/프리셋")]
    public string displayName = "기본 손님";
    public GameObject customerPrefab; 

    [Header("스폰 확률")]
    [Tooltip("값이 클수록 자주 등장. 예: 거지=1, 기본=8, 부유층=2")]
    public int spawnWeight = 8;

    [Header("지불 보정")]
    [Tooltip("최종 지불액에 곱해질 배수. 예: 거지 0.7, 기본 1.0, 부유층 1.5")]
    [Range(0.1f, 5f)] public float paymentMultiplier = 1.0f;

    [TextArea]
    public string leaveLine;
}
