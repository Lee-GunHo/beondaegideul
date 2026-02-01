using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AnimalishNpcVoice : MonoBehaviour
{
    [Header("Test Line")]
    [TextArea(2, 4)]
    public string line = "안녕! 오늘도 좋은 하루야."; // 대사 

    [Header("Voice Tone")]
    [Range(300f, 2500f)] public float baseFreq = 1100f;   // 기본 음 높이
    [Range(0f, 1f)] public float volume = 0.12f;

    [Header("Animal-ish Style")]
    [Range(0f, 1f)] public float brightness = 0.28f;      // 높을수록 반짝/전자
    [Range(0f, 1f)] public float wobble = 0.06f;          // 미세 흔들림
    [Range(0f, 0.2f)] public float attack = 0.01f;
    [Range(0f, 0.3f)] public float release = 0.05f;

    [Header("Speech Timing")]
    public float unitSec = 0.055f;    // 글자 1개당 길이(짧을수록 말이 빠름)
    public float gapSec = 0.012f;    // 글자 사이 간격
    public float spaceSec = 0.08f;    // 띄어쓰기/공백
    public float punctuationSec = 0.12f;  // .,!?,~ 같은 구두점

    AudioSource _src;

    void Awake()
    {
        _src = GetComponent<AudioSource>();
        _src.playOnAwake = false;
        _src.spatialBlend = 0f; // 2D
    }

    // 인스펙터 우클릭 메뉴로 실행 가능
    [ContextMenu("Play Line")]
    public void PlayLine()
    {
        Play(line);
    }

    public void Play(string text)
    {
        int sr = AudioSettings.outputSampleRate;

        // 총 길이 대략 계산(안전하게 넉넉히)
        float totalSec = 0f;
        foreach (char c in text)
        {
            if (c == ' ') totalSec += spaceSec;
            else if (IsPunctuation(c)) totalSec += punctuationSec;
            else totalSec += (unitSec + gapSec);
        }
        totalSec += 0.2f;

        int totalSamples = Mathf.CeilToInt(totalSec * sr);
        float[] data = new float[totalSamples];
        int cursor = 0;

        // 글자 → 주파수 매핑(캐릭터마다 음이 살짝 달라져서 “말하는 느낌”이 생김)
        float CharToFreq(char c)
        {
            // 한글/영문/기타 섞여도 안정적으로 변하게 간단 해시
            int h = c;
            float t = Mathf.Abs((h % 17) - 8) / 8f; // 0~1
            // baseFreq 주변에서 약간 오르내림
            return baseFreq * Mathf.Lerp(0.78f, 1.18f, t);
        }

        void AddSilence(float sec)
        {
            int n = Mathf.CeilToInt(sec * sr);
            cursor = Mathf.Min(cursor + n, data.Length);
        }

        void AddTone(float freq, float durSec)
        {
            int n = Mathf.CeilToInt(durSec * sr);
            for (int i = 0; i < n && cursor < data.Length; i++, cursor++)
            {
                float t = (float)i / sr;

                // 미세한 흔들림(동숲 느낌)
                float vib = 1f + wobble * Mathf.Sin(2f * Mathf.PI * 7f * t);

                // 기본 sine + 살짝 밝게(고조파 아주 약하게)
                float s1 = Mathf.Sin(2f * Mathf.PI * freq * vib * t);
                float s2 = Mathf.Sin(2f * Mathf.PI * (freq * 2f) * t) * 0.12f;
                float s3 = Mathf.Sin(2f * Mathf.PI * (freq * 3f) * t) * 0.05f;
                float sample = Mathf.Lerp(s1, s1 + s2 + s3, brightness);

                // attack/release
                float env = 1f;
                if (attack > 0f && t < attack) env = t / attack;
                float tail = durSec - release;
                if (release > 0f && t > tail) env = Mathf.Clamp01((durSec - t) / release);

                data[cursor] += sample * env * volume;
            }
        }

        foreach (char c in text)
        {
            if (c == ' ')
            {
                AddSilence(spaceSec);
                continue;
            }

            if (IsPunctuation(c))
            {
                // 구두점은 “짧은 끊김”으로 처리(말하는 리듬)
                AddSilence(punctuationSec);
                continue;
            }

            float f = CharToFreq(c);
            AddTone(f, unitSec);
            AddSilence(gapSec);
        }

        // 클리핑 방지
        for (int i = 0; i < data.Length; i++)
            data[i] = Mathf.Clamp(data[i], -0.9f, 0.9f);

        var clip = AudioClip.Create("NpcBeepLine", totalSamples, 1, sr, false);
        clip.SetData(data, 0);

        _src.Stop();
        _src.clip = clip;
        _src.Play();
    }

    static bool IsPunctuation(char c)
    {
        return c == '.' || c == '!' || c == '?' || c == ',' || c == '~' || c == '…';
    }
}