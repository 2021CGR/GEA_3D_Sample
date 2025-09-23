using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{

    public GameObject enemyPrefab; // 생성할 적 프리팹

    public float spawnInterval = 3f; // 적 생성 간격

    public float spawnRange = 5f; // 생성 반경

    private float timer = 0f;
 
    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            //x,z는 랜덤, y는 고정
            Vector3 spawnPos = new Vector3(
                transform.position.x + Random.Range(-spawnRange, spawnRange),
                 transform.position.y,
                 transform.position.z + Random.Range(-spawnRange, spawnRange)
                 );

            Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            timer = 0f;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(spawnRange * 2, 1, spawnRange * 2));
    }
}
