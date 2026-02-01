using UnityEngine;
using System;

[DisallowMultipleComponent]
public class EntrantAudioFlavor : MonoBehaviour
{
    [Header("Target (auto-find)")]
    [SerializeField] private AudioSource targetSource;
    [SerializeField] private AudioLowPassFilter lowPass;
    [SerializeField] private AudioDistortionFilter distortion;

    [Header("Per-Entrant Variation")]
    [Range(0f, 0.4f)] public float pitchJitter = 0.12f;      // 0.12면 대략 0.88~1.12
    [Range(0f, 0.5f)] public float volumeJitter = 0.15f;     // 0.15면 대략 0.85~1.0
    public Vector2 lowPassRange = new Vector2(900f, 6000f);  // 컷오프 범위
    [Range(0f, 0.7f)] public float distortionMax = 0.25f;    // 왜곡 최대

    [Header("Tuning")]
    public bool affectOnlyWhenWaiting = true; // Waiting 상태에서만 변조 적용(원하면)

    private EntrantState lastState = EntrantState.None;

    private void Awake()
    {
        // 1) targetSource 자동 탐색: 같은 GameObject에 있으면 그것, 없으면 부모/자식에서 찾기
        if (targetSource == null)
            targetSource = GetComponent<AudioSource>() ?? GetComponentInParent<AudioSource>() ?? GetComponentInChildren<AudioSource>();

        // 2) 필터 자동 탐색(없으면 null일 수 있음)
        if (lowPass == null) lowPass = GetComponent<AudioLowPassFilter>();
        if (distortion == null) distortion = GetComponent<AudioDistortionFilter>();
    }

    private void OnEnable()
    {
        EntrantManager.OnNewEntrantLoaded += HandleNewEntrant;
        EntrantManager.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        EntrantManager.OnNewEntrantLoaded -= HandleNewEntrant;
        EntrantManager.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(EntrantState state)
    {
        lastState = state;
    }

    private void HandleNewEntrant(IdentificationData data)
    {
        if (targetSource == null) return;

        if (affectOnlyWhenWaiting && lastState != EntrantState.Waiting && lastState != EntrantState.Approaching)
        {
            // 상황에 따라 적용 타이밍을 제한하고 싶으면 여기서 컷
            // (Approaching/Waiting 사이 타이밍이 꼬이면 false로 꺼도 됨)
        }

        ApplyFlavor(data);
    }

    private void ApplyFlavor(IdentificationData data)
    {
        // “사람마다 항상 같은 값”을 만들기 위한 시드: ID 기반
        int seed = (data.Entrant_ID != null) ? data.Entrant_ID.GetHashCode() : UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        var rng = new System.Random(seed);

        // 0~1 랜덤
        float r1 = (float)rng.NextDouble();
        float r2 = (float)rng.NextDouble();
        float r3 = (float)rng.NextDouble();
        float r4 = (float)rng.NextDouble();

        // pitch: 1을 중심으로 +/- jitter
        float pitch = 1f + Mathf.Lerp(-pitchJitter, pitchJitter, r1);

        // volume: 1에서 깎는 느낌(너무 튀면 단서가 되니까)
        float volume = 1f - Mathf.Lerp(0f, volumeJitter, r2);

        targetSource.pitch = pitch;
        targetSource.volume = volume;

        // lowpass(있으면)
        if (lowPass != null)
        {
            lowPass.enabled = true;
            lowPass.cutoffFrequency = Mathf.Lerp(lowPassRange.x, lowPassRange.y, r3);
        }

        // distortion(있으면)
        if (distortion != null)
        {
            distortion.enabled = true;

            // 노이즈/심박 같은 타입에만 더 강하게 주고 싶으면 data.Actual_Sound로 분기 가능
            float strength = Mathf.Lerp(0f, distortionMax, r4);
            distortion.distortionLevel = strength;
        }
        Debug.Log(
    $"[AudioFlavor] ID={data.Entrant_ID} " +
    $"pitch={targetSource.pitch:F2} vol={targetSource.volume:F2} " +
    $"LP={(lowPass ? lowPass.cutoffFrequency.ToString("F0") : "none")} " +
    $"Dist={(distortion ? distortion.distortionLevel.ToString("F2") : "none")}"
);
    }
}