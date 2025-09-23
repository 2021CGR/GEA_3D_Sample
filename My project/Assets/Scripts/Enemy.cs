using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float moveSpeed = 2f;  // �̵� �ӵ�
    public int maxHealth = 100;   // �ִ� ü��
    private int currentHealth;    // ���� ü��

    private Transform player;     // �÷��̾� ������

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        currentHealth = maxHealth; // ���� �� �ִ� ü������ �ʱ�ȭ
    }

    void Update()
    {
        if (player != null)
        {
            // �÷��̾������ ���� ���ϱ�
            Vector3 direction = (player.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
            transform.LookAt(player.position);
        }
    }

    // �ܺο��� �������� ���� �� ȣ��
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
        Destroy(gameObject); // �� ������Ʈ ����
    }
}

