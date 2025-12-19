using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement; // 씬 전환용

/// <summary>
/// 지형 생성, NavMesh 빌드, 플레이어 스폰을 담당하는 월드 관리자입니다.
/// Perlin Noise 기반으로 지형과 광물 분포를 생성합니다.
/// </summary>
public class NosieVoxelMap : MonoBehaviour
{
    [Header("블록 프리팹 참조")]
    public GameObject grassPrefab;   // 잔디
    public GameObject dirtPrefab;    // 흙
    public GameObject stonePrefab;   // 돌 (신규)
    public GameObject waterPrefab;   // 물
    public GameObject ironPrefab;    // 철
    public GameObject diamondPrefab; // 다이아몬드
    public GameObject sandPrefab;    // 모래 (사막)
    public GameObject cactusPrefab;  // 선인장 (사막)
    public GameObject snowPrefab;    // 눈 (빙하)
    public GameObject icePrefab;     // 얼음 (빙하)

    public enum MapBiome { Normal, Desert, Glacier }

    // 현재 활성화된(살아있는) 적 수
    private int activeEnemyCount = 0;

    /// <summary>
    /// 외부에서 적 리스폰 요청
    /// </summary>
    public void RespawnEnemy()
    {
        StartCoroutine(RespawnEnemyRoutine());
    }

    IEnumerator RespawnEnemyRoutine()
    {
        yield return new WaitForSeconds(3f); // 3초 뒤 리스폰

        if (enemyPrefab == null || validSpawnPoints.Count == 0) yield break;

        // 스폰 가능한 위치 중 하나를 랜덤으로 고름
        Vector3Int pos = validSpawnPoints[Random.Range(0, validSpawnPoints.Count)];
        Vector3 spawnPos = new Vector3(pos.x, pos.y + 0.5f, pos.z);

        if (useNavMesh)
        {
            if (NavMesh.SamplePosition(spawnPos, out var hit, 2f, NavMesh.AllAreas))
            {
                spawnPos = hit.position;
            }
        }
        else
        {
            if (Physics.Raycast(spawnPos + Vector3.up * 5f, Vector3.down, out var groundHit, 10f))
            {
                spawnPos = groundHit.point + Vector3.up * 0.05f;
            }
        }

        var enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        var agent = enemy.GetComponent<NavMeshAgent>();
        if (useNavMesh && agent != null)
        {
            agent.Warp(spawnPos);
        }
        Debug.Log("[NosieVoxelMap] 적 리스폰 완료");
    }

    public void RegisterEnemy()
    {
        activeEnemyCount++;
    }

    public void UnregisterEnemy()
    {
        activeEnemyCount--;
        if (activeEnemyCount < 0) activeEnemyCount = 0;
    }

    /// <summary>
    /// 적이 죽었을 때 호출. 
    /// 사막 맵 이상에서는 모든 적 처치 시 다음 스테이지로 이동.
    /// </summary>
    public void NotifyEnemyKilled()
    {
        UnregisterEnemy();
        Debug.Log($"[Map] 적 처치됨. 남은 적: {activeEnemyCount}");

        if (activeEnemyCount <= 0)
        {
            if (currentBiome == MapBiome.Desert)
            {
                Debug.Log("🎉 모든 적 처치 완료! 빙하 맵으로 이동합니다.");
                // 인벤토리 저장
                var inv = FindObjectOfType<Inventory>();
                if (inv != null) inv.SyncToGlobal();
                
                SceneManager.LoadScene("Map3"); 
            }
            else if (currentBiome == MapBiome.Glacier)
            {
                Debug.Log("🏆 모든 적 처치 완료! 게임 클리어!");
                
                // [수정] 보스를 잡았을 때도 게임 클리어
                // 인벤토리 저장(선택사항)
                var inv = FindObjectOfType<Inventory>();
                if (inv != null) inv.SyncToGlobal();

                // 게임 클리어 UI나 씬으로 이동할 수 있습니다.
                // 여기서는 로그만 출력하고 종료를 가정합니다.
                // SceneManager.LoadScene("EndScene"); 
            }
        }
    }

    [Header("바이옴 설정")]
    public MapBiome currentBiome = MapBiome.Normal;

    [Header("맵 크기 설정")]
    public int width = 20;       // 가로 크기
    public int depth = 20;       // 세로 크기
    public int maxHeight = 16;   // 최대 높이
    public int waterLevel = 5;   // 물 높이
    [Tooltip("지하 깊이 (0 이하로 몇 칸 더 생성할지)")]
    public int bedrockDepth = 5; // 기본값 5칸 더 깊게 생성

    [Header("노이즈(광물) 설정")]
    [SerializeField] float terrainNoiseScale = 20f; // 지형 노이즈 스케일(값이 클수록 완만)
    [SerializeField] float oreNoiseScale = 10f;     // 광물 노이즈 스케일
    [SerializeField] float ironThreshold = 0.7f;    // 철 생성 임계값
    [SerializeField] float diamondThreshold = 0.85f;// 다이아 생성 임계값

    [Header("플레이어 및 AI")]
    public GameObject playerPrefab;
    public GameObject enemyPrefab; // 적 프리팹
    public NavMeshSurface navMeshSurface; // NavMesh 빌드용 컴포넌트
    public bool useNavMesh = false;

    // 싱글톤 접근
    public static NosieVoxelMap Instance;

    [Tooltip("적 생성 수")]
    public int enemyCount = 3;
    [Tooltip("맵 생성 후 적 스폰까지 지연 시간(초)")]
    public float enemySpawnDelay = 3f;

    // 점유 좌표: 중복 생성 방지 및 스폰 위치 계산
    private HashSet<Vector3Int> occupiedPositions = new HashSet<Vector3Int>();
    private List<Vector3Int> validSpawnPoints = new List<Vector3Int>();

    // 광물 노이즈 오프셋(각 축마다 다른 난수 적용)
    private float offsetX_ore;
    private float offsetY_ore;
    private float offsetZ_ore;

    void Awake()
    {
        Instance = this;
    }

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

        StartCoroutine(SpawnEnemiesRoutine());             // 적 지연 스폰
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

                if (h <= 0) h = 1; // 최소 1칸은 보장

                // 바닥(-bedrockDepth)부터 높이(h)까지 블록을 쌓습니다.
                for (int y = -bedrockDepth; y < h; y++)
                {
                    if (y == h - 1) // 최상단 레이어(표면)
                    {
                        if (currentBiome == MapBiome.Desert)
                        {
                            PlaceBlock(sandPrefab, x, y, z, BlockType.Sand, 3, 1, true);
                            // 사막: 가끔 선인장 생성 (수면 위)
                            if (y > waterLevel && Random.value < 0.02f)
                            {
                                PlaceBlock(cactusPrefab, x, y + 1, z, BlockType.Cactus, 2, 1, true);
                                occupiedPositions.Add(new Vector3Int(x, y + 1, z));
                            }
                        }
                        else if (currentBiome == MapBiome.Glacier)
                        {
                            PlaceBlock(snowPrefab, x, y, z, BlockType.Snow, 3, 1, true);
                        }
                        else
                        {
                            PlaceBlock(grassPrefab, x, y, z, BlockType.Grass, 3, 1, true);
                        }

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

                        if (currentBiome == MapBiome.Normal)
                        {
                            // 일반: 돌(Stone)만 생성 (철/다이아 없음)
                            // oreNoise가 0.5 이상이면 돌, 아니면 흙
                            if (oreNoise > 0.5f)
                                PlaceBlock(stonePrefab, x, y, z, BlockType.Stone, 4, 1, true);
                            else
                                PlaceBlock(dirtPrefab, x, y, z, BlockType.Dirt, 3, 1, true);
                        }
                        else if (currentBiome == MapBiome.Desert)
                        {
                            // 사막: 철, 돌 생성 (다이아 없음)
                            if (oreNoise > ironThreshold) // 0.7
                                PlaceBlock(ironPrefab, x, y, z, BlockType.Iron, 5, 1, true);
                            else if (oreNoise > 0.5f)
                                PlaceBlock(stonePrefab, x, y, z, BlockType.Stone, 4, 1, true);
                            else
                                PlaceBlock(sandPrefab, x, y, z, BlockType.Sand, 3, 1, true);
                        }
                        else if (currentBiome == MapBiome.Glacier)
                        {
                            // 빙하: 다이아(높은 확률), 철, 돌 생성
                            // 다이아 확률을 높이기 위해 임계값을 낮춤 (예: 0.85 -> 0.75)
                            float glacierDiamondThreshold = 0.75f;

                            if (oreNoise > glacierDiamondThreshold)
                                PlaceBlock(diamondPrefab, x, y, z, BlockType.Diamond, 10, 1, true);
                            else if (oreNoise > 0.65f) // 철도 약간 더 잘 나오게
                                PlaceBlock(ironPrefab, x, y, z, BlockType.Iron, 5, 1, true);
                            else if (oreNoise > 0.4f)
                                PlaceBlock(stonePrefab, x, y, z, BlockType.Stone, 4, 1, true);
                            else
                                PlaceBlock(snowPrefab, x, y, z, BlockType.Snow, 3, 1, true); // 혹은 얼음
                        }
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
                // 물도 0부터 시작하거나, 필요하면 -bedrockDepth부터 채울 수도 있지만
                // 보통 물은 해수면(0) 이상부터 채우는 것이 자연스럽습니다.
                // 여기서는 기존대로 0부터 채우되, 빈 공간인지 확인합니다.
                for (int y = 0; y <= waterLevel; y++)
                {
                    Vector3Int pos = new Vector3Int(x, y, z);

                    // 이미 블록이 존재하면 생략
                    if (!occupiedPositions.Contains(pos))
                    {
                        if (currentBiome == MapBiome.Glacier)
                            PlaceBlock(icePrefab, x, y, z, BlockType.Ice, 3, 1, true); // 빙하는 얼음
                        else
                            PlaceBlock(waterPrefab, x, y, z, BlockType.Water, 1, 0, false); // 나머지는 물
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

        // 물은 NavMesh에서 통행 불가로 설정
        if (type == BlockType.Water)
        {
            var mod = go.GetComponent<NavMeshModifier>() ?? go.AddComponent<NavMeshModifier>();
            mod.overrideArea = true;
            mod.area = NavMesh.GetAreaFromName("Not Walkable");
        }
    }

    void BuildNavMesh()
    {
        if (!useNavMesh || navMeshSurface == null) return;

        // 전체 맵을 정확히 커버하도록 NavMeshSurface 범위/수집 방식을 설정
        navMeshSurface.collectObjects = CollectObjects.Children;
        navMeshSurface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        navMeshSurface.layerMask = ~0; // 모든 레이어 포함
        
        // 맵 크기에 맞춘 Bounds 설정 (로컬 좌표 기준)
        // 높이는 -bedrockDepth부터 maxHeight까지 커버해야 함
        float totalHeight = maxHeight + bedrockDepth;
        float centerY = (maxHeight - bedrockDepth) / 2f;
        
        navMeshSurface.center = new Vector3(width / 2f, centerY, depth / 2f);
        navMeshSurface.size = new Vector3(width, totalHeight, depth);

        // 작은 타일과 분절로 인해 구멍이 생기는 것을 줄이기 위해 타일/복셀 설정을 보수적으로 조정
        navMeshSurface.overrideTileSize = true;
        navMeshSurface.tileSize = 64;
        navMeshSurface.overrideVoxelSize = true;
        navMeshSurface.voxelSize = 0.2f;

        navMeshSurface.BuildNavMesh(); // NavMesh 빌드
    }

    void SpawnPlayer()
    {
        if (playerPrefab == null || validSpawnPoints.Count == 0) return;

        // 저장된 스폰 좌표에서 무작위 선택
        Vector3 spawnPos = validSpawnPoints[Random.Range(0, validSpawnPoints.Count)];
        spawnPos.y += 0.5f; // 수면 위에서 안전히 스폰되도록 오프셋

        Instantiate(playerPrefab, spawnPos, Quaternion.identity);
    }

    /// <summary>
    /// 지정된 지연 시간 후, 유효한 스폰 위치에 적들을 무작위로 생성합니다.
    /// </summary>
    IEnumerator SpawnEnemiesRoutine()
    {
        if (enemyPrefab == null || validSpawnPoints.Count == 0)
        {
            Debug.LogWarning("[NoiseVoxelMap] 적 프리팹이 없거나 스폰 지점이 없습니다.");
            yield break;
        }

        Debug.Log($"[NoiseVoxelMap] {enemySpawnDelay}초 후 적 {enemyCount}마리 스폰 예정...");
        yield return new WaitForSeconds(enemySpawnDelay);

        int spawnedCount = 0;
        int safetyCount = 0; // 무한루프 방지

        while (spawnedCount < enemyCount && safetyCount < 100)
        {
            // 스폰 가능한 위치 중 하나를 랜덤으로 고름
            Vector3Int pos = validSpawnPoints[Random.Range(0, validSpawnPoints.Count)];
            Vector3 spawnPos = new Vector3(pos.x, pos.y + 0.5f, pos.z);

            if (useNavMesh)
            {
                // NavMesh에서 가장 가까운 유효 지점 샘플링
                if (NavMesh.SamplePosition(spawnPos, out var hit, 2f, NavMesh.AllAreas))
                {
                    spawnPos = hit.position;
                }
            }
            else
            {
                // 지형 표면으로 레이캐스트하여 안전 위치 결정
                if (Physics.Raycast(spawnPos + Vector3.up * 5f, Vector3.down, out var groundHit, 10f))
                {
                    spawnPos = groundHit.point + Vector3.up * 0.05f;
                }
            }

            // 적 생성 후 NavMesh에 워프하여 지면에 정확히 붙임 + 가장자리 안전거리 확보
            var enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            var agent = enemy.GetComponent<NavMeshAgent>();
            if (useNavMesh && agent != null)
            {
                agent.Warp(spawnPos);
                // 가장자리에서 너무 가깝다면 조금 안쪽으로 밀어넣기
                if (NavMesh.FindClosestEdge(agent.transform.position, out var edgeHit, NavMesh.AllAreas))
                {
                    float safe = Mathf.Max(agent.radius * 1.2f, 0.4f);
                    if (edgeHit.distance < safe)
                    {
                        Vector3 adjust = edgeHit.normal * (safe - edgeHit.distance);
                        Vector3 safePos = agent.transform.position + adjust;
                        if (NavMesh.SamplePosition(safePos, out var safeHit, 1.0f, NavMesh.AllAreas))
                        {
                            agent.Warp(safeHit.position);
                        }
                    }
                }
            }
            spawnedCount++;
            safetyCount++;
        }

        Debug.Log($"[NoiseVoxelMap] 적 {spawnedCount}마리 스폰 완료!");
    }
}
