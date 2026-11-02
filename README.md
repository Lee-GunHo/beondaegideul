# 번대기들

> 출입자가 정상 로봇인지, 위조·고장 로봇인지, 로봇으로 위장한 인간인지 판별하는 Unity 판별 게임 (글로벌 게임잼 출품)

![Unity](https://img.shields.io/badge/Unity-6000.0.66f2-black)
![C#](https://img.shields.io/badge/C%23-239120)
![Game Jam](https://img.shields.io/badge/Global%20Game%20Jam-2026-blue)

---

## 게임 소개

플레이어는 출입 심사관입니다. 하루에 정해진 인원의 출입자를 서류와 실제 모습, 기계음, 문자 인식 검사로 확인하고 **통과 · 거절 · 폐기** 중 하나로 판정합니다. 판정 결과에 따라 보수와 벌금이 오가고, 번 돈으로 상점에서 도구를 사서 다음 날을 준비합니다.

**판정 규칙**

| 판정 | 정답인 경우 |
|---|---|
| 통과 | 아무 문제가 없는 정상 로봇 (인간을 통과시키면 즉시 게임 오버) |
| 거절 | 로봇이지만 서류 위조·외형 불일치·얼룩이 있는 경우 |
| 폐기 | 인간이거나, 심각하게 고장 난 로봇 |

## 개발 정보

| 항목 | 내용 |
|---|---|
| 기간 | 2026.01 ~ 2026.02 (글로벌 게임잼) |
| 팀 | 6인 |
| 엔진 | Unity 6 (6000.0.66f2) |
| 언어 | C# |

## 담당 파트 — 이건호

인간·로봇 판별 시스템, 전체 UI, 아이템·상점, 저장, 게임 진행과 엔딩 분기를 담당했습니다.

### 1. 이벤트로 전달하는 판별 결과

`EntrantManager`가 출입자 상태와 판정을 관리하고, 결과는 정적 이벤트로 알립니다.

```csharp
// Assets/Script/System/Manager/EntrantManager.cs
public static event Action<EntrantState> OnStateChanged;
public static event Action<IdentificationData> OnNewEntrantLoaded;
public static event Action<bool, DecisionType> OnDecisionResult;
public static event Action OnGameOver;
```

판정 로직은 UI를 직접 알지 않고, 결과 표시·점수·연출은 이벤트를 구독해 반응합니다. 출입자 상태(`Approaching → Waiting → Verifying → Decided → Exiting`)를 enum으로 관리해, 판정이 끝난 출입자에게 같은 입력이 다시 처리되지 않게 했습니다.

### 2. 데이터로 조절하는 하루 난이도

일차마다 `StageData`(ScriptableObject)에 목표 인원, 성공 보수와 실패 벌금, 인간·서류 위조·고장 로봇 출현 확률, 정전 이벤트, 신문 기사를 넣어 두고 `StageManager`가 읽어 적용합니다. 난이도를 바꿀 때 코드를 고치지 않고 에셋 값만 조정합니다.

### 3. 씬이 바뀌어도 이어지는 진행과 저장

- `GlobalDataManager`를 싱글톤 + `DontDestroyOnLoad`로 두어, 타이틀·인게임·엔딩 씬을 오가도 돈·아이템·일차가 유지됩니다
- 진행 데이터를 `JsonUtility`로 직렬화해 `persistentDataPath`의 `SaveFile.json`에 저장하고 불러옵니다

### 4. 도구와 상점

- **세척 스프레이:** 보유 개수를 확인한 뒤 화면의 얼룩을 지우고 아이템을 소모합니다
- **스캐너:** 판정 기준과 같은 조건(인간 마스크, 심장 박동음)으로 인간 여부를 2초간 보여 줍니다
- **문자 인식 검사:** 코루틴으로 단계별 검사 연출을 보여 준 뒤 결과를 표시합니다
- 상점에서 번 돈으로 도구를 구매합니다

### 5. 엔딩 분기

`EndingManager`가 돈·일차·판정 결과를 확인해 **파산 / 인간 승인 / 일차 완료** 엔딩 씬으로 보냅니다. 돈이 0 이하가 되면 돈 UI를 붉게 바꾸는 경고 이벤트를 먼저 보낸 뒤 파산 엔딩으로 넘어갑니다.

## 배운 점

- 게임잼에서는 세부 기능보다 시작부터 엔딩까지 이어지는 플레이 흐름을 먼저 확보해야 합니다
- 판별 결과가 바뀐 뒤 관련 UI가 함께 갱신되도록 흐름을 연결해야, 내부 데이터와 플레이어에게 보이는 정보가 어긋나지 않습니다

## 폴더 구조

```
Assets/Script/
├── System/
│   ├── Manager/     판별(Entrant), 일차(Stage), 경제, 아이템·도구, 엔딩, 스토리
│   ├── Global/      씬 간 진행 데이터와 JSON 저장
│   └── DataConfig/  일차별 StageData (ScriptableObject)
├── Common/          판정·상태 enum, 출입자 데이터
├── Visuals/         출입자·엔딩 연출
└── Audio/           출입자 음성·기계음
Assets/UI/SHOP/      상점 UI와 구매 처리
```

## 실행 방법

1. Unity Hub에서 **Unity 6000.0.66f2**로 프로젝트를 엽니다
2. `Assets/Scenes/Title.unity`를 열고 Play 합니다
