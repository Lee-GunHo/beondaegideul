using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineAnimation : MonoBehaviour
{
    [Header("Queue Slots Root")]
    [SerializeField] private Transform slotsRoot;   // World/QueueSlots
    private Transform[] slots;

    [Header("Entrant Prefab")]
    [SerializeField] private Entrant entrantPrefab;

    [Header("Start Fill")]
    [Tooltip("게임 시작 시 줄에 미리 채울 인원 수")]
    [SerializeField] private int startFillCount = 12;

    [Tooltip("비워두면 슬롯 위치에 바로 생성(추천). 넣으면 그 지점에서 생성 후 자리로 이동")]
    [SerializeField] private Transform prefillSpawnPoint;

    [Header("Call System")]
    [Tooltip("호출되면 맨 앞 사람이 이동할 검사대 위치")]
    [SerializeField] private Transform deskPos;

    [Tooltip("검사대에서 나갈 출구(씬 전환을 할 거면 안 써도 됨)")]
    [SerializeField] private Transform exitPoint;

    [Tooltip("deskPos 도착 판정 거리")]
    [SerializeField] private float deskArriveThreshold = 0.08f;

    [Header("After Desk")]
    [Tooltip("desk 도착하면 이벤트만 쏘고 멈출지(씬 전환용). true면 줄 당김/퇴장/보충 안 함")]
    [SerializeField] private bool stopAfterDeskEnter = true;

    [Tooltip("stopAfterDeskEnter=false일 때만 사용: desk에서 자동으로 출구로 보낼지")]
    [SerializeField] private bool autoExitAfterDesk = true;

    [Tooltip("stopAfterDeskEnter=false일 때만 사용: desk 도착 후 대기 시간")]
    [SerializeField] private float deskWaitSeconds = 0.6f;

    [Tooltip("exit 도착 판정 거리(자동 퇴장 사용 시)")]
    [SerializeField] private float exitArriveThreshold = 0.12f;

    [Header("Line Move Feel")]
    [Tooltip("앞사람이 자기 슬롯에 이 거리 안으로 들어오면 다음이 출발")]
    [SerializeField] private float followThreshold = 0.08f;

    [Tooltip("한 명 출발시키고 다음 체크까지의 텀")]
    [SerializeField] private float stepDelay = 0.08f;

    [Header("Destroy")]
    [SerializeField] private bool destroyAfterExit = true;
    [SerializeField] private float destroyDelayAfterArrive = 0.1f;

    [Header("Auto Refill")]
    [Tooltip("stopAfterDeskEnter=false일 때만 의미 있음. 한 명 나가면 startFillCount까지 자동 보충")]
    [SerializeField] private bool keepLineFilled = true;

    [Tooltip("보충 스폰 위치(없으면 맨 뒤 슬롯 근처)")]
    [SerializeField] private Transform refillSpawnPoint;

    [Tooltip("스폰 겹침 방지용 오프셋(가로 줄 기준 X). 줄이 세로면 SpawnAtBack에서 y로 바꿔도 됨")]
    [SerializeField] private float spawnOffsetX = 0.15f;

    // ✅ deskPos에 "들어간" 순간 외부에 알림(씬 전환은 바깥에서 처리)
    public event Action<Entrant> OnEntrantEnteredDesk;

    private class Node { public Entrant e; }
    private readonly List<Node> queue = new();

    private bool shifting = false;
    private bool isCalling = false;
    private Entrant currentAtDesk = null;

    private void Awake()
    {
        LoadSlots();
    }

    private void Start()
    {
        PrefillLine();
        StartCoroutine(ShiftLineRoutine());
    }

    private void LoadSlots()
    {
        if (slotsRoot == null)
        {
            Debug.LogError("[LineAnimation] slotsRoot 비어있음 (World/QueueSlots 넣어줘)");
            return;
        }

        int count = slotsRoot.childCount;
        slots = new Transform[count];
        for (int i = 0; i < count; i++) slots[i] = slotsRoot.GetChild(i);

        // Slot_0..Slot_22 이름 기준 정렬(하이어라키 순서 꼬여도 OK)
        Array.Sort(slots, (a, b) => ExtractSlotIndex(a.name).CompareTo(ExtractSlotIndex(b.name)));
    }

    private int ExtractSlotIndex(string name)
    {
        int idx = -1, mul = 1;
        for (int i = name.Length - 1; i >= 0; i--)
        {
            char c = name[i];
            if (c >= '0' && c <= '9')
            {
                if (idx < 0) idx = 0;
                idx += (c - '0') * mul;
                mul *= 10;
            }
            else if (idx >= 0) break;
        }
        return idx >= 0 ? idx : int.MaxValue;
    }

    private void PrefillLine()
    {
        if (entrantPrefab == null)
        {
            Debug.LogError("[LineAnimation] entrantPrefab 비어있음");
            return;
        }
        if (slots == null || slots.Length == 0) return;

        int count = Mathf.Clamp(startFillCount, 0, slots.Length);
        queue.Clear();

        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPos = prefillSpawnPoint != null ? prefillSpawnPoint.position : slots[i].position;
            Entrant e = Instantiate(entrantPrefab, spawnPos, Quaternion.identity);

            // 시작은 "이미 자리에 서있게"
            e.transform.position = slots[i].position;
            e.targetSlot = null;

            queue.Add(new Node { e = e });
        }
    }

    // =========================
    // 외부(사이렌 클릭)에서 호출
    // =========================
    public void CallNext()
    {
        if (deskPos == null)
        {
            Debug.LogError("[LineAnimation] deskPos 연결 필요");
            return;
        }

        if (isCalling) return;
        if (currentAtDesk != null) return;
        if (queue.Count == 0) return;

        StartCoroutine(CallNextRoutine());
    }

    private IEnumerator CallNextRoutine()
    {
        isCalling = true;

        // 맨 앞을 desk로 보냄
        currentAtDesk = queue[0].e;
        queue.RemoveAt(0);

        if (currentAtDesk != null)
        {
            currentAtDesk.targetSlot = deskPos;

            // desk 도착까지 대기
            while (currentAtDesk != null &&
                   Vector2.Distance(currentAtDesk.transform.position, deskPos.position) > deskArriveThreshold)
            {
                yield return null;
            }

            // ✅ desk에 "들어간 순간" 알림
            OnEntrantEnteredDesk?.Invoke(currentAtDesk);

            // ✅ 씬 전환용: 여기서 멈춘다
            if (stopAfterDeskEnter)
            {
                isCalling = false;
                yield break;
            }

            // 아래는 씬 전환 안 할 때만(테스트/대체 루프)
            if (deskWaitSeconds > 0f)
                yield return new WaitForSeconds(deskWaitSeconds);

            if (autoExitAfterDesk)
            {
                if (exitPoint == null)
                {
                    Debug.LogError("[LineAnimation] autoExitAfterDesk가 켜져있는데 exitPoint가 비어있음");
                }
                else
                {
                    currentAtDesk.targetSlot = exitPoint;

                    while (currentAtDesk != null &&
                           Vector2.Distance(currentAtDesk.transform.position, exitPoint.position) > exitArriveThreshold)
                    {
                        yield return null;
                    }

                    if (destroyAfterExit && currentAtDesk != null)
                    {
                        yield return new WaitForSeconds(destroyDelayAfterArrive);
                        Destroy(currentAtDesk.gameObject);
                    }
                }

                currentAtDesk = null;
            }
        }

        // 줄 당김
        yield return ShiftLineRoutine();

        // 자동 보충
        if (keepLineFilled)
        {
            yield return RefillToStartCount();
            yield return ShiftLineRoutine();
        }

        isCalling = false;
    }

    private IEnumerator RefillToStartCount()
    {
        if (entrantPrefab == null || slots == null) yield break;

        int targetCount = Mathf.Clamp(startFillCount, 0, slots.Length);

        while (queue.Count < targetCount)
        {
            Vector3 pos;
            if (refillSpawnPoint != null) pos = refillSpawnPoint.position;
            else pos = slots[Mathf.Clamp(queue.Count, 0, slots.Length - 1)].position;

            // 겹침 방지(가로 줄 기준)
            pos.x -= queue.Count * spawnOffsetX;

            Entrant e = Instantiate(entrantPrefab, pos, Quaternion.identity);
            e.targetSlot = null;

            queue.Add(new Node { e = e });

            yield return null;
        }
    }

    // =========================
    // 줄 당김 (앞사람 도착하면 다음 출발)
    // =========================
    private IEnumerator ShiftLineRoutine()
    {
        if (shifting) yield break;
        shifting = true;

        // 전부 멈춤
        for (int i = 0; i < queue.Count; i++)
            if (queue[i].e != null) queue[i].e.targetSlot = null;

        yield return null;

        for (int i = 0; i < queue.Count; i++)
        {
            if (i > 0)
            {
                Entrant prev = queue[i - 1].e;
                Transform prevSlot = slots[i - 1];

                while (prev != null && Vector2.Distance(prev.transform.position, prevSlot.position) > followThreshold)
                    yield return null;

                if (stepDelay > 0f) yield return new WaitForSeconds(stepDelay);
            }

            Entrant me = queue[i].e;
            if (me != null) me.targetSlot = slots[i];
        }

        shifting = false;
    }
}