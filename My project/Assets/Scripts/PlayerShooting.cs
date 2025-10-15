// C# 스크립트는 항상 using으로 시작하여 필요한 라이브러리를 가져옵니다.
using System.Collections.Generic;
using UnityEngine;

// MonoBehaviour를 상속받아 유니티 게임 오브젝트의 컴포넌트로 작동할 수 있게 합니다.
public class PlayerShooting : MonoBehaviour
{
    // public 변수는 유니티 에디터의 Inspector 창에서 값을 설정할 수 있습니다.
    public Transform firePoint;                      // 총알이 생성될 위치 (총구)
    public KeyCode switchKey = KeyCode.Z;            // 무기(총알 프리팹) 전환 키

    // Tooltip을 사용하면 Inspector 창에서 변수 위에 마우스를 올렸을 때 설명이 나옵니다.
    [Tooltip("Z키로 순환 전환할 발사 프리팹 목록 (첫 항목이 초기값)")]
    public List<GameObject> projectileVariants = new List<GameObject>();

    // private 변수는 이 스크립트 내부에서만 사용되며, 외부에서 접근할 수 없습니다.
    private Camera cam;         // 메인 카메라를 저장할 변수
    private int currentIndex = 0; // 현재 선택된 총알 프리팹의 인덱스(순번)

    // Start() 메서드는 게임 시작 시 첫 프레임이 업데이트되기 전에 한 번만 호출됩니다.
    void Start()
    {
        // Camera.main은 "MainCamera" 태그가 붙은 카메라를 자동으로 찾아옵니다.
        cam = Camera.main;

        // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ 이 부분이 추가되었습니다 ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
        // 게임이 시작되면 마우스 커서를 화면 중앙에 고정시킵니다.
        Cursor.lockState = CursorLockMode.Locked;
        // 마우스 커서가 보이지 않도록 숨깁니다.
        Cursor.visible = false;
        // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

        // 총알 목록이 비어있지 않다면, 초기 인덱스가 목록 범위를 벗어나지 않도록 보정합니다.
        if (projectileVariants != null && projectileVariants.Count > 0)
            currentIndex = Mathf.Clamp(currentIndex, 0, projectileVariants.Count - 1);
    }

    // Update() 메서드는 매 프레임마다 호출됩니다. 게임의 핵심 로직이 담기는 곳입니다.
    void Update()
    {
        // Input.GetMouseButtonDown(0)은 마우스 왼쪽 버튼을 "누르는 순간"을 감지합니다.
        if (Input.GetMouseButtonDown(0))
            Shoot(); // 마우스 왼쪽 버튼이 눌리면 Shoot() 메서드를 호출합니다.

        // Input.GetKeyDown()은 키보드 키를 "누르는 순간"을 감지합니다.
        if (Input.GetKeyDown(switchKey) && projectileVariants != null && projectileVariants.Count > 0)
            // '%' 연산자(나머지)를 사용해 인덱스가 리스트 크기 내에서 순환하도록 만듭니다.
            currentIndex = (currentIndex + 1) % projectileVariants.Count;
    }

    // 총알 발사를 처리하는 메서드입니다.
    void Shoot()
    {
        // 에디터에서 firePoint나 cam을 할당하지 않았을 경우를 대비한 방어 코드입니다.
        if (firePoint == null || cam == null) return;
        if (projectileVariants == null || projectileVariants.Count == 0) return;

        // 현재 선택된 총알 프리팹을 가져옵니다.
        GameObject prefab = projectileVariants[currentIndex];
        // 리스트의 해당 칸이 비어있는 경우를 대비한 방어 코드입니다.
        if (prefab == null) return;

        // 카메라의 정중앙에서부터 앞으로 뻗어나가는 가상의 선(Ray)을 생성합니다.
        Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));

        // Ray가 50유닛(미터) 떨어진 지점의 좌표를 계산합니다. 이 지점이 목표 지점이 됩니다.
        Vector3 targetPoint = ray.GetPoint(50f);
        // 발사 위치(firePoint)에서 목표 지점(targetPoint)을 향하는 방향 벡터를 계산하고 정규화(길이를 1로 만듦)합니다.
        Vector3 direction = (targetPoint - firePoint.position).normalized;

        // Instantiate 함수로 총알 프리팹을 게임 세상에 복제하여 생성합니다.
        // firePoint.position: 생성될 위치
        // Quaternion.LookRotation(direction): 생성될 때 바라볼 방향
        Instantiate(prefab, firePoint.position, Quaternion.LookRotation(direction));
    }
}