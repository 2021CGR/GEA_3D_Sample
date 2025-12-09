using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 지형 생성, NavMesh 빌드, 플레이어 스폰을 담당하는 월드 관리자입니다.
/// Perlin Noise 기반으로 지형과 광물 분포를 생성합니다.
/// </summary>
public class NosieVoxelMap : MonoBehaviour
{
    [Header("블록 프리팹 참조")]
    public GameObject grassPrefab;   // 잔디
    public GameObject dirtPrefab;    // 흙
    public GameObject waterPrefab;   // 물
    public GameObject ironPrefab;    // 철
    public GameObject diamondPrefab; // 다이아몬드

    [Header("맵 크기 설정")]
    public int width = 20;       // 가로 크기
    public int depth = 20;       // 세로 크기
    public int maxHeight = 16;   // 최대 높이
    public int waterLevel = 5;   // 물 높이

    [Header("노이즈(광물) 설정")]
    [SerializeField] float terrainNoiseScale = 20f; // 지형 노이즈 스케일(값이 클수록 완만)
    [SerializeField] float oreNoiseScale = 10f;     // 광물 노이즈 스케일
    [SerializeField] float ironThreshold = 0.7f;    // 철 생성 임계값
    [SerializeField] float diamondThreshold = 0.85f;// 다이아 생성 임계값

    [Header("플레이어 및 AI")]
    public GameObject playerPrefab;
    public NavMeshSurface navMeshSurface; // NavMesh ����ũ�� ������Ʈ

    // 점유 좌표: 중복 생성 방지 및 스폰 위치 계산
    private HashSet<Vector3Int> occupiedPositions = new HashSet<Vector3Int>();
    private List<Vector3Int> validSpawnPoints = new List<Vector3Int>();

    // 광물 노이즈 오프셋(각 축마다 다른 난수 적용)
    private float offsetX_ore;
    private float offsetY_ore;
    private float offsetZ_ore;

    void Start()
    {
        // 1. 오프셋 난수 초기화(각 실행마다 다른 지형/광물 배치)
        float offsetX_terrain = Random.Range(0f, 9999f);
        float offsetZ_terrain = Random.Range(0f, 9999f);

        offsetX_ore = Random.Range(10000f, 19999f);
        offsetY_ore = Random.Range(10000f, 19999f);
        offsetZ_ore = Random.Range(10000f, 19999f);

        // 2. 생성 순서
        GenerateTerrain(offsetX_terrain, offsetZ_terrain); // 지형 블록 배치
        GenerateWater();                                   // 물 채우기
        BuildNavMesh();                                    // AI 이동 경로 빌드
        SpawnPlayer();                                     // 플레이어 스폰
    }

    /// <summary>
    /// 지형(블록)을 생성합니다.
    /// </summary>
    void GenerateTerrain(float offsetX, float offsetZ)
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                // Perlin Noise로 해당 (x, z)의 높이(y)를 계산합니다.
                float nx = (x + offsetX) / terrainNoiseScale;
                float nz = (z + offsetZ) / terrainNoiseScale;
                float noise = Mathf.PerlinNoise(nx, nz);

                // 0~1 값을 최대 높이에 매핑해 정수 높이로 변환
                int h = Mathf.FloorToInt(noise * maxHeight);

                if (h <= 0) continue; // 높이가 0 이하면 스킵

                // 바닥(0)부터 높이(h)까지 블록을 쌓습니다.
                for (int y = 0; y < h; y++)
                {
                    if (y == h - 1) // 최상단 레이어(표면)
                    {
                        // 표면은 잔디
                        PlaceBlock(grassPrefab, x, y, z, BlockType.Grass, 3, 1, true);

                        // 수면보다 높으면 플레이어 스폰 가능한 좌표 저장
                        if (y > waterLevel)
                        {
                            validSpawnPoints.Add(new Vector3Int(x, y + 1, z));
                        }
                    }
                    else // 표면 아래(지하)
                    {
                        // 3D 노이즈를 이용해 광물 배치 결정
                        float oreNoise = Get3DNoise(x, y, z);

                        if (oreNoise > diamondThreshold)
                            PlaceBlock(diamondPrefab, x, y, z, BlockType.Diamond, 10, 1, true);
                        else if (oreNoise > ironThreshold)
                            PlaceBlock(ironPrefab, x, y, z, BlockType.Iron, 5, 1, true);
                        else
                            PlaceBlock(dirtPrefab, x, y, z, BlockType.Dirt, 3, 1, true);
                    }
                    // 점유 좌표 기록(물 채우기 시 중복 방지)
                    occupiedPositions.Add(new Vector3Int(x, y, z));
                }
            }
        }
    }

    /// <summary>
    /// 3축 좌표 기반으로 광물 노이즈를 계산합니다.
    /// (여러 평면 노이즈를 평균)
    /// </summary>
    float Get3DNoise(int x, int y, int z)
    {
        float nx = (x + offsetX_ore) / oreNoiseScale;
        float ny = (y + offsetY_ore) / oreNoiseScale;
        float nz = (z + offsetZ_ore) / oreNoiseScale;

        // XY, XZ, YZ 평면 노이즈를 평균내어 3D 값으로 만듭니다.
        return (Mathf.PerlinNoise(nx, ny) + Mathf.PerlinNoise(nx, nz) + Mathf.PerlinNoise(ny, nz)) / 3f;
    }

    /// <summary>
    /// 물 높이 이하의 빈 공간에 물을 채웁니다.
    /// </summary>
    void GenerateWater()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                for (int y = 0; y <= waterLevel; y++)
                {
                    Vector3Int pos = new Vector3Int(x, y, z);

                    // 이미 블록이 존재하면 생략
                    if (!occupiedPositions.Contains(pos))
                    {
                        PlaceBlock(waterPrefab, x, y, z, BlockType.Water, 1, 0, false);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 블록 프리팹을 인스턴스화하고 속성을 설정합니다.
    /// </summary>
    void PlaceBlock(GameObject prefab, int x, int y, int z, BlockType type, int hp, int drop, bool mineable)
    {
        var go = Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity, transform);
        go.name = $"{type}_{x}_{y}_{z}"; // 디버깅용 이름

        // Block 컴포넌트가 없으면 추가
        var b = go.GetComponent<Block>() ?? go.AddComponent<Block>();

        // 속성 설정
        b.type = type;
        b.maxHp = hp;
        b.dropCount = drop;
        b.mineable = mineable;
    }

    void BuildNavMesh()
    {
        if (navMeshSurface != null) navMeshSurface.BuildNavMesh(); // NavMesh 빌드
    }

    void SpawnPlayer()
    {
        if (playerPrefab == null || validSpawnPoints.Count == 0) return;

        // 저장된 스폰 좌표에서 무작위 선택
        Vector3 spawnPos = validSpawnPoints[Random.Range(0, validSpawnPoints.Count)];
        spawnPos.y += 0.5f; // 수면 위에서 안전히 스폰되도록 오프셋

        Instantiate(playerPrefab, spawnPos, Quaternion.identity);
    }
}
