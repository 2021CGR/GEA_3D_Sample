using System.Collections.Generic;
using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    public Transform firePoint;                      // 발사 위치
    public KeyCode switchKey = KeyCode.Z;            // 프리팹 전환 키

    [Tooltip("Z키로 순환 전환할 발사 프리팹 목록 (첫 항목이 초기값)")]
    public List<GameObject> projectileVariants = new List<GameObject>();

    private Camera cam;
    private int currentIndex = 0;

    void Start()
    {
        cam = Camera.main;

        // 초기 인덱스 보정
        if (projectileVariants != null && projectileVariants.Count > 0)
            currentIndex = Mathf.Clamp(currentIndex, 0, projectileVariants.Count - 1);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            Shoot();

        // Z키로 프리팹 순환 전환
        if (Input.GetKeyDown(switchKey) && projectileVariants != null && projectileVariants.Count > 0)
            currentIndex = (currentIndex + 1) % projectileVariants.Count;
    }

    void Shoot()
    {
        if (firePoint == null || cam == null) return;
        if (projectileVariants == null || projectileVariants.Count == 0) return;

        GameObject prefab = projectileVariants[currentIndex];
        if (prefab == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Vector3 targetPoint = ray.GetPoint(50f);
        Vector3 direction = (targetPoint - firePoint.position).normalized;

        Instantiate(prefab, firePoint.position, Quaternion.LookRotation(direction));
    }
}


