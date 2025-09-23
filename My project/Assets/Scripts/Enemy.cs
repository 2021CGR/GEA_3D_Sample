using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float moveSpeed = 2f;  // 이동 속도
    public int maxHealth = 100;   // 최대 체력
    private int currentHealth;    // 현재 체력

    private Transform player;     // 플레이어 추적용

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        currentHealth = maxHealth; // 시작 시 최대 체력으로 초기화
    }

    void Update()
    {
        if (player != null)
        {
            // 플레이어까지의 방향 구하기
            Vector3 direction = (player.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
            transform.LookAt(player.position);
        }
    }

    // 외부에서 데미지를 입힐 때 호출
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("Enemy took damage: " + damage + ", Current Health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Enemy died!");
        Destroy(gameObject); // 적 오브젝트 제거
    }
}

