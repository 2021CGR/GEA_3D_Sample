## 변경 범위
- `my project/assets/scripts/2` 폴더 내의 C# 스크립트만 수정/추가
- 외부 리소스(프리팹/씬/에셋) 변경 없이, 코드 레벨 개선 중심

## 주요 개선 항목
- 아이템 픽업 품질 개선: 시야 가림 차단 확인, 자석(마그넷) 애니메이션, 반경 설정
- 인벤토리/핫바 UX 강화: 스크롤로 슬롯 이동, 선택 정보 표시, 선택 안정성 유지
- 제작(Crafting) 개선: Shift-클릭 시 다중 제작, 출력 슬롯 자동 정리, UI 동기화
- 일시정지/커서 충돌 최소화: Pause 중 커서 재잠금 방지, 뷰 전환 단축키
- 인벤토리에서 월드로 드랍: `Q`로 선택 아이템 1개 드랍(테스트용 기본 드랍 프리팹 사용)

## 파일별 변경 예정
- `ItemPickupRadius.cs` (my project/assets/scripts/2/ItemPickupRadius.cs:29–47)
  - 라인오브사이트(시야 차단) 검사 후 픽업 처리
  - `public float radius` 추가 및 `SphereCollider` 반경 적용
  - 즉시 파괴 대신 `ItemDrop.BeginPickup(target)` 호출로 짧은 흡입 애니메이션 후 파괴
- `ItemDrop.cs` (my project/assets/scripts/2/ItemDrop.cs)
  - `BeginPickup(Transform target)` 추가: 물리 비활성화, 트리거/머지 중지, 타깃으로 빠르게 이동 후 스스로 파괴
  - 픽업 중 `isPickedUp` 플래그 엄격 관리로 중복 픽업/합치기 차단
- `InventoryUI.cs` (my project/assets/scripts/2/InventoryUI.cs:67–106, 114–137)
  - `TextMeshProUGUI selectedInfoText` 추가, 현재 선택 아이템/수량 표시
  - 선택 변경 시 텍스트 동기화 로직 추가
- `PlayerHarvester.cs` (my project/assets/scripts/2/PlayerHarvester.cs:106–165, 172–245)
  - 마우스 휠로 슬롯 이동(앞/뒤) 지원, 선택 안정성 로직 유지
  - `Q` 키로 선택 아이템 1개 드랍: 간단한 `ItemDrop` 생성 및 태그/수량 설정
  - 드랍 프리팹 레퍼런스 필드 추가(테스트용 범용 드랍 프리팹)
- `CraftingUI.cs` (my project/assets/scripts/2/CraftingUI.cs:59–90, 102–126)
  - 결과 아이템 버튼 클릭 시 Shift 누르면 가능한 최대 수량까지 연속 제작
  - 제작 후 그리드/결과 슬롯 정리 및 `InventoryUI.UpdateUI()` 보장
- `PlayerController.cs` (my project/assets/scripts/2/PlayerController.cs:54–78)
  - Pause 중(`PauseMenuController.GameIsPaused`)에는 좌클릭으로 커서 재잠금하지 않도록 가드
  - `Tab`/`C`로 인벤토리/제작 뷰 전환(`CraftingUI.Instance`) 단축키 연결

## 검증 방법
- 채광 후 `ItemDrop`이 바닥에 안착, 근접 시 서로 합쳐짐 확인
- 플레이어 근처에서 픽업: 가림(블록) 존재 시 픽업 차단, 가림 없을 때 자석 애니메이션 후 인벤토리 수량 증가
- 숫자키/마우스휠로 슬롯 이동, `InventoryUI` 선택 하이라이트 및 선택 정보 텍스트 동기화 확인
- 철 2개로 철검 제작: 일반 클릭 1개 제작, Shift-클릭 시 가능한 만큼 연속 제작, UI 갱신 확인
- ESC로 Pause/Resume, Pause 중 좌클릭해도 커서 재잠금되지 않음 확인
- `Q`로 선택 아이템 드랍 후 바닥 안착/합치기/픽업 동작 확인

## 예상 영향 및 주의사항
- 모든 변경은 `my project/assets/scripts/2` 내에 한정되며, 기존 프리팹/태그(`Block`, `ItemDrop`) 가정을 유지
- 자석 애니메이션은 매우 짧게(수백 ms) 설계하여 즉시 파괴 대비 체감 개선만 제공
- 다중 제작은 현재 레시피 구조(단일 결과/정수 재료) 기준으로 안전하게 반복
- 입력 충돌을 최소화하기 위해 Pause 상태/UI 위 클릭 가드를 유지