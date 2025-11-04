// 파일 이름: PlayerShooting.cs
using System.Collections.Generic;
using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    public Transform firePoint;
    public KeyCode switchKey = KeyCode.Z;

    // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ [수정된 변수] ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
    [Tooltip("게임에 등장하는 '모든' 무기 프리팹 목록 (해금될 순서대로 배치하세요)")]
    public List<GameObject> allWeaponPrefabs = new List<GameObject>(); // (1) '전체' 목록

    [Tooltip("현재 플레이어가 '실제로 사용 가능한' 해금된 무기 목록 (자동으로 채워짐)")]
    private List<GameObject> projectileVariants = new List<GameObject>(); // (2) '사용' 목록 (private로 변경)
    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

    private Camera cam;
    private int currentIndex = 0;

    void Start()
    {
        cam = Camera.main;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ [추가된 로직] ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
        // 해금된 무기를 로드하는 함수를 호출합니다.
        UpdateUnlockedWeapons();
        // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
    }

    // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ [추가된 함수] ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
    /// <summary>
    /// GameProgressManager에서 현재 해금된 무기 개수를 가져와
    /// 'projectileVariants' (실제 사용 목록) 리스트를 채웁니다.
    /// </summary>
    void UpdateUnlockedWeapons()
    {
        // 1. GameProgressManager에서 몇 개의 무기가 해금되었는지 가져옵니다.
        //    (매니저가 없으면 기본값 1개만 사용)
        int weaponsToUnlock = 1;
        if (GameProgressManager.instance != null)
        {
            weaponsToUnlock = GameProgressManager.instance.unlockedWeaponCount;
        }

        // 2. 현재 사용 중인 무기 목록(projectileVariants)을 비웁니다.
        projectileVariants.Clear();
        currentIndex = 0; // 무기 인덱스 리셋

        // 3. '전체' 무기 목록(allWeaponPrefabs)에서 해금된 개수만큼
        //    '사용' 무기 목록(projectileVariants)으로 복사합니다.
        for (int i = 0; i < weaponsToUnlock; i++)
        {
            // (혹시 allWeaponPrefabs 목록을 다 채우지 않았을 경우를 대비한 방어 코드)
            if (i < allWeaponPrefabs.Count && allWeaponPrefabs[i] != null)
            {
                projectileVariants.Add(allWeaponPrefabs[i]);
            }
        }

        Debug.Log($"[PlayerShooting] {projectileVariants.Count}개의 무기를 로드했습니다.");
    }
    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲


    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            Shoot();

        // 사용 가능한 무기가 2개 이상일 때만 무기 전환이 작동하도록 수정
        if (Input.GetKeyDown(switchKey) && projectileVariants.Count > 1)
        {
            currentIndex = (currentIndex + 1) % projectileVariants.Count;
        }
    }

    void Shoot()
    {
        if (firePoint == null || cam == null) return;

        // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ [수정된 로직] ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
        // 사용 가능한 무기가 0개이면 발사하지 않음
        if (projectileVariants.Count == 0)
        {
            Debug.LogWarning("사용 가능한 무기가 없습니다!");
            return;
        }

        // (혹시 리스트가 변경되어 인덱스가 범위를 벗어났다면 0으로 리셋)
        if (currentIndex >= projectileVariants.Count)
        {
            currentIndex = 0;
        }

        // '사용' 목록(projectileVariants)에서 현재 선택된 프리팹을 가져옵니다.
        GameObject prefab = projectileVariants[currentIndex];
        // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

        if (prefab == null) return;

        Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
        Vector3 targetPoint = ray.GetPoint(50f);
        Vector3 direction = (targetPoint - firePoint.position).normalized;
        Instantiate(prefab, firePoint.position, Quaternion.LookRotation(direction));
    }
}