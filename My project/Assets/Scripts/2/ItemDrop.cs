using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 바닥에 떨어진 아이템 큐브의 '데이터'를 보관합니다.
/// [수정] 충돌(OnCollisionEnter) 및 픽업 로직을 모두 제거합니다.
/// 픽업은 플레이어의 ItemPickupRadius.cs가 담당합니다.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class ItemDrop : MonoBehaviour
{
    // 이 아이템 드롭이 어떤 타입인지 (Block.cs에서 설정해 줌)
    public BlockType type;

    // 이 아이템 드롭이 몇 개인지 (Block.cs에서 설정해 줌)
    public int count = 1;

    // [제거] Start() 함수를 제거했습니다.

    // [제거] OnCollisionEnter() 함수 전체를 제거했습니다.
    // (물리적 충돌로 픽업하는 로직 삭제)
}