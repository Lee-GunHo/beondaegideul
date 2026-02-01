using UnityEngine;
using System.Collections;
using TMPro;

public class RemoteControlManager : MonoBehaviour
{
    [Header("Main Panel")]
    public GameObject remoteMainPanel;      // 리모컨 UI 전체 부모 오브젝트

    [Header("Display UI")]
    public TextMeshProUGUI monitorDisplayText;

    [Header("Thermometer UI")]
    public GameObject temperYellowObj;
    public GameObject temperOrangeObj;
    public GameObject temperRedObj;
    public TextMeshProUGUI temperText;

    [Header("Sound Check UI")]
    public GameObject soundNormalObj;
    public GameObject soundNoisyObj;
    public GameObject soundHeartObj;

    [Header("Captcha UI")]
    public GameObject captchaPassObj;
    public GameObject captchaFailedObj;
    public GameObject[] captchaTimerImages;

    private bool isCaptchaProcessing = false;

    // --- 일련번호 확인 ---
    public void OnClickSerialNumber()
    {
        var d = EntrantManager.Instance.GetCurrentData();
        if (d != null) monitorDisplayText.text = $"SERIAL ID:\n{d.Actual_ID}";
    }

    // --- 온도 확인 ---
    public void OnClickTemperature()
    {
        var d = EntrantManager.Instance.GetCurrentData();
        if (d == null) return;
        ResetAllStatusUI();

        float temp = d.Temperature;
        monitorDisplayText.text = $"TEMP: {temp:F1}C";
        if (temperText)
        {
            temperText.text = $"{temp:F1}C";
            temperText.gameObject.SetActive(true);
        }

        if (temp >= 40f) temperRedObj.SetActive(true);
        else if (temp >= 36.0f) temperOrangeObj.SetActive(true);
        else temperYellowObj.SetActive(true);
    }

    // --- 소리 확인 ---
    public void OnClickSoundCheck()
    {
        var d = EntrantManager.Instance.GetCurrentData();
        if (d == null) return;
        ResetAllStatusUI();

        switch (d.Actual_Sound)
        {
            case MachineSoundType.Normal:
                monitorDisplayText.text = "SOUND:\n<color=cyan>STABLE</color>";
                if (soundNormalObj) soundNormalObj.SetActive(true);
                break;
            case MachineSoundType.Noisy:
                monitorDisplayText.text = "SOUND:\n<color=yellow>UNSTABLE NOISE</color>";
                if (soundNoisyObj) soundNoisyObj.SetActive(true);
                break;
            case MachineSoundType.Heartbeat:
                monitorDisplayText.text = "SOUND:\n<color=red>BIO-RESPONSE!!</color>";
                if (soundHeartObj) soundHeartObj.SetActive(true);
                break;
        }
    }

    // --- 캡챠 인증 버튼 클릭 ---
    public void OnClickCaptcha()
    {
        if (isCaptchaProcessing) return;
        var d = EntrantManager.Instance.GetCurrentData();
        if (d == null) return;

        StartCoroutine(CaptchaProcessRoutine(d));
    }

    // --- 캡챠 프로세스: 분석 -> 결과 노출 -> 1초 뒤 비활성화 ---
    private IEnumerator CaptchaProcessRoutine(IdentificationData d)
    {
        isCaptchaProcessing = true;
        ResetAllStatusUI();

        monitorDisplayText.text = "ANALYZING...";

        // 1. 타이머 진행
        float totalTime = (d.CaptchaTime > 0) ? d.CaptchaTime : 5.0f;
        float stepTime = totalTime / captchaTimerImages.Length;

        for (int i = 0; i < captchaTimerImages.Length; i++)
        {
            if (i > 0) captchaTimerImages[i - 1].SetActive(false);
            if (captchaTimerImages[i] != null) captchaTimerImages[i].SetActive(true);
            yield return new WaitForSeconds(stepTime);
        }

        if (captchaTimerImages.Length > 0)
            captchaTimerImages[captchaTimerImages.Length - 1].SetActive(false);

        // 2. 최종 결과 표시
        if (d.Actual_Captcha == CaptchaResult.Pass)
        {
            monitorDisplayText.text = "CAPTCHA:\n<color=cyan>VERIFIED</color>";
            if (captchaPassObj) captchaPassObj.SetActive(true);
        }
        else
        {
            monitorDisplayText.text = "CAPTCHA:\n<color=red>DENIED</color>";
            if (captchaFailedObj) captchaFailedObj.SetActive(true);
        }

        // 3. 1초 대기 (결과 확인 시간)
        yield return new WaitForSeconds(1.0f);

        // 4. 결과 정리 및 리모컨 비활성화 (없애기)
        ResetAllStatusUI();
        monitorDisplayText.text = "READY...";

        if (remoteMainPanel != null)
        {
            remoteMainPanel.SetActive(false); // 리모컨 창 전체를 비활성화
        }

        isCaptchaProcessing = false;
    }

    public void ResetAllStatusUI()
    {
        GameObject[] uiElements = {
            temperYellowObj, temperOrangeObj, temperRedObj,
            soundNormalObj, soundNoisyObj, soundHeartObj,
            captchaPassObj, captchaFailedObj
        };

        foreach (var obj in uiElements) if (obj) obj.SetActive(false);
        if (temperText) temperText.gameObject.SetActive(false);
        foreach (var img in captchaTimerImages) if (img) img.SetActive(false);
    }

    public void OnExitRemoteMenu()
    {
        StopAllCoroutines();
        isCaptchaProcessing = false;
        ResetAllStatusUI();
        if (remoteMainPanel) remoteMainPanel.SetActive(false);
    }
}