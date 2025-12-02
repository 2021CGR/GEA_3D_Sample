using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 맵 생성, NavMesh 빌드, 플레이어 스폰을 관리하는 월드 매니저입니다.
/// Perlin Noise 알고리즘을 사용하여 지형을 절차적으로 생성합니다.
/// </summary>
public class NosieVoxelMap : MonoBehaviour
{
    [Header("블록 프리팹 연결")]
    public GameObject grassPrefab;   // 잔디 블록
    public GameObject dirtPrefab;    // 흙 블록
    public GameObject waterPrefab;   // 물 블록
    public GameObject ironPrefab;    // 철 광석
    public GameObject diamondPrefab; // 다이아몬드

    [Header("맵 크기 설정")]
    public int width = 20;       // 맵 가로 크기
    public int depth = 20;       // 맵 세로 크기
    public int maxHeight = 16;   // 최대 높이
    public int waterLevel = 5;   // 수면 높이

    [Header("노이즈(지형) 설정")]
    [SerializeField] float terrainNoiseScale = 20f; // 지형의 굴곡 빈도 (클수록 완만함)
    [SerializeField] float oreNoiseScale = 10f;     // 광물 분포의 빈도
    [SerializeField] float ironThreshold = 0.7f;    // 철 생성 확률 (높을수록 희귀)
    [SerializeField] float diamondThreshold = 0.85f;// 다이아 생성 확률

    [Header("플레이어 및 AI")]
    public GameObject playerPrefab;
    public NavMeshSurface navMeshSurface; // NavMesh 베이크용 컴포넌트

    // 내부 변수: 블록 중복 생성 방지 및 스폰 위치 저장
    private HashSet<Vector3Int> occupiedPositions = new HashSet<Vector3Int>();
    private List<Vector3Int> validSpawnPoints = new List<Vector3Int>();

    // 노이즈 시드값 (매번 다른 맵을 만들기 위함)
    private float offsetX_ore;
    private float offsetY_ore;
    private float offsetZ_ore;

    void Start()
    {
        // 1. 랜덤 시드 생성 (매 실행마다 다른 패턴의 노이즈가 나옵니다)
        float offsetX_terrain = Random.Range(0f, 9999f);
        float offsetZ_terrain = Random.Range(0f, 9999f);

        offsetX_ore = Random.Range(10000f, 19999f);
        offsetY_ore = Random.Range(10000f, 19999f);
        offsetZ_ore = Random.Range(10000f, 19999f);

        // 2. 월드 생성 프로세스 시작
        GenerateTerrain(offsetX_terrain, offsetZ_terrain); // 지형 블록 배치
        GenerateWater();                                   // 빈 곳에 물 채우기
        BuildNavMesh();                                    // AI 이동 경로 계산
        SpawnPlayer();                                     // 플레이어 배치
    }

    /// <summary>
    /// 노이즈를 기반으로 땅과 광물을 생성합니다.
    /// </summary>
    void GenerateTerrain(float offsetX, float offsetZ)
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                // 펄린 노이즈로 해당 (x, z) 위치의 높이(y)를 계산합니다.
                float nx = (x + offsetX) / terrainNoiseScale;
                float nz = (z + offsetZ) / terrainNoiseScale;
                float noise = Mathf.PerlinNoise(nx, nz);

                // 0~1 사이의 노이즈 값을 실제 높이 정수로 변환
                int h = Mathf.FloorToInt(noise * maxHeight);

                if (h <= 0) continue; // 높이가 0 이하라면 생성 안 함

                // 바닥(0)부터 계산된 높이(h)까지 블록을 쌓아 올립니다.
                for (int y = 0; y < h; y++)
                {
                    if (y == h - 1) // 가장 꼭대기 층 (표면)
                    {
                        // 표면은 무조건 잔디
                        PlaceBlock(grassPrefab, x, y, z, BlockType.Grass, 3, 1, true);

                        // 물 높이보다 높다면 플레이어가 스폰될 수 있는 안전한 장소로 기록
                        if (y > waterLevel)
                        {
                            validSpawnPoints.Add(new Vector3Int(x, y + 1, z));
                        }
                    }
                    else // 표면 아래 (지하)
                    {
                        // 3D 노이즈를 사용하여 광물 배치 여부 결정
                        float oreNoise = Get3DNoise(x, y, z);

                        if (oreNoise > diamondThreshold)
                            PlaceBlock(diamondPrefab, x, y, z, BlockType.Diamond, 10, 1, true);
                        else if (oreNoise > ironThreshold)
                            PlaceBlock(ironPrefab, x, y, z, BlockType.Iron, 5, 1, true);
                        else
                            PlaceBlock(dirtPrefab, x, y, z, BlockType.Dirt, 3, 1, true);
                    }
                    // 블록이 생성된 위치 기록 (나중에 물 생성 시 겹치지 않게)
                    occupiedPositions.Add(new Vector3Int(x, y, z));
                }
            }
        }
    }

    /// <summary>
    /// 3차원 좌표에 대한 노이즈 값을 계산합니다. (3면의 노이즈 평균값 사용)
    /// </summary>
    float Get3DNoise(int x, int y, int z)
    {
        float nx = (x + offsetX_ore) / oreNoiseScale;
        float ny = (y + offsetY_ore) / oreNoiseScale;
        float nz = (z + offsetZ_ore) / oreNoiseScale;

        // XY, XZ, YZ 평면의 노이즈를 평균내어 3D 질감을 흉내냅니다.
        return (Mathf.PerlinNoise(nx, ny) + Mathf.PerlinNoise(nx, nz) + Mathf.PerlinNoise(ny, nz)) / 3f;
    }

    /// <summary>
    /// 지형이 없는 빈 공간 중, 수면 높이 이하인 곳에 물을 채웁니다.
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

                    // 이미 블록이 있는 곳이면 패스
                    if (!occupiedPositions.Contains(pos))
                    {
                        PlaceBlock(waterPrefab, x, y, z, BlockType.Water, 1, 0, false);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 실제 블록 오브젝트를 인스턴스화하고 속성을 설정합니다.
    /// </summary>
    void PlaceBlock(GameObject prefab, int x, int y, int z, BlockType type, int hp, int drop, bool mineable)
    {
        var go = Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity, transform);
        go.name = $"{type}_{x}_{y}_{z}"; // 디버깅하기 쉽게 이름 변경

        // 블록 스크립트 가져오기 또는 추가하기
        var b = go.GetComponent<Block>() ?? go.AddComponent<Block>();

        // 블록 속성 주입
        b.type = type;
        b.maxHp = hp;
        b.dropCount = drop;
        b.mineable = mineable;
    }

    void BuildNavMesh()
    {
        if (navMeshSurface != null) navMeshSurface.BuildNavMesh();
    }

    void SpawnPlayer()
    {
        if (playerPrefab == null || validSpawnPoints.Count == 0) return;

        // 안전한 스폰 위치 중 랜덤 선택
        Vector3 spawnPos = validSpawnPoints[Random.Range(0, validSpawnPoints.Count)];
        spawnPos.y += 0.5f; // 바닥에 끼지 않도록 살짝 띄움

        Instantiate(playerPrefab, spawnPos, Quaternion.identity);
    }
}