// 파일 이름: LavaSplash.cs
using UnityEngine;

public class LavaSplash : MonoBehaviour
{
    // --- public 변수 (인스펙터 창에서 값을 수정할 수 있습니다) ---

    [Header("점프 설정")]
    [Tooltip("불똥이 솟구치는 최소 힘")]
    public float minJumpForce = 5f; // 불똥이 쏘아 올려지는 최소 힘의 크기

    [Tooltip("불똥이 솟구치는 최대 힘")]
    public float maxJumpForce = 10f; // 불똥이 쏘아 올려지는 최대 힘의 크기

    [Header("물리 설정")]
    [Tooltip("불똥에 적용될 중력 값")]
    public float gravity = 9.8f; // 불똥을 아래로 끌어당기는 중력의 크기

    // --- private 변수 (스크립트 내부에서만 사용됩니다) ---
    private float verticalVelocity; // 불똥의 현재 수직 속도
    private float initialYPosition; // 불똥이 처음 생성된 Y축 위치

    /// <summary>
    /// 게임 오브젝트가 처음 생성될 때 한 번 호출되는 함수입니다.
    /// </summary>
    void Start()
    {
        // 현재 오브젝트의 시작 Y축 위치를 저장합니다.
        // 이 위치는 나중에 불똥이 용암으로 다시 돌아왔는지 판단하는 기준이 됩니다.
        initialYPosition = transform.position.y;

        // 최소 힘과 최대 힘 사이에서 랜덤한 값을 선택하여 초기 수직 속도로 설정합니다.
        // 이로 인해 불똥이 튀어 오르는 높이가 매번 달라지게 됩니다.
        verticalVelocity = Random.Range(minJumpForce, maxJumpForce);
    }

    /// <summary>
    /// 매 프레임마다 호출되는 함수입니다.
    /// </summary>
    void Update()
    {
        // 중력 값을 수직 속도에 계속해서 빼줍니다. (아래로 떨어지는 효과)
        // Time.deltaTime을 곱해주는 이유는 모든 컴퓨터에서 동일한 속도로 움직이게 하기 위함입니다.
        verticalVelocity -= gravity * Time.deltaTime;

        // 계산된 수직 속도만큼 Y축으로 오브젝트를 이동시킵니다.
        transform.Translate(Vector3.up * verticalVelocity * Time.deltaTime);

        // 불똥의 현재 Y축 위치가 처음 위치보다 낮거나 같아지면
        if (transform.position.y <= initialYPosition)
        {
            // 불똥이 용암으로 다시 돌아온 것으로 간주하고 오브젝트를 파괴(삭제)합니다.
            // 이렇게 하지 않으면 게임이 진행될수록 불필요한 오브젝트가 계속 쌓여 성능이 저하될 수 있습니다.
            Destroy(gameObject);
        }
    }
}
