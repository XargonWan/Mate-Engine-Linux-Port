using System;
using System.Collections.Generic;
using UnityEngine;
using X11;
using Random = UnityEngine.Random;

public class AvatarWindowHandler : MonoBehaviour
{
    public float desktopScale = 1f;
    public int snapThreshold = 30;
    [Header("Window Sit BlendTree")]
    public int totalWindowSitAnimations = 4;
    private static readonly int WindowSitIndexParam = Animator.StringToHash("WindowSitIndex");
    private static readonly int IsWindowSit = Animator.StringToHash("isWindowSit");
    private static readonly int IsSitting = Animator.StringToHash("isSitting");
    private static readonly int IsBigScreenAlarm = Animator.StringToHash("isBigScreenAlarm");
    private bool wasSitting;

    [Header("User Y-Offset Slider")]
    [Range(-0.015f, 0.015f)]
    public float windowSitYOffset;
    
    IntPtr _snappedHwnd = IntPtr.Zero;
    IntPtr _unityHwnd = IntPtr.Zero;
    Vector2 lastDesktopPosition;
    readonly List<WindowEntry> cachedWindows = new();

    Animator animator;
    AvatarAnimatorController controller;

    private float lastCacheUpdateTime;
    private const float CacheUpdateCooldown = 0.05f;
    
    private float horizontalOffset;

    void Start()
    {
        _unityHwnd = X11Manager.Instance.UnityWindow;
        animator = GetComponent<Animator>();
        controller = GetComponent<AvatarAnimatorController>();
        lastDesktopPosition = X11Manager.Instance.GetWindowPosition();
    }

    void Update()
    {
        if (_unityHwnd == IntPtr.Zero || !animator || !controller) return;
        if (!SaveLoadHandler.Instance.data.enableWindowSitting) return;

        bool isSittingNow = animator.GetBool(IsWindowSit);
        if (isSittingNow && !wasSitting)
        {
            int sitIdx = Random.Range(0, totalWindowSitAnimations);
            animator.SetFloat(WindowSitIndexParam, sitIdx);
        }
        wasSitting = isSittingNow;

        Vector2 unityPos = GetUnityWindowPosition();

        if (controller.isDragging && !animator.GetBool(IsSitting))
        {
            if (_snappedHwnd == IntPtr.Zero)
                TrySnap(unityPos);
            else if (!IsStillNearSnappedWindow(unityPos))
            {
                ExitWindowSitting();
            }
        }
        else if (!controller.isDragging && _snappedHwnd != IntPtr.Zero)
        {
            FollowSnappedWindow();
        }

        if (_snappedHwnd != IntPtr.Zero)
        {
            if (X11Manager.Instance.IsWindowMaximized(_snappedHwnd) || IsWindowFullscreen(_snappedHwnd))
            {
                ExitWindowSitting();
                MoveMateToDesktopPosition();
            }
        }

        if (animator.GetBool(IsBigScreenAlarm))
        {
            ExitWindowSitting();
        }
    }

    void TrySnap(Vector2 unityPos)
    {
        if (Time.time < lastCacheUpdateTime + CacheUpdateCooldown) return;
        UpdateCachedWindows();

        X11Manager.Instance.GetWindowRect(out Rect unityRect);

        foreach (var entry in cachedWindows)
        {
            if (entry.Hwnd == _unityHwnd) continue;
            
            X11Manager.Instance.GetWindowRect(entry.Hwnd, out Rect winRect);
            Rect topBar = new Rect(winRect.x, winRect.y, winRect.width, 5 * desktopScale);
            Rect snapRect = new Rect(unityRect.x, unityRect.y + unityRect.height, unityRect.width, snapThreshold * desktopScale);
            if (!snapRect.Overlaps(topBar)) continue;
            _snappedHwnd = entry.Hwnd;
            animator.SetBool(IsWindowSit, true);
            lastDesktopPosition = unityPos;
            horizontalOffset = unityPos.x - winRect.x;
        }
    }

    void FollowSnappedWindow()
    {
        if (!X11Manager.Instance.GetWindowRect(_snappedHwnd, out Rect winRect) || 
            !X11Manager.Instance.IsWindowVisible(_snappedHwnd))
        {
            ExitWindowSitting();
            return;
        }

        Vector2 unitySize = X11Manager.Instance.GetWindowSize();
        float targetY = winRect.y - unitySize.y + windowSitYOffset * unitySize.y;
        float targetX = winRect.x + horizontalOffset;

        X11Manager.Instance.SetWindowPosition(targetX, targetY);
    }

    bool IsStillNearSnappedWindow(Vector2 unityPos)
    {
        if (_snappedHwnd == IntPtr.Zero) return false;
        if (!X11Manager.Instance.GetWindowRect(_snappedHwnd, out Rect winRect)) return false;

        Vector2 size = X11Manager.Instance.GetWindowSize();
        float currentBottom = unityPos.y + size.y;
        float targetBottom = winRect.y;

        return Mathf.Abs(currentBottom - targetBottom) < snapThreshold;
    }

    void UpdateCachedWindows()
    {
        cachedWindows.Clear();
        var allWindows = X11Manager.Instance.GetAllVisibleWindows();
        foreach (var hWnd in allWindows)
        {
            if (!X11Manager.Instance.GetWindowRect(hWnd, out Rect r)) continue;
            string cls = X11Manager.Instance.GetClassName(hWnd);
            bool isTaskbar = X11Manager.Instance.IsDock(hWnd);

            if (!isTaskbar)
            {
                if (r.width < 100 || r.height < 100) continue;
                if (cls.Length == 0) continue;
                if (X11Manager.Instance.IsDesktop(hWnd)) continue;
            }

            cachedWindows.Add(new WindowEntry { Hwnd = hWnd, Rect = r });
        }
        lastCacheUpdateTime = Time.time;
    }

    void ExitWindowSitting()
    {
        _snappedHwnd = IntPtr.Zero;
        animator.SetBool(IsWindowSit, false);
        animator.SetBool(IsSitting, false);
    }

    struct WindowEntry { public IntPtr Hwnd; public Rect Rect; }

    Vector2 GetUnityWindowPosition() => X11Manager.Instance.GetWindowPosition();

    bool IsWindowFullscreen(IntPtr hwnd)
    {
        if (!X11Manager.Instance.GetWindowRect(hwnd, out Rect rect)) return false;
        int screenWidth = Display.main.systemWidth;
        int screenHeight = Display.main.systemHeight;
        int tolerance = 2;
        return Mathf.Abs(rect.width - screenWidth) <= tolerance && 
               Mathf.Abs(rect.height - screenHeight) <= tolerance;
    }

    void MoveMateToDesktopPosition()
    {
        X11Manager.Instance.SetWindowPosition(lastDesktopPosition.x, lastDesktopPosition.y);
    }

    public void ForceExitWindowSitting()
    {
        ExitWindowSitting();
    }
}