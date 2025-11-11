using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI; // NavMeshSurface를 사용하기 위해 추가

/// <summary>
/// 노이즈를 기반으로 복셀 맵을 생성, 스폰, NavMesh 빌드를 총괄하는 메인 스크립트입니다.
/// (플레이어 스폰 기능 복원됨)
/// </summary>
public class NosieVoxelMap : MonoBehaviour
{
    [Header("Block Prefabs")]
    public GameObject grassPrefab;   // 잔디 블록 프리팹
    public GameObject dirtPrefab;    // 흙 블록 프리팹
    public GameObject waterPrefab;   // 물 블록 프리팹
    public GameObject ironPrefab;    // 철 블록 프리팹
    public GameObject diamondPrefab; // 다이아몬드 블록 프리팹

    [Header("Map Dimensions")]
    public int width = 20;     // 맵 너비 (X축)
    public int depth = 20;     // 맵 깊이 (Z축)
    public int maxHeight = 16; // 지형의 최대 높이 (Y축)
    public int waterLevel = 5; // 해수면 높이

    [Header("Noise Settings")]
    [SerializeField] float terrainNoiseScale = 20f; // 지형 노이즈 스케일 (값이 클수록 완만)
    [SerializeField] float oreNoiseScale = 10f;     // 광물 3D 노이즈 스케일
    [SerializeField] float ironThreshold = 0.7f;    // 이 값보다 노이즈가 크면 철 생성
    [SerializeField] float diamondThreshold = 0.85f; // 이 값보다 노이즈가 크면 다이아 생성

    // [복원] 플레이어 스폰을 위한 프리팹 변수
    [Header("Player Settings")]
    public GameObject playerPrefab; // 스폰할 플레이어 프리팹

    [Header("NavMesh Settings")]
    public NavMeshSurface navMeshSurface; // 런타임에 NavMesh를 구울(Bake) 컴포넌트

    // 이미 블록이 설치된 위치를 저장 (물 생성을 위함)
    private HashSet<Vector3Int> occupiedPositions = new HashSet<Vector3Int>();

    // [복원] 플레이어가 스폰될 수 있는 유효한 지상 위치 목록
    private List<Vector3Int> validSpawnPoints = new List<Vector3Int>();

    // 광물용 3D 노이즈를 위한 랜덤 오프셋 값
    private float offsetX_ore;
    private float offsetY_ore;
    private float offsetZ_ore;

    /// <summary>
    /// 게임 시작 시 맵 생성을 총괄합니다.
    /// </summary>
    void Start()
    {
        // 1. 노이즈 오프셋 설정 (매번 다른 맵을 생성하기 위함)
        float offsetX_terrain = Random.Range(0f, 9999f); // 지형 X축 오프셋
        float offsetZ_terrain = Random.Range(0f, 9999f); // 지형 Z축 오프셋

        offsetX_ore = Random.Range(10000f, 19999f); // 광물 X축 오프셋
        offsetY_ore = Random.Range(10000f, 19999f); // 광물 Y축 오프셋
        offsetZ_ore = Random.Range(10000f, 19999f); // 광물 Z축 오프셋

        // 2. 맵 생성 (순서 중요)
        GenerateTerrain(offsetX_terrain, offsetZ_terrain); // 1. 땅 생성
        GenerateWater();                                   // 2. 물 생성
        BuildNavMesh();                                    // 3. 몬스터 길찾기 영역 생성

        // [추가] 4. 플레이어 스폰
        SpawnPlayer();
    }

    /// <summary>
    /// 지형(땅, 광물)을 생성합니다.
    /// </summary>
    void GenerateTerrain(float offsetX, float offsetZ)
    {
        // 맵 너비(x)와 깊이(z)만큼 반복
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                // 1. 펄린 노이즈를 사용해 (x, z) 위치의 지형 높이(h) 계산
                float nx = (x + offsetX) / terrainNoiseScale;
                float nz = (z + offsetZ) / terrainNoiseScale;
                float noise = Mathf.PerlinNoise(nx, nz); // 0.0 ~ 1.0 사이의 값
                int h = Mathf.FloorToInt(noise * maxHeight); // 맵 최대 높이 적용

                if (h <= 0) continue; // 높이가 0 이하면 블록 생성 안 함

                // 2. 계산된 높이(h)만큼 y축으로 블록 쌓기
                for (int y = 0; y < h; y++)
                {
                    if (y == h - 1) // 가장 윗부분(표면)
                    {
                        // 잔디 블록 설치 (체력3, 드롭1, 채굴가능)
                        PlaceBlock(grassPrefab, x, y, z, BlockType.Grass, 3, 1, true);

                        // [복원] 스폰 위치 저장 로직
                        // 물 높이(waterLevel)보다 높은 잔디 블록(맵 상단) 위를 스폰 위치로 저장
                        if (y > waterLevel)
                        {
                            validSpawnPoints.Add(new Vector3Int(x, y + 1, z)); // 블록 '위' 좌표
                        }
                    }
                    else // 지표면 아래
                    {
                        // 3D 노이즈를 사용해 광물 생성 여부 결정
                        float oreNoise = Get3DNoise(x, y, z);

                        if (oreNoise > diamondThreshold) // 다이아몬드 생성
                            PlaceBlock(diamondPrefab, x, y, z, BlockType.Diamond, 10, 1, true);
                        else if (oreNoise > ironThreshold) // 철 생성
                            PlaceBlock(ironPrefab, x, y, z, BlockType.Iron, 5, 1, true);
                        else // 흙 생성
                            PlaceBlock(dirtPrefab, x, y, z, BlockType.Dirt, 3, 1, true);
                    }
                    // 블록이 설치된 위치 기록 (나중에 물이 겹치지 않도록)
                    occupiedPositions.Add(new Vector3Int(x, y, z));
                }
            }
        }
    }

    // (Get3DNoise, GenerateWater, PlaceBlock, BuildNavMesh 함수는 변경 없습니다)
    // ... (이전 코드와 동일) ...

    /// <summary>
    /// 광물 생성을 위한 3D 노이즈 값을 반환합니다.
    /// </summary>
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

    /// <summary>
    /// 물(해수면)을 생성합니다.
    /// </summary>
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
                        PlaceBlock(waterPrefab, x, y, z, BlockType.Water, 1, 0, false);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 지정된 위치에 블록 프리팹을 생성하고, Block 컴포넌트 속성을 설정합니다.
    /// </summary>
    private void PlaceBlock(GameObject prefabToPlace, int x, int y, int z,
                            BlockType type, int maxHp, int dropCount, bool mineable)
    {
        var go = Instantiate(prefabToPlace, new Vector3(x, y, z), Quaternion.identity, transform);
        go.name = $"{type}_{x}_{y}_{z}";
        var b = go.GetComponent<Block>() ?? go.AddComponent<Block>();

        b.type = type;
        b.maxHp = maxHp;
        b.dropCount = dropCount;
        b.mineable = mineable;
    }

    /// <summary>
    /// 몬스터가 걸어다닐 NavMesh를 실시간으로 생성(Bake)합니다.
    /// </summary>
    void BuildNavMesh()
    {
        if (navMeshSurface != null)
        {
            navMeshSurface.BuildNavMesh();
            Debug.Log("NavMesh 빌드 완료.");
        }
        else
        {
            Debug.LogError("NavMeshSurface가 할당되지 않았습니다! 몬스터 AI가 작동하지 않습니다.");
        }
    }

    // [복원] SpawnPlayer() 함수
    /// <summary>
    /// 플레이어를 유효한 스폰 위치(validSpawnPoints) 중 랜덤한 곳에 스폰합니다.
    /// </summary>
    void SpawnPlayer()
    {
        // 1. 스폰 가능한 지점이 있는지, 플레이어 프리팹이 할당되었는지 확인
        if (playerPrefab == null || validSpawnPoints.Count == 0)
        {
            Debug.LogError("플레이어 프리팹이 할당되지 않았거나 스폰 위치(validSpawnPoints)가 없습니다.");
            return;
        }

        // 2. 유효한 스폰 위치 리스트(맵 상단 잔디 위)에서 랜덤한 위치 선택
        Vector3 spawnPos = validSpawnPoints[Random.Range(0, validSpawnPoints.Count)];

        // 3. CharacterController가 땅에 끼는 것을 방지하기 위해 Y축으로 살짝 위로 이동
        spawnPos.y += 0.5f;

        // 4. 플레이어 프리팹을 해당 위치에 생성
        Instantiate(playerPrefab, spawnPos, Quaternion.identity);
    }
}