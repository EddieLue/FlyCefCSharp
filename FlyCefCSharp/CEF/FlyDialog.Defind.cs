using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace Fly
{
    partial class FlyDialog
    {
        public enum CefBorderStyle
        {
            bsNone,
            bsSingle,
            bsSizeable,
            bsDialog,
            bsToolWindow,
            bsSizeToolWin
        }
        private enum ShowType
        {
            Modal,
            Normal
        }

        public enum MsgType
        {
            /// <summary>
            /// 詢問：Yes,No
            /// </summary>
            Question = System.Windows.Forms.MessageBoxIcon.Question,
            /// <summary>
            /// 詢問：OK,Cancel
            /// </summary>
            Confirm = System.Windows.Forms.MessageBoxIcon.Question | 1024,
            /// <summary>
            /// 提示
            /// </summary>
            Information = System.Windows.Forms.MessageBoxIcon.Information,
            /// <summary>
            /// 警告
            /// </summary>
            Warning = System.Windows.Forms.MessageBoxIcon.Warning,
            /// <summary>
            /// 錯誤
            /// </summary>
            Error = System.Windows.Forms.MessageBoxIcon.Error

        }


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct CreateInfo
        {
            public int width;
            public int height;
            public int min_width;
            public int min_height;
            public int max_width;
            public int max_height;
            public CefBorderStyle border_style;
            public IntPtr hicon;
        }

        private struct MouseDragRect
        {
            public Rectangle Rect;
            public bool Enabled;
        }

        public delegate string RecvMessage(IntPtr hWnd, int msg, int param_int, string param_str, string data);
        public delegate void LoadEndCallBack(FlyDialog Sender);
        public delegate void LoseFocusCallBack(FlyDialog Sender);
        public delegate void JsonCallBack(string p_Json);

        public delegate string EventCallBack(string p_Json);
        public delegate string EventCallBackReturn();
        public delegate void EventCallBackJson(string p_Json);
        public delegate void EventCallBackEmpty();

        public delegate void OnMessageCallBack(ref Message op_Msg);

    }
}

