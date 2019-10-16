using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;


namespace Fly
{
    /// <summary>
    /// FlyDialog Cef窗口類 
    /// </summary>
    partial class FlyDialog : NativeWindow, IWin32Window
    {
        /// <summary>
        /// 页面载入完成
        /// </summary>
        public event LoadEndCallBack LoadEnd = null;
        /// <summary>
        /// 接收JS消息
        /// </summary>
        public event RecvMessage RecvData = null;
        /// <summary>
        /// 失去焦点
        /// </summary>
        public event LoseFocusCallBack LoseFocus = null;

        private Dictionary<string, JsonCallBack> m_dicJsonEvents = new Dictionary<string, JsonCallBack>();
        private Dictionary<string, EventCallBack> m_dicJsEventEvents = new Dictionary<string, EventCallBack>();
        private List<MouseDragRect> m_MouseDragRects = new List<MouseDragRect>();
        private Dictionary<int, OnMessageCallBack> m_MessageCallBacks = new Dictionary<int, OnMessageCallBack>();

        private IntPtr m_Handle;
        /// <summary>
        /// 窗口句柄
        /// </summary>
        public new IntPtr Handle
        {
            get { return m_Handle; }
        }
        private string m_Content;
        private CreateInfo m_Cinfo;
        /// <summary>
        /// 创建窗口前预先赋予的参数
        /// </summary>
        public CreateInfo CreateConst { get { return m_Cinfo; } set { m_Cinfo = value; } }

        private bool m_Hiding = false;
        public bool Visible { get { return m_Hiding; } set { ShowWindow(m_Handle, value ? SW_SHOW : SW_HIDE); m_Hiding = value; } }

        private ShowType m_ShowType = ShowType.Normal;
        private ChromeHost m_ChromeHost;


        private string m_Text = "";
        public string Text 
        {
            get
            {
                return this.m_Text;
            }
            set
            {
                if (IsWindow(this.Handle))
                {
                    SetWindowText(this.Handle, value);
                }
                this.m_Text = value;
            }
        }

        /// <summary>
        /// 實例化對象
        /// </summary>
        /// <param name="p_Parent"></param>
        public FlyDialog(FlyDialog p_Parent = null)
        {
            try
            {
                this.m_Text = "";
                this.m_Content = "";
                this.m_Cinfo = new CreateInfo
                {
                    width = 600,
                    height = 400,
                    min_width = 200,
                    min_height = 100,
                    hicon = IntPtr.Zero,
                    border_style = CefBorderStyle.bsSizeable
                };
                this.m_MouseDragRects = new List<MouseDragRect>();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 创建窗口
        /// </summary>
        /// <param name="p_Parent"></param>
        /// <param name="p_Content"></param>
        public void Create(IWin32Window p_Parent, string p_Content) 
        {
            try
            {
                IntPtr h_Parent = IntPtr.Zero;
                if (p_Parent != null)
                {
                    h_Parent = p_Parent.Handle;
                }
                this.m_Content = p_Content;
                IntPtr h_Wnd = CreateChromium(this.m_Text, h_Parent.ToInt32(), p_Content, ref m_Cinfo, JsRecvData);
                this.m_Handle = h_Wnd;
                this.AssignHandle(m_Handle);
                
                m_ChromeHost = new ChromeHost(this);

                OnWindowMessage(WM_KILLFOCUS, OnKillFocusCallBack);
                OnWindowMessage(WM_SYSCOMMAND, OnKillFocusCallBack);
                OnWindowMessage(WM_NCLBUTTONDOWN, OnNCLButtonDownCallBack);
                OnWindowMessage(WM_LBUTTONDOWN, OnLButtonDownCallBack);
                OnWindowMessage(WM_LBUTTONUP, OnLButtonUpCallBack);
                OnWindowMessage(WM_DESTROY, OnDestroy);
                
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void OnKillFocusCallBack(ref Message m)
        {
            if (LoseFocus != null)
            {
                LoseFocus.Invoke(this);
            }
        }
        void OnNCLButtonDownCallBack(ref Message m)
        {
            if (m.WParam.ToInt32() == HTCAPTION)
            {
                if (LoseFocus != null)
                {
                    LoseFocus.Invoke(this);
                }
            }
        }
        void OnLButtonDownCallBack(ref Message m)
        {
            int X = m.LParam.ToInt32() & 0xffff;
            int Y = m.LParam.ToInt32() >> 16;

            foreach (MouseDragRect item in m_MouseDragRects)
            {
                if (X >= item.Rect.Left && Y >= item.Rect.Top &&
                    X <= item.Rect.Right && Y <= item.Rect.Bottom)
                {
                    if (item.Enabled)
                    {
                        ReleaseCapture();
                        SendMessage(m.HWnd, WM_SYSCOMMAND, SC_MOVE | HTCAPTION, 0);
                    }
                }
            }
        }
        void OnLButtonUpCallBack(ref Message m)
        {
            ReleaseCapture();
        }
        void OnDestroy(ref Message m)
        {
            //正常模式顯示的話，窗體銷毀前去銷毀Delphi窗体對象
            if (m.HWnd == m_Handle && m_ShowType == ShowType.Normal)
            {
                this.Close();
            }
        }
        

        /// <summary>
        /// 显示为模态窗体(如果為主窗口，在窗體關閉之後請及時調用Exit()，如果為子窗口請及時調用Close())
        /// </summary>
        /// <returns></returns>
        public DialogResult ShowModal()
        {
            try
            {
                this.m_ShowType = ShowType.Modal;
                int n_Result = ShowChromium(m_Handle, true);
                return (DialogResult)n_Result;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return DialogResult.None;
        }

        /// <summary>
        /// 显示窗体
        /// </summary>
        public void Show()
        {
            try
            {
                this.m_ShowType = ShowType.Normal;
                ShowChromium(m_Handle, false);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 关闭当前窗体，并釋放資源
        /// </summary>
        public void Close()
        {
            try
            {
                CloseChromium(m_Handle);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 退出當前程序
        /// </summary>
        public void Exit()
        {
            try
            {
                cef_shutdown();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            //System.Diagnostics.Process.GetCurrentProcess().Kill();
        }


        /// <summary>
        /// 执行JS
        /// </summary>
        /// <param name="p_Code"></param>
        public void Js(string p_Code)
        {
            try
            {
                string s_code = p_Code;
                ExecuteWebJS(m_Handle, s_code);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 设置事件，需接收Json，需返回執行JS
        /// </summary>
        /// <param name="p_Selector">jQuery選擇器</param>
        /// <param name="p_Event">事件</param>
        /// <param name="p_CallBack">囘調函數，EventCallBack的參數1：通過執行p_JsonCode得到返回的Json，返回值：返回囘調之後執行的Js代碼</param>
        /// <param name="p_ReturnJson">返回給囘調函數的參數2</param>
        public void On(string p_Selector, string p_Event, string p_ReturnJson, EventCallBack p_CallBack)
        {
            try
            {
                string s_Guid = Guid.NewGuid().ToString();
                string s_Js = "";
                if (p_ReturnJson.Replace(" ", "").Trim() != "")
                {
                    if (p_ReturnJson.Substring(p_ReturnJson.Length - 1) != ";")
                    {
                        p_ReturnJson += "; ";
                    }
                    p_ReturnJson = "return " + p_ReturnJson;
                }

                s_Js = @"
                        (function () {
                            $('" + p_Selector + @"').on('" + p_Event + @"', function (ev) {
                                this.returnjson = function () { " + p_ReturnJson + @" };
                                var o_json = this.returnjson();
                                var s_json = JSON.stringify(o_json);
                                var s_Js = Cef(2, 1, 'event_" + s_Guid + @"', s_json);
                                if (s_Js != '') {
                                    eval(s_Js);
                                }
                            });
                        })();
                        ";
                m_dicJsEventEvents.Add("event_" + s_Guid, p_CallBack);
                this.Js(s_Js);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 设置事件，無接收Json，需返回JS執行
        /// </summary>
        /// <param name="p_Selector">jQuery選擇器</param>
        /// <param name="p_Event">事件</param>
        /// <param name="p_CallBack">囘調函數</param>
        /// <param name="p_ReturnJson">返回給囘調函數的參數2</param>
        public void On(string p_Selector, string p_Event, EventCallBackReturn p_CallBack) 
        {
            On(p_Selector, p_Event, "{ result: 'OK' }", (string s_Json) =>
            {
                return p_CallBack.Invoke();
            });
        }

        /// <summary>
        /// 设置事件，需接收Json，無返回JS執行
        /// </summary>
        /// <param name="p_Selector">jQuery選擇器</param>
        /// <param name="p_Event">事件</param>
        /// <param name="p_CallBack">囘調函數</param>
        /// <param name="p_ReturnJson">返回給囘調函數的參數2</param>
        public void On(string p_Selector, string p_Event, string p_ReturnJson, EventCallBackJson p_CallBack)
        {
            On(p_Selector, p_Event, p_ReturnJson, (string s_Json) =>
            {
                p_CallBack.Invoke(s_Json);
                return "";
            });
        }

        /// <summary>
        /// 设置事件，無接收Json，無返回JS執行
        /// </summary>
        /// <param name="p_Selector">jQuery選擇器</param>
        /// <param name="p_Event">事件</param>
        /// <param name="p_CallBack">囘調函數</param>
        /// <param name="p_ReturnJson">返回給囘調函數的參數2</param>
        public void On(string p_Selector, string p_Event, EventCallBackEmpty p_CallBack) 
        {
            On(p_Selector, p_Event, "{ result: 'OK' }", (string s_Json) =>
            {
                p_CallBack.Invoke();
                return "";
            });
        }

        /// <summary>
        /// 将JS对象转成JSON字符串并传回 p_Code例如：var tmpObject = { id:0, name:"yadi" }; return tmpObject;
        /// </summary>
        /// <param name="p_Code"></param>
        /// <param name="p_ResultCallBack"></param>
        public void Json(string p_Code, JsonCallBack p_ResultCallBack)
        {
            try
            {
                string s_Guid = Guid.NewGuid().ToString();
                string s_Js = @"(function(){ ";
                s_Js += "var o_result = (function(){ ";
                s_Js += p_Code;
                s_Js += @" })(); ";
                s_Js += @"var s_result = JSON.stringify(o_result); ";
                s_Js += @"Cef(2, 2, 'json_" + s_Guid + @"', s_result);";
                s_Js += @"  })(); ";
                m_dicJsonEvents.Add("json_" + s_Guid, p_ResultCallBack);
                this.Js(s_Js);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void OnWindowMessage(int p_Msg, OnMessageCallBack p_CallBack) 
        {
            m_MessageCallBacks.Add(p_Msg, p_CallBack);
            m_ChromeHost.PosMsgs.Add(p_Msg);
        }

        /// <summary>
        /// 簡化版消息框
        /// </summary>
        /// <returns></returns>
        public DialogResult MsgBox(string p_Text, MsgType p_Type = MsgType.Information, string p_Title = "") 
        {
            MessageBoxButtons e_Buttons = MessageBoxButtons.OK;
            MessageBoxDefaultButton e_Default = MessageBoxDefaultButton.Button1;
            switch (p_Type)
            {
                case MsgType.Question:
                    e_Buttons = MessageBoxButtons.YesNo;
                    e_Default = MessageBoxDefaultButton.Button2;
                    break;
                case MsgType.Confirm:
                    e_Buttons = MessageBoxButtons.OKCancel;
                    e_Default = MessageBoxDefaultButton.Button2;
                    p_Type = MsgType.Question;
                    break;
                case MsgType.Information:
                    e_Buttons = MessageBoxButtons.OK;
                    break;
                case MsgType.Warning:
                    e_Buttons = MessageBoxButtons.OK;
                    break;
                default:
                    break;
            }
            return MessageBox.Show(this, p_Text, p_Title.Trim() == "" ? this.Text : p_Title, e_Buttons, (MessageBoxIcon)p_Type, e_Default);
        }

        /// <summary>
        /// JS消息传回 （注意：JS的消息0~10为内部消息，请使用以外的消息值）
        /// </summary>
        /// <param name="p_Msg"></param>
        /// <returns></returns>
        virtual protected string JsRecvData(IntPtr hWnd, int msg, int param_int, string param_str, string data)
        {
            try
            {
                if (hWnd != this.Handle)
                {
                    return "";
                }

                //页面载入完成
                if (msg == 1)
                {
                    string s_Js = @"";
                    s_Js += "function Cef(nMsg, nParam, sParam, sData) { ";
                    s_Js += "var o_cefobj = new cef.cefmsg.msg_object; ";
                    s_Js += "var vParam = sParam == 'undefined'?' ':sParam; ";
                    s_Js += "var vData = sData == 'undefined'?' ':sData; ";
                    s_Js += "return o_cefobj.SendMessage("+ this.Handle.ToString() +", nMsg, nParam, vParam, vData); ";
                    s_Js += "} ";
                    Js(s_Js);

                    if (LoadEnd != null)
                    {
                        LoadEnd.Invoke(this);
                    }

                    return "";
                }

                //事件回调
                if (msg == 2 && param_int == 1)
                {
                    if (param_str.Trim() != "undefined")
                    {
                        if (m_dicJsEventEvents.ContainsKey(param_str))
                        {
                            if (m_dicJsEventEvents[param_str] != null)
                            {
                                return m_dicJsEventEvents[param_str].Invoke(data);
                            }
                        }
                    }
                    return "";
                }

                //JSON回调
                if (msg == 2 && param_int == 2)
                {
                    if (param_str.Trim() != "undefined" && data.Trim() != "undefined")
                    {
                        if (m_dicJsonEvents.ContainsKey(param_str))
                        {
                            if (m_dicJsonEvents[param_str] != null)
                            {
                                m_dicJsonEvents[param_str].Invoke(data);
                            }
                            m_dicJsonEvents.Remove(param_str);
                        }
                    }
                    return "";
                }

                if (RecvData != null)
                {
                    return RecvData.Invoke(hWnd, msg, param_int, param_str, data);
                }
            
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return "";
        }

        /// <summary>
        /// 设置鼠标可拖拽的区域
        /// </summary>
        /// <param name="p_Rect"></param>
        public void SetMouseDragRect(Rectangle p_Rect, bool p_Enabled = true)
        {
            try
            {
                for (int i = 0; i < m_MouseDragRects.Count; i++)
                {
                    if (m_MouseDragRects[i].Rect.Equals(p_Rect))
                    {
                        MouseDragRect m = m_MouseDragRects[i];
                        m.Enabled = p_Enabled;
                        m_MouseDragRects[i] = m;
                        return;
                    }
                }

                m_MouseDragRects.Add(new MouseDragRect { Rect = p_Rect, Enabled = p_Enabled });
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 消息過程
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            try
            {

                if (m_MessageCallBacks.ContainsKey(m.Msg))
                {
                    if (m_MessageCallBacks[m.Msg] != null)
                    {
                        m_MessageCallBacks[m.Msg].Invoke(ref m);
                    }
                }

            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            base.WndProc(ref m);
        }

        /// <summary>
        /// 内部Chrome窗体
        /// </summary>
        private class ChromeHost: NativeWindow
        {
            #region WindowAPI
            [DllImport("user32.dll")]
            private static extern IntPtr FindWindowExA(IntPtr hWnd, IntPtr hWnd2, string sClass, string sTitle);
            [DllImport("user32.dll")]
            private static extern bool IsWindow(IntPtr hWnd);
            [DllImport("user32.dll")]
            private static extern long SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
            [DllImport("user32.dll")]//拖动无窗体的控件
            private static extern bool ReleaseCapture();

            private const int WM_SYSCOMMAND = 0x0112;
            private const int SC_MOVE = 0xF010;
            private const int HTCAPTION = 0x0002;
            private const int WM_LBUTTONDOWN = 0x201;
            private const int WM_LBUTTONUP = 0x202;
            private const int WM_KILLFOCUS = 8;
            #endregion

            private List<int> m_PosMsgs = new List<int>();
            public List<int> PosMsgs{ get{ return m_PosMsgs; } set { m_PosMsgs = value; } }

            #region 找窗口
            private FlyDialog m_Parent;
            private IntPtr m_hHostView;
            public ChromeHost(FlyDialog p_Parent)
            {
                m_Parent = p_Parent;
                IntPtr h_Child;
                h_Child = FindWindowExA(m_Parent.Handle, IntPtr.Zero, "TChromium", string.Empty);
                h_Child = FindWindowExA(h_Child, IntPtr.Zero, "CefBrowserWindow", string.Empty);
                m_hHostView = FindWindowExA(h_Child, IntPtr.Zero, "WebViewHost", string.Empty);
                if (IsWindow(m_hHostView))
                {
                    this.AssignHandle(m_hHostView);
                }
            }
            #endregion



            /// <summary>
            /// 消息過程
            /// </summary>
            /// <param name="m"></param>
            protected override void WndProc(ref Message m)
            {
                //传给主窗口处理的消息
                if (m_PosMsgs.Contains(m.Msg))
                {
                    SendMessage(m_Parent.Handle, m.Msg, m.WParam.ToInt32(), m.LParam.ToInt32());
                }

                base.WndProc(ref m);
            }

        }

    }
}
