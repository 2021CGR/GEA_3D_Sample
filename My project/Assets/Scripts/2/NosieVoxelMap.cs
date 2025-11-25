using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 맵 생성, NavMesh 빌드, 플레이어 스폰을 관리하는 월드 매니저입니다.
/// </summary>
public class NosieVoxelMap : MonoBehaviour
{
    [Header("블록 프리팹")]
    public GameObject grassPrefab;
    public GameObject dirtPrefab;
    public GameObject waterPrefab;
    public GameObject ironPrefab;
    public GameObject diamondPrefab;

    [Header("맵 크기")]
    public int width = 20;
    public int depth = 20;
    public int maxHeight = 16;
    public int waterLevel = 5;

    [Header("노이즈 설정")]
    [SerializeField] float terrainNoiseScale = 20f;
    [SerializeField] float oreNoiseScale = 10f;
    [SerializeField] float ironThreshold = 0.7f;
    [SerializeField] float diamondThreshold = 0.85f;

    [Header("플레이어 및 네비게이션")]
    public GameObject playerPrefab;
    public NavMeshSurface navMeshSurface;

    // 내부 변수
    private HashSet<Vector3Int> occupiedPositions = new HashSet<Vector3Int>();
    private List<Vector3Int> validSpawnPoints = new List<Vector3Int>();

    private float offsetX_ore;
    private float offsetY_ore;
    private float offsetZ_ore;

    void Start()
    {
        // 1. 랜덤 시드 생성 (매번 다른 맵)
        float offsetX_terrain = Random.Range(0f, 9999f);
        float offsetZ_terrain = Random.Range(0f, 9999f);

        offsetX_ore = Random.Range(10000f, 19999f);
        offsetY_ore = Random.Range(10000f, 19999f);
        offsetZ_ore = Random.Range(10000f, 19999f);

        // 2. 지형 생성 단계
        GenerateTerrain(offsetX_terrain, offsetZ_terrain);
        GenerateWater();
        BuildNavMesh(); // 몬스터 길찾기 데이터 굽기
        SpawnPlayer();  // 플레이어 생성
    }

    void GenerateTerrain(float offsetX, float offsetZ)
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                // 펄린 노이즈로 높이 결정
                float nx = (x + offsetX) / terrainNoiseScale;
                float nz = (z + offsetZ) / terrainNoiseScale;
                float noise = Mathf.PerlinNoise(nx, nz);
                int h = Mathf.FloorToInt(noise * maxHeight);

                if (h <= 0) continue;

                for (int y = 0; y < h; y++)
                {
                    if (y == h - 1) // 표면
                    {
                        PlaceBlock(grassPrefab, x, y, z, BlockType.Grass, 3, 1, true);

                        // 물 위쪽이면 스폰 가능 위치로 저장
                        if (y > waterLevel)
                        {
                            validSpawnPoints.Add(new Vector3Int(x, y + 1, z));
                        }
                    }
                    else // 지하
                    {
                        float oreNoise = Get3DNoise(x, y, z);
                        if (oreNoise > diamondThreshold) PlaceBlock(diamondPrefab, x, y, z, BlockType.Diamond, 10, 1, true);
                        else if (oreNoise > ironThreshold) PlaceBlock(ironPrefab, x, y, z, BlockType.Iron, 5, 1, true);
                        else PlaceBlock(dirtPrefab, x, y, z, BlockType.Dirt, 3, 1, true);
                    }
                    occupiedPositions.Add(new Vector3Int(x, y, z));
                }
            }
        }
    }

    float Get3DNoise(int x, int y, int z)
    {
        float nx = (x + offsetX_ore) / oreNoiseScale;
        float ny = (y + offsetY_ore) / oreNoiseScale;
        float nz = (z + offsetZ_ore) / oreNoiseScale;
        return (Mathf.PerlinNoise(nx, ny) + Mathf.PerlinNoise(nx, nz) + Mathf.PerlinNoise(ny, nz)) / 3f;
    }

    void GenerateWater()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                for (int y = 0; y <= waterLevel; y++)
                {
                    Vector3Int pos = new Vector3Int(x, y, z);
                    if (!occupiedPositions.Contains(pos))
                    {
                        PlaceBlock(waterPrefab, x, y, z, BlockType.Water, 1, 0, false);
                    }
                }
            }
        }
    }

    void PlaceBlock(GameObject prefab, int x, int y, int z, BlockType type, int hp, int drop, bool mineable)
    {
        var go = Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity, transform);
        go.name = $"{type}_{x}_{y}_{z}";
        var b = go.GetComponent<Block>() ?? go.AddComponent<Block>();
        b.type = type; b.maxHp = hp; b.dropCount = drop; b.mineable = mineable;
    }

    void BuildNavMesh()
    {
        if (navMeshSurface != null) navMeshSurface.BuildNavMesh();
    }

    void SpawnPlayer()
    {
        if (playerPrefab == null || validSpawnPoints.Count == 0) return;
        Vector3 spawnPos = validSpawnPoints[Random.Range(0, validSpawnPoints.Count)];
        spawnPos.y += 0.5f; // 끼임 방지
        Instantiate(playerPrefab, spawnPos, Quaternion.identity);
    }
}