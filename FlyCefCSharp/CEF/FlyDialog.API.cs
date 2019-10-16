using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Fly
{
    partial class FlyDialog
    {
        [DllImport("FlyCef.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr CreateChromium(string p_Title, int p_Parent, string p_Content, ref CreateInfo p_CreateInfo, RecvMessage p_RecvMsg);
        [DllImport("FlyCef.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        private static extern int ShowChromium(IntPtr hWnd, bool blIsModal);
        [DllImport("FlyCef.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        private static extern void ExecuteWebJS(IntPtr p_HWnd, string p_Code);
        [DllImport("FlyCef.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        private static extern void CloseChromium(IntPtr p_HWnd);

        [DllImport("libcef.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        private static extern void cef_shutdown();


        [DllImport("user32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        private static extern bool IsWindow(IntPtr hWnd);
        [DllImport("user32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint uMsg, int wParam, uint lParam);
        [DllImport("user32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        private static extern bool SetWindowText(IntPtr hWnd, string lpString);
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hwnd, int nIndex, uint nLong);
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hwnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowPos(IntPtr hwnd, int hWndInsertAfter, int x, int y, int cx, int cy, int wFlag);
        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hwnd, bool bRevert);
        [DllImport("user32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        private static extern int ShowWindow(IntPtr hWnd, int nShowCmd);


        private const int WM_KILLFOCUS = 8;
        private const int WM_SYSCOMMAND = 0x0112;
        private const int WM_NCLBUTTONDOWN = 161;
        private const int WM_NCLBUTTONUP = 162;
        private const int WM_LBUTTONDOWN = 0x201;
        private const int WM_LBUTTONUP = 0x202;
        private const int WM_DESTROY = 16;
        private const int SC_MOVE = 0xF010;
        private const int HTCAPTION = 0x0002;

        private const int SW_HIDE = 0;
        private const int SW_NORMAL = 1;
        private const int SW_MAXIMIZE = 3;
        private const int SW_SHOW = 5;
        private const int SW_MINIMIZE = 6;



    }
}
