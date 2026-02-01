using System;
using UnityEngine;
using UnityEngine.UI;

public class UI_RemoteSelectMover : MonoBehaviour
{
    [Header("Remote Panel (리모컨 패널)")]
    public RectTransform remotePanel;
    public Vector2 remoteDownOffset = new Vector2(0f, -300f); // 리모컨 내려가는 거리

    [Header("Options (버튼 -> 여러 이미지 매핑)")]
    public Option[] options;

    [Serializable]
    public class MoveTarget
    {
        public RectTransform image;                 // 이동할 이미지
        public Vector2 offset = new Vector2(0f, 300f); // 이동 오프셋(각 이미지별)
    }

    [Serializable]
    public class Option
    {
        public Button button;           // 리모컨 버튼
        public MoveTarget[] targets;    // 버튼을 누르면 움직일 이미지들(1개 이상)
    }

    [Header("Optional: Remote Open Button")]
    public Button openRemoteButton; // 리모컨 다시 올리는 버튼(선택)
    public bool resetImageWhenOpenRemote = true;

    private Vector2 remoteStartPos;
    private bool remoteIsDown = false;

    // 각 옵션마다 "원래 위치"를 저장 (여러 이미지라서 2중 배열처럼 저장)
    private Vector2[][] startPosByOption;
    private int currentIndex = -1;

    void Awake()
    {
        if (remotePanel == null)
            remotePanel = GetComponent<RectTransform>();

        remoteStartPos = remotePanel.anchoredPosition;

        // 시작 위치 저장 + 버튼 이벤트 연결
        startPosByOption = new Vector2[options != null ? options.Length : 0][];

        for (int i = 0; i < options.Length; i++)
        {
            int idx = i;

            int targetCount = (options[idx].targets != null) ? options[idx].targets.Length : 0;
            startPosByOption[idx] = new Vector2[targetCount];

            // 각 타겟 이미지의 시작 위치 저장
            for (int t = 0; t < targetCount; t++)
            {
                var mt = options[idx].targets[t];
                if (mt != null && mt.image != null)
                    startPosByOption[idx][t] = mt.image.anchoredPosition;
            }

            // 버튼 클릭 연결
            if (options[idx].button != null)
                options[idx].button.onClick.AddListener(() => OnRemoteButtonClicked(idx));
        }

        if (openRemoteButton != null)
            openRemoteButton.onClick.AddListener(OpenRemote);
    }

    private void OnRemoteButtonClicked(int index)
    {
        // 1) 이전 옵션으로 이동해둔 이미지들을 원래대로
        ResetCurrentOptionTargets();

        // 2) 이번 옵션의 이미지들 이동
        var opt = options[index];
        if (opt.targets != null)
        {
            for (int t = 0; t < opt.targets.Length; t++)
            {
                var mt = opt.targets[t];
                if (mt == null || mt.image == null) continue;

                mt.image.anchoredPosition = startPosByOption[index][t] + mt.offset;
            }
        }

        currentIndex = index;

        // 3) 리모컨 아래로 이동
        MoveRemoteDown();
    }

    private void MoveRemoteDown()
    {
        remotePanel.anchoredPosition = remoteStartPos + remoteDownOffset;
        remoteIsDown = true;
    }

    public void OpenRemote()
    {
        // 리모컨 원위치
        remotePanel.anchoredPosition = remoteStartPos;
        remoteIsDown = false;

        // (선택) 리모컨 다시 열 때 현재 옵션 이미지들 원복
        if (resetImageWhenOpenRemote)
            ResetCurrentOptionTargets();
    }

    private void ResetCurrentOptionTargets()
    {
        if (currentIndex < 0) return;

        var opt = options[currentIndex];
        if (opt.targets != null)
        {
            for (int t = 0; t < opt.targets.Length; t++)
            {
                var mt = opt.targets[t];
                if (mt == null || mt.image == null) continue;

                mt.image.anchoredPosition = startPosByOption[currentIndex][t];
            }
        }

        currentIndex = -1;
    }

    public void ResetAll()
    {
        // 리모컨 원복
        remotePanel.anchoredPosition = remoteStartPos;
        remoteIsDown = false;

        // 모든 옵션의 모든 이미지 원복
        for (int i = 0; i < options.Length; i++)
        {
            var opt = options[i];
            if (opt.targets == null) continue;

            for (int t = 0; t < opt.targets.Length; t++)
            {
                var mt = opt.targets[t];
                if (mt == null || mt.image == null) continue;

                mt.image.anchoredPosition = startPosByOption[i][t];
            }
        }

        currentIndex = -1;
    }
}