// 파일 이름: Projectile.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 20f;
    public float lifeTime = 2f;
    public int damage = 25; // 투사체가 입히는 데미지

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    /// <summary>
    /// [수정됨] "Enemy" 태그 또는 "Boss" 태그를 확인합니다.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ [수정된 부분] ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
        // 부딪힌 대상이 "Enemy" 태그 이거나 "Boss" 태그인지 확인
        if (other.CompareTag("Enemy") || other.CompareTag("Boss"))
        {
            // Enemy 컴포넌트를 가져옴
            // (Boss.cs도 Enemy.cs를 상속받으므로 GetComponent<Enemy>()로 찾아집니다)
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                // [수정됨] 넉백을 위해 2개의 인자(데미지, 총알위치)를 전달합니다.
                enemy.TakeDamage(damage, transform.position);
            }

            Destroy(gameObject); // 충돌 시 투사체 제거
        }
        // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
    }
}