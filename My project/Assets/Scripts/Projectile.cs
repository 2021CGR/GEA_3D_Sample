// ���� �̸�: Projectile.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 20f;
    public float lifeTime = 2f;
    public int damage = 25; // ����ü�� ������ ������

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    /// <summary>
    /// [������] "Enemy" �±� �Ǵ� "Boss" �±׸� Ȯ���մϴ�.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // �������������������� [������ �κ�] ��������������������
        // �ε��� ����� "Enemy" �±� �̰ų� "Boss" �±����� Ȯ��
        if (other.CompareTag("Enemy") || other.CompareTag("Boss"))
        {
            // Enemy ������Ʈ�� ������
            // (Boss.cs�� Enemy.cs�� ��ӹ����Ƿ� GetComponent<Enemy>()�� ã�����ϴ�)
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                // [������] �˹��� ���� 2���� ����(������, �Ѿ���ġ)�� �����մϴ�.
                enemy.TakeDamage(damage, transform.position);
            }

            Destroy(gameObject); // �浹 �� ����ü ����
        }
        // ���������������������������������������������������
    }
}