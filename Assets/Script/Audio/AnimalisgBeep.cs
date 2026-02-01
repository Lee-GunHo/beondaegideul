using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AnimalishBeep : MonoBehaviour
{
    [Header("Beep Tone")]
    [Range(200f, 3000f)] public float baseFreq = 1100f;  // 기본 삐 주파수
    [Range(0f, 1f)] public float volume = 0.15f;

    [Header("Style (Animal Crossing-ish)")]
    [Range(0f, 1f)] public float brightness = 0.35f;     // 고음 성분(너무 높으면 날카로움)
    [Range(0f, 1f)] public float wobble = 0.08f;          // 미세 흔들림(너무 높이면 삐리삐리 과해짐)
    [Range(0f, 0.2f)] public float attack = 0.01f;        // 시작 부드럽게
    [Range(0f, 0.3f)] public float release = 0.05f;       // 끝 부드럽게

    [Header("Pattern: '삐리삐리 삐리리리리'")]
    public float unitSec = 0.12f;                         // 한 음 길이
    public float gapSec = 0.03f;                         // 음 사이 간격
    public int trillCount = 5;                            // 마지막 리리리리리 반복

    AudioSource _src;

    void Awake()
    {
        _src = GetComponent<AudioSource>();
        _src.playOnAwake = false;
        _src.spatialBlend = 0f; // 2D
    }

    [ContextMenu("Play Test Beep")]
    public void PlayTest()
    {
        // 패턴: 삐(높) 리(낮) 삐 리 | 삐(높) + (리리리리리)
        float fHi = baseFreq;
        float fLo = baseFreq * 0.75f;
        float fMidA = baseFreq * 0.9f;
        float fMidB = baseFreq * 0.78f;

        // 길이 계산
        int sr = AudioSettings.outputSampleRate;
        float totalSec =
            (unitSec + gapSec) * 4f + // 삐리삐리
            (unitSec + gapSec) * 1f + // 삐
            (unitSec + gapSec) * trillCount; // 리리리리리
        int totalSamples = Mathf.CeilToInt(totalSec * sr);

        float[] data = new float[totalSamples];
        int cursor = 0;

        // helper: tone segment
        void AddTone(float freq, float durSec)
        {
            int n = Mathf.CeilToInt(durSec * sr);
            for (int i = 0; i < n && cursor < data.Length; i++, cursor++)
            {
                float t = (float)i / sr;

                // 미세한 “동숲 느낌” 흔들림(가볍게)
                float vib = 1f + wobble * Mathf.Sin(2f * Mathf.PI * 7f * t);

                // 기본 sine + 살짝 밝게(2~3차 고조파를 아주 약하게)
                float s1 = Mathf.Sin(2f * Mathf.PI * freq * vib * t);
                float s2 = Mathf.Sin(2f * Mathf.PI * (freq * 2f) * t) * 0.15f;
                float s3 = Mathf.Sin(2f * Mathf.PI * (freq * 3f) * t) * 0.06f;
                float sample = Mathf.Lerp(s1, s1 + s2 + s3, brightness);

                // 간단 ADSR (attack/release)
                float env = 1f;
                float a = attack;
                float r = release;
                float dur = durSec;

                if (a > 0f && t < a) env = t / a;
                float tail = dur - r;
                if (r > 0f && t > tail) env = Mathf.Clamp01((dur - t) / r);

                data[cursor] += sample * env * volume;
            }
        }

        void AddGap()
        {
            int n = Mathf.CeilToInt(gapSec * sr);
            cursor = Mathf.Min(cursor + n, data.Length);
        }

        // 삐리삐리
        AddTone(fHi, unitSec); AddGap();
        AddTone(fLo, unitSec); AddGap();
        AddTone(fHi, unitSec); AddGap();
        AddTone(fLo, unitSec); AddGap();

        // 삐 + 리리리리리(트릴)
        AddTone(fHi, unitSec); AddGap();
        for (int k = 0; k < trillCount; k++)
        {
            float fk = (k % 2 == 0) ? fMidA : fMidB;
            AddTone(fk, unitSec); AddGap();
        }

        // 클리핑 방지(살짝 리미팅)
        for (int i = 0; i < data.Length; i++)
            data[i] = Mathf.Clamp(data[i], -0.9f, 0.9f);

        AudioClip clip = AudioClip.Create("AnimalishBeep", totalSamples, 1, sr, false);
        clip.SetData(data, 0);

        _src.Stop();
        _src.clip = clip;
        _src.Play();
    }
}