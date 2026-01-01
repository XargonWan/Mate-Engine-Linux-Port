using System;
using UnityEngine;

public class RemoveTaskbarApp : MonoBehaviour
{

    private IntPtr _unityHwnd = IntPtr.Zero;

    private bool _isHidden = true;
    public bool IsHidden => _isHidden;

    void Start()
    {
#if !UNITY_EDITOR
        _unityHwnd = WindowManager.Instance.UnityWindow;
        if (_unityHwnd != IntPtr.Zero)
        {
            WindowManager.Instance.HideFromTaskbar();
            _isHidden = true;
        }
#endif
    }

    public void ToggleAppMode()
    {
#if !UNITY_EDITOR
        if (_unityHwnd == IntPtr.Zero)
            return;

            _isHidden = !_isHidden;
            WindowManager.Instance.HideFromTaskbar(_isHidden);
#endif
    }
}
