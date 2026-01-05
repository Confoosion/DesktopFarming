using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class WindowController : MonoBehaviour
{
    [Header("Window Settings")]
    public int defaultWidth = 600;
    public int defaultHeight = 600;
    public int minWidth = 400;
    public int minHeight = 400;
    public int maxWidth = 800;
    public int maxHeight = 800;
    
    [Header("Window Behavior")]
    public bool alwaysOnTop = true;
    public bool removeBorder = false;
    
    [Header("Drag Settings")]
    public bool enableDragging = true;
    public KeyCode dragKey = KeyCode.Mouse0; // Left click
    
    private bool isDragging = false;
    private Vector2 dragOffset;
    
    #if UNITY_STANDALONE_WIN && !UNITY_EDITOR
    // Windows API functions
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();
    
    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
    
    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);
    
    [DllImport("user32.dll")]
    private static extern uint GetWindowLong(IntPtr hWnd, int nIndex);
    
    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);
    
    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
    
    [DllImport("user32.dll")]
    private static extern bool ReleaseCapture();
    
    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
    
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
    
    // Constants
    private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
    private const uint SWP_SHOWWINDOW = 0x0040;
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOSIZE = 0x0001;
    private const int GWL_STYLE = -16;
    private const uint WS_BORDER = 0x00800000;
    private const uint WS_CAPTION = 0x00C00000;
    private const uint WS_SYSMENU = 0x00080000;
    private const uint WS_THICKFRAME = 0x00040000;
    private const uint WM_NCLBUTTONDOWN = 0xA1;
    private const uint HT_CAPTION = 0x2;
    
    private IntPtr windowHandle;
    #endif
    
    private void Start()
    {
        #if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        SetupWindow();
        #else
        Debug.Log("Window controller only works in Windows builds, not in the editor.");
        #endif
    }
    
    #if UNITY_STANDALONE_WIN && !UNITY_EDITOR
    private void SetupWindow()
    {
        windowHandle = GetActiveWindow();
        
        // Set initial window size
        Screen.SetResolution(defaultWidth, defaultHeight, false);
        
        // Configure window style
        uint style = GetWindowLong(windowHandle, GWL_STYLE);
        
        if (removeBorder)
        {
            // Remove caption and system menu but keep resize frame
            style &= ~(WS_CAPTION | WS_SYSMENU);
        }
        else
        {
            // Keep standard window with resize capability
            style |= WS_THICKFRAME;
        }
        
        SetWindowLong(windowHandle, GWL_STYLE, style);
        
        // Set always on top
        if (alwaysOnTop)
        {
            SetWindowPos(windowHandle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
        }
        
        // Apply the style changes
        SetWindowPos(windowHandle, IntPtr.Zero, 0, 0, 0, 0, 
            SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
    }
    
    private void Update()
    {
        EnforceWindowSizeLimits();
        HandleDragging();
        
        // Toggle always on top with 'T' key
        if (Input.GetKeyDown(KeyCode.T))
        {
            ToggleAlwaysOnTop();
        }
    }
    
    private void EnforceWindowSizeLimits()
    {
        int currentWidth = Screen.width;
        int currentHeight = Screen.height;
        bool needsResize = false;
        
        // Check if window is outside size limits
        if (currentWidth < minWidth)
        {
            currentWidth = minWidth;
            needsResize = true;
        }
        else if (currentWidth > maxWidth)
        {
            currentWidth = maxWidth;
            needsResize = true;
        }
        
        if (currentHeight < minHeight)
        {
            currentHeight = minHeight;
            needsResize = true;
        }
        else if (currentHeight > maxHeight)
        {
            currentHeight = maxHeight;
            needsResize = true;
        }
        
        if (needsResize)
        {
            Screen.SetResolution(currentWidth, currentHeight, false);
        }
    }
    
    private void HandleDragging()
    {
        if (!enableDragging) return;
        
        // Start dragging when left click is pressed
        if (Input.GetKeyDown(dragKey))
        {
            // Check if clicking in a "safe" area (not on UI or game objects)
            // You can customize this logic based on your needs
            isDragging = true;
            
            POINT cursorPos;
            GetCursorPos(out cursorPos);
            
            RECT windowRect;
            GetWindowRect(windowHandle, out windowRect);
            
            dragOffset = new Vector2(
                cursorPos.X - windowRect.Left,
                cursorPos.Y - windowRect.Top
            );
        }
        
        if (Input.GetKey(dragKey) && isDragging)
        {
            POINT cursorPos;
            GetCursorPos(out cursorPos);
            
            int newX = cursorPos.X - (int)dragOffset.x;
            int newY = cursorPos.Y - (int)dragOffset.y;
            
            SetWindowPos(windowHandle, IntPtr.Zero, newX, newY, 0, 0, 
                SWP_NOSIZE | SWP_SHOWWINDOW);
        }
        
        if (Input.GetKeyUp(dragKey))
        {
            isDragging = false;
        }
    }
    
    public void ToggleAlwaysOnTop()
    {
        alwaysOnTop = !alwaysOnTop;
        
        if (alwaysOnTop)
        {
            SetWindowPos(windowHandle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
        }
        else
        {
            SetWindowPos(windowHandle, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
        }
        
        Debug.Log("Always On Top: " + alwaysOnTop);
    }
    
    // Alternative drag method using Windows native dragging
    public void StartNativeDrag()
    {
        ReleaseCapture();
        SendMessage(windowHandle, WM_NCLBUTTONDOWN, (IntPtr)HT_CAPTION, IntPtr.Zero);
    }
    #endif
}