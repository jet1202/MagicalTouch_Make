using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class TitleBarSetter : MonoBehaviour 
{
    static TitleBarSetter instance = null;

    public static TitleBarSetter Instance { get { return instance;} }

//Windowsのみに限定
#if UNITY_STANDALONE_WIN
    [DllImport("user32.dll", EntryPoint = "SetWindowText")]
    public static extern bool SetWindowText(System.IntPtr hwnd, System.String lpString);
    [DllImport("user32.dll", EntryPoint = "FindWindow")]
    public static extern System.IntPtr FindWindow(System.String className, System.String windowName);

    System.IntPtr hWnd;
    private void Start()
    {
        instance = this;
        DontDestroyOnLoad(this.gameObject);

        //Product NameのWindowを探す
        hWnd = FindWindow(null, Application.productName);
    }
#endif

    public void SetTitleBar(string text)
    {
#if UNITY_STANDALONE_WIN
        SetWindowText(hWnd, text);
#endif
    }
}