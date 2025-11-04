using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using Cinemachine; // [제거] 시네머신 네임스페이스 제거

public class NosieVoxelMap : MonoBehaviour
{
    // ... (Block Prefabs, Map Dimensions, Noise Settings 변수들은 동일) ...
    [Header("Block Prefabs")]
    public GameObject grassPrefab;
    public GameObject dirtPrefab;
    public GameObject waterPrefab;
    public GameObject ironPrefab;
    public GameObject diamondPrefab;

    [Header("Map Dimensions")]
    public int width = 20;
    public int depth = 20;
    public int maxHeight = 16;
    public int waterLevel = 5;

    [Header("Noise Settings")]
    [SerializeField] float terrainNoiseScale = 20f;
    [SerializeField] float oreNoiseScale = 10f;
    [SerializeField] float ironThreshold = 0.7f;
    [SerializeField] float diamondThreshold = 0.85f;

    [Header("Monster Settings")]
    public GameObject monsterPrefab;
    public int numberOfMonsters = 10;

    [Header("Player Settings")]
    public GameObject playerPrefab;

    // [제거] 씬 카메라 참조 변수 제거
    // public CinemachineVirtualCamera sceneVirtualCamera; 


    private HashSet<Vector3Int> occupiedPositions = new HashSet<Vector3Int>();
    private List<Vector3Int> validSpawnPoints = new List<Vector3Int>();
    private float offsetX_ore;
    private float offsetY_ore;
    private float offsetZ_ore;


    void Start()
    {
        float offsetX_terrain = Random.Range(0f, 9999f);
        float offsetZ_terrain = Random.Range(0f, 9999f);

        offsetX_ore = Random.Range(10000f, 19999f);
        offsetY_ore = Random.Range(10000f, 19999f);
        offsetZ_ore = Random.Range(10000f, 19999f);

        GenerateTerrain(offsetX_terrain, offsetZ_terrain);
        GenerateWater();
        GeneratePlayer(); // 3단계
        GenerateMonsters(); // 4단계
    }

    /// <CodeOmitted /> 
    /// 1단계: GenerateTerrain() - (변경점 없음)
    void GenerateTerrain(float offsetX, float offsetZ)
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                float nx = (x + offsetX) / terrainNoiseScale;
                float nz = (z + offsetZ) / terrainNoiseScale;
                float noise = Mathf.PerlinNoise(nx, nz);

                int h = Mathf.FloorToInt(noise * maxHeight);
                if (h <= 0) continue;

                for (int y = 0; y < h; y++)
                {
                    if (y == h - 1)
                    {
                        PlaceBlock(grassPrefab, x, y, z);
                        if (y > waterLevel)
                        {
                            validSpawnPoints.Add(new Vector3Int(x, y + 1, z));
                        }
                    }
                    else
                    {
                        float oreNoise = Get3DNoise(x, y, z);

                        if (oreNoise > diamondThreshold)
                            PlaceBlock(diamondPrefab, x, y, z);
                        else if (oreNoise > ironThreshold)
                            PlaceBlock(ironPrefab, x, y, z);
                        else
                            PlaceBlock(dirtPrefab, x, y, z);
                    }
                    occupiedPositions.Add(new Vector3Int(x, y, z));
                }
            }
        }
    }

    /// <CodeOmitted /> 
    /// Get3DNoise() - (변경점 없음)
    float Get3DNoise(int x, int y, int z)
    {
        float nx = (x + offsetX_ore) / oreNoiseScale;
        float ny = (y + offsetY_ore) / oreNoiseScale;
        float nz = (z + offsetZ_ore) / oreNoiseScale;

        float noise_xy = Mathf.PerlinNoise(nx, ny);
        float noise_xz = Mathf.PerlinNoise(nx, nz);
        float noise_yz = Mathf.PerlinNoise(ny, nz);

        return (noise_xy + noise_xz + noise_yz) / 3f;
    }

    /// <CodeOmitted /> 
    /// 2단계: GenerateWater() - (변경점 없음)
    void GenerateWater()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                for (int y = 0; y <= waterLevel; y++)
                {
                    Vector3Int currentPos = new Vector3Int(x, y, z);
                    if (!occupiedPositions.Contains(currentPos))
                    {
                        PlaceBlock(waterPrefab, x, y, z);
                    }
                }
            }
        }
    }

    /// <summary>
    /// [수정됨] 3단계: 플레이어를 배치합니다. (카메라 연결 로직 모두 제거)
    /// </summary>
    void GeneratePlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogWarning("Player Prefab이 할당되지 않았습니다.");
            return;
        }
        if (validSpawnPoints.Count == 0)
        {
            Debug.LogError("플레이어를 스폰할 유효한 지점이 없습니다!");
            return;
        }

        int randomIndex = Random.Range(0, validSpawnPoints.Count);
        Vector3 spawnPos = validSpawnPoints[randomIndex];

        // 1. 플레이어 생성
        GameObject playerGO = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        playerGO.name = "Player";

        // --- [제거] ---
        // PlayerController를 가져오거나, 카메라를 연결하는 모든 로직을
        // 여기에서 제거합니다. 플레이어 프리팹이 스스로 처리합니다.
        // --- [제거 완료] ---

        // 2. 스폰 지점 리스트에서 제거
        validSpawnPoints.RemoveAt(randomIndex);
    }


    /// <summary>
    /// 4단계: 유효한 스폰 지점에 몬스터를 배치합니다. (변경점 없음)
    /// </summary>
    void GenerateMonsters()
    {
        if (monsterPrefab == null)
        {
            Debug.LogWarning("Monster Prefab이 할당되지 않았습니다. 몬스터를 스폰할 수 없습니다.");
            return;
        }
        if (validSpawnPoints.Count == 0)
        {
            Debug.LogWarning("유효한 몬스터 스폰 지점이 없습니다.");
            return;
        }

        int monstersToSpawn = Mathf.Min(numberOfMonsters, validSpawnPoints.Count);
        for (int i = 0; i < monstersToSpawn; i++)
        {
            int randomIndex = Random.Range(0, validSpawnPoints.Count);
            Vector3 spawnPos = validSpawnPoints[randomIndex];
            var go = Instantiate(monsterPrefab, spawnPos, Quaternion.identity, transform);
            go.name = $"Monster_{i}";
            validSpawnPoints.RemoveAt(randomIndex);
        }
    }


    /// <CodeOmitted /> 
    /// PlaceBlock() - (변경점 없음)
    private void PlaceBlock(GameObject prefabToPlace, int x, int y, int z)
    {
        var go = Instantiate(prefabToPlace, new Vector3(x, y, z), Quaternion.identity, transform);
        go.name = $"{prefabToPlace.name}_{x}_{y}_{z}";
    }
}