using System;
using System.Runtime.InteropServices;
using System.Collections;
using UnityEngine;

public class SystemdJournalReader : MonoBehaviour
{
    const string LibSystemd = "libsystemd.so.0";

    [DllImport(LibSystemd)]
    private static extern int sd_journal_open(out IntPtr j, int flags);

    [DllImport(LibSystemd)]
    private static extern void sd_journal_close(IntPtr j);

    [DllImport(LibSystemd)]
    private static extern int sd_journal_next(IntPtr j);

    [DllImport(LibSystemd)]
    private static extern int sd_journal_get_data(IntPtr j, string field, out IntPtr data, out nuint length);

    [DllImport(LibSystemd)]
    private static extern int sd_journal_add_match(IntPtr j, string match, nuint length);

    private bool closing;

    private void Start()
    {
        StartCoroutine(ExampleReadRecentMessages());
    }

    public IEnumerator ExampleReadRecentMessages()
    {
        string unit = "kwin_x11";
        int ret = sd_journal_open(out var journal, 0); // Flags: 0 for default
        if (ret < 0) throw new Exception("Failed to open journal");

        string match = $"_SYSTEMD_UNIT={unit}.service";
        ret = sd_journal_add_match(journal, match, (nuint)match.Length);
        if (ret < 0)
        {
            Debug.LogError($"Failed to add match: {ret}");
            yield break;
        }

        while (sd_journal_next(journal) > 0 && !closing)
        {
            // Get MESSAGE field
            ret = sd_journal_get_data(journal, "MESSAGE", out IntPtr dataPtr, out nuint len);
            if (ret >= 0)
            {
                string message = Marshal.PtrToStringUTF8(dataPtr + "MESSAGE=".Length, (int)len - "MESSAGE=".Length);
                Debug.Log(message);
            }

            yield return null;
        }

        sd_journal_close(journal);
    }

    private void OnApplicationQuit()
    {
        closing = true;
    }
}