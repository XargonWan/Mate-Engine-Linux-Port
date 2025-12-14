using System;
using System.Collections.Generic;
using UnityEngine;
using X11;
using Random = UnityEngine.Random;
public class AvatarWindowHandler : MonoBehaviour
{
    public int verticalOffset;
    public float desktopScale = 1f;
    [Header("Pink Snap Zone (Unity-side)")]
    public Vector2 snapZoneOffset = new(0, -5);
    public Vector2 snapZoneSize = new(100, 10);
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

    [Header("Fine-Tune")]
    float snapFraction;
    public float baseOffset = 40f;
    public float baseScale = 1f;

    IntPtr _snappedHwnd = IntPtr.Zero, _unityHwnd = IntPtr.Zero;
    Vector2 lastDesktopPosition;
    readonly List<WindowEntry> cachedWindows = new();
    Rect pinkZoneDesktopRect;

    Animator animator;
    AvatarAnimatorController controller;

    private float lastCacheUpdateTime;
    private const float CacheUpdateCooldown = 0.05f; // Optional cooldown during dragging

    void Start()
    {
        _unityHwnd = X11Manager.Instance.UnityWindow;
        animator = GetComponent<Animator>();
        controller = GetComponent<AvatarAnimatorController>();
    }
    void Update()
    {
        if (_unityHwnd == IntPtr.Zero || !animator || !controller) return;
        if (!SaveLoadHandler.Instance.data.enableWindowSitting) return;

        bool isSittingNow = animator && animator.GetBool(IsWindowSit);
        if (isSittingNow && !wasSitting)
        {
            int sitIdx = Random.Range(0, totalWindowSitAnimations);
            animator.SetFloat(WindowSitIndexParam, sitIdx);
        }
        wasSitting = isSittingNow;

        var unityPos = GetUnityWindowPosition();
        UpdatePinkZone(unityPos);

        if (controller.isDragging && !controller.animator.GetBool(IsSitting))
        {
            if (_snappedHwnd == IntPtr.Zero)
                TrySnap(unityPos);
            else if (!IsStillNearSnappedWindow())
            {
                _snappedHwnd = IntPtr.Zero;
                animator.SetBool(IsWindowSit, false);
            }
            else
                FollowSnappedWindowWhileDragging();
        }
        else if (!controller.isDragging && _snappedHwnd != IntPtr.Zero)
            FollowSnappedWindow();

        if (_snappedHwnd != IntPtr.Zero)
        {
            if (X11Manager.Instance.IsWindowMaximized(_snappedHwnd) || IsWindowFullscreen(_snappedHwnd))
            {
                MoveMateToDesktopPosition();

                _snappedHwnd = IntPtr.Zero;
                if (animator)
                {
                    animator.SetBool(IsWindowSit, false);
                    animator.SetBool(IsSitting, false);
                }
            }
        }

        if (!animator || !animator.GetBool(IsBigScreenAlarm)) return;
        if (animator.GetBool(IsWindowSit))
        {
            animator.SetBool(IsWindowSit, false);
        }
        _snappedHwnd = IntPtr.Zero;
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

    void UpdatePinkZone(Vector2 unityPos)
    {
        float cx = unityPos.x + GetUnityWindowWidth() * 0.5f + snapZoneOffset.x;
        float by = unityPos.y + GetUnityWindowHeight() + snapZoneOffset.y;
        pinkZoneDesktopRect = new Rect(cx - snapZoneSize.x * 0.5f, by, snapZoneSize.x, snapZoneSize.y);
    }

    void TrySnap(Vector2 unityWindowPosition)
    {
        if (Time.time < lastCacheUpdateTime + CacheUpdateCooldown) return;
        UpdateCachedWindows();

        foreach (var win in cachedWindows)
        {
            if (win.Hwnd == _unityHwnd) continue;
            var topBar = new Rect(win.Rect.x, win.Rect.y, win.Rect.width, 5);
            if (!pinkZoneDesktopRect.Overlaps(topBar)) continue;
            lastDesktopPosition = GetUnityWindowPosition();
            _snappedHwnd = win.Hwnd;
            float winWidth = win.Rect.width, unityWidth = GetUnityWindowWidth();
            float petCenterX = unityWindowPosition.x + unityWidth * 0.5f;
            snapFraction = (petCenterX - win.Rect.x) / winWidth;
            animator.SetBool(IsWindowSit, true);
            return;
        }
    }
    void FollowSnappedWindowWhileDragging()
    {
        if (!X11Manager.Instance.GetWindowRect(_snappedHwnd, out Rect winRect) || !X11Manager.Instance.IsWindowVisible(_snappedHwnd))
        {
            _snappedHwnd = IntPtr.Zero;
            animator.SetBool(IsWindowSit, false);
            return;
        }

        var unityPos = GetUnityWindowPosition();
        float winWidth = winRect.width, unityWidth = GetUnityWindowWidth();
        float petCenterX = unityPos.x + unityWidth * 0.5f;
        snapFraction = (petCenterX - winRect.x) / winWidth;
        float newCenterX = winRect.x + snapFraction * winWidth;
        int targetX = Mathf.RoundToInt(newCenterX - unityWidth * 0.5f);
        float yOffset = GetUnityWindowHeight() + snapZoneOffset.y + snapZoneSize.y * 0.5f;
        float scale = transform.localScale.y, scaleOffset = (baseScale - scale) * baseOffset;
        float windowSitOffset = windowSitYOffset * GetUnityWindowHeight();
        float targetY = winRect.y - (int)(yOffset + scaleOffset) + verticalOffset + Mathf.RoundToInt(windowSitOffset);
        SetUnityWindowPosition(targetX, targetY);
    }

    void FollowSnappedWindow()
    {
        if (!X11Manager.Instance.GetWindowRect(_snappedHwnd, out Rect winRect) || !X11Manager.Instance.IsWindowVisible(_snappedHwnd))
        {
            _snappedHwnd = IntPtr.Zero;
            animator.SetBool(IsWindowSit, false);
            return;
        }

        float winWidth = winRect.width, unityWidth = GetUnityWindowWidth();
        float newCenterX = winRect.x + snapFraction * winWidth;
        int targetX = Mathf.RoundToInt(newCenterX - unityWidth * 0.5f);
        float yOffset = GetUnityWindowHeight() + snapZoneOffset.y + snapZoneSize.y * 0.5f;
        float scale = transform.localScale.y, scaleOffset = (baseScale - scale) * baseOffset;
        float windowSitOffset = windowSitYOffset * GetUnityWindowHeight();
        float targetY = winRect.y - (int)(yOffset + scaleOffset) + verticalOffset + Mathf.RoundToInt(windowSitOffset);
        SetUnityWindowPosition(targetX, targetY);
    }

    bool IsStillNearSnappedWindow()
    {
        if (!X11Manager.Instance.GetWindowRect(_snappedHwnd, out Rect winRect) || !X11Manager.Instance.IsWindowVisible(_snappedHwnd))
        {
            return false;
        }
        return pinkZoneDesktopRect.Overlaps(new Rect(winRect.x, winRect.y, winRect.width, 5));
    }

    struct WindowEntry { public IntPtr Hwnd; public Rect Rect; }
    
    Vector2 GetUnityWindowPosition() { Vector2 r = X11Manager.Instance.GetWindowPosition(); return new(r.x, r.y); }
    int GetUnityWindowWidth() { Vector2 r = X11Manager.Instance.GetWindowSize(); return (int)r.x; }
    int GetUnityWindowHeight() { Vector2 r = X11Manager.Instance.GetWindowSize(); return (int)r.y; }
    void SetUnityWindowPosition(float x, float y)
    {
        if (!controller.isDragging) X11Manager.Instance.SetWindowPosition(x, y);
    }

    bool IsWindowFullscreen(IntPtr hwnd)
    {
        if (!X11Manager.Instance.GetWindowRect(hwnd, out Rect rect)) return false;

        float width = rect.width;
        float height = rect.height;
        int screenWidth = Display.main.systemWidth;
        int screenHeight = Display.main.systemHeight;
        int tolerance = 2; 
        return Mathf.Abs(width - screenWidth) <= tolerance && Mathf.Abs(height - screenHeight) <= tolerance;
    }
    void MoveMateToDesktopPosition()
    {
        SetUnityWindowPosition(lastDesktopPosition.x, lastDesktopPosition.y);
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        float basePixel = 1000f / desktopScale;
        Gizmos.color = Color.magenta; DrawDesktopRect(pinkZoneDesktopRect, basePixel);
        X11Manager.Instance.GetWindowRect(_unityHwnd, out Rect uRect);
        Gizmos.color = Color.green; DrawDesktopRect(new Rect(uRect.x, uRect.height - 5, uRect.width, 5), basePixel);
        foreach (var win in cachedWindows)
        {
            if (win.Hwnd == _unityHwnd) continue;
            float w = win.Rect.width, h = win.Rect.height;
            Gizmos.color = Color.red; DrawDesktopRect(new Rect(win.Rect.x, win.Rect.y, w, 5), basePixel);
            Gizmos.color = Color.yellow; DrawDesktopRect(new Rect(win.Rect.x, win.Rect.y, w, h), basePixel);
        }
    }

    void DrawDesktopRect(Rect r, float basePixel)
    {
        float cx = r.x + r.width * 0.5f, cy = r.y + r.height * 0.5f;
        int screenWidth = Display.main.systemWidth, screenHeight = Display.main.systemHeight;
        float unityX = (cx - screenWidth * 0.5f) / basePixel, unityY = -(cy - screenHeight * 0.5f) / basePixel;
        Vector3 worldPos = new(unityX, unityY, 0), worldSize = new(r.width / basePixel, r.height / basePixel, 0);
        Gizmos.DrawWireCube(worldPos, worldSize);
    }

    public void ForceExitWindowSitting()
    {
        _snappedHwnd = IntPtr.Zero;
        if (!animator) return;
        animator.SetBool(IsWindowSit, false);
        animator.SetBool(IsSitting, false);
    }
}