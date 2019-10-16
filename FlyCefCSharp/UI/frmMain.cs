using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FlyCefCSharp.UI
{
    class frmMain: Fly.FlyDialog
    {
        public frmMain()
        {
            this.CreateConst = new CreateInfo
            {
                border_style = CefBorderStyle.bsSizeable,
                width = (int)(Screen.PrimaryScreen.WorkingArea.Width * 70 / 100),
                height = (int)(Screen.PrimaryScreen.WorkingArea.Height * 70 / 100),
                min_width = 600,
                min_height = 400,
                hicon = IntPtr.Zero
            };
            this.Text = "this is a window";
            this.Create(null, "file:///View/frmMain.html");

            this.LoadEnd += frmMain_LoadEnd;
            this.LoseFocus += frmMain_LoseFocus;
            
        }

        void frmMain_LoseFocus(Fly.FlyDialog Sender)
        {
            Js("hanfcef.UnFocus();");
        }

        void frmMain_LoadEnd(Fly.FlyDialog Sender)
        {
            On("#butClick", "click", "{nomsg:'yes'}", (json) =>
            {
                JObject Obj = (JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                MsgBox(Obj["nomsg"].ToString());
            });
            On("#butPopup", "click", () =>
            {

                MsgBox("this is a message");
            });
            On("#butExit", "click", () =>
            {
                this.Close();
                return "alert('exited');";
            });
        }
    }
}
