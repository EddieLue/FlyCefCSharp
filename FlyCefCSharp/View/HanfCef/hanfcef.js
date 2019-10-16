
var G_MenuData = [{
    text: '撤銷',
    func: function () {
        document.execCommand("Undo");
    }
}, {
    text: '複製',
    func: function () {
        document.execCommand("Copy");
    }
}, {
    text: '剪切',
    func: function () {
        document.execCommand("Cut");
    }
}, {
    text: '粘貼',
    func: function () {
        document.execCommand("Paste");
    }
}, {
    text: '刪除',
    func: function () {
        document.execCommand("Delete");
    }
}, {
    text: '全選',
    func: function () {
        document.execCommand("SelectAll");
    }
}];
function G_MenuClick(i) {
    G_MenuData[i].func();
    $("#Text-Menu").hide();
}

var hanfcef = new function () {
    var ready_cnt = 0;

    this.UnFocus = function () {
        $("#Text-Menu").hide();
    };


    //禁選
    this.DisableSelect = function () {
        if (typeof (document.onselectstart) != "undefined") {
            //IE
            document.onselectstart = function () {
                //                //當前焦點不在輸入框上不可以選擇
                //                if (document.activeElement.tagName.toUpperCase() != "INPUT" ||
                //                    document.activeElement.tagName.toUpperCase() != "TEXTAREA") {
                //                    return false;
                //                }
                return false;
            };
        } else {
            //firefox
            document.onmousedown = new Function("return false;");
            document.onmouseup = new Function("return true;");
            document.ondragstart = new Function("return false");
        }
    };
    //禁止拖拽
    this.DisableDrag = function () {
        document.body.ondragstart = function () { window.event.returnValue = false; }
    };
    //禁止菜單
    this.DisableMenu = function () {
        document.oncontextmenu = new Function("event.returnValue=false;");
    };
    this.CefModal = function () {
        this.DisableMenu();
        this.DisableSelect();
        this.DisableDrag();
        $("html").css({ "cursor": "default" });
    };

    this.doc_ready = function () {
        ready_cnt++;

        if (ready_cnt == 1) {
            this.CefModal();
            $("body").append("<span style='color:#837F7F;position:absolute; width:80px; left:0px; bottom:2px; font-size:0.5em;'>By Yadi</span>");
            var menu = "";
            menu = "<ul id='Text-Menu' class='text-mune'>";
            for (var i = 0; i < G_MenuData.length; i++) {
                menu += "<li onclick='G_MenuClick(" + i + ")'>";
                menu += "<span class='G_MenuButton'>" + G_MenuData[i].text + "</span>";
                menu += "</li>";
            }
            menu += "</ul>";
            $("body").prepend(menu);
        }

        $('#Text-Menu li').on('mousedown', function (e) {
            e.preventDefault();
        });
        $('#Text-Menu li button').on('contextmenu', function (e) {
            e.preventDefault();
        });

        $("input[type='text'],textarea").contextmenu(function (ev) {

            var nX = ev.clientX, nY = ev.clientY;

            if ((nY + $("#Text-Menu").height()) > $(document).height()) {
                nY -= $("#Text-Menu").height();
            }
            if ((nX + $("#Text-Menu").width()) > $(document).width()) {
                nX -= $("#Text-Menu").width();
            }

            $("#Text-Menu").css
            (
                {
                    left: nX + "px",
                    top: nY + "px"
                }
            ).show();

            return false;
        }).blur(function () {
            if ($("#Text-Menu").is(":visible")) {
                $(this).focus();
                return false;
            }
        });


        $(document).on("mousedown", function (ev) {
            if ($("#Text-Menu").is(":visible")) {
                if (ev.clientX >= $("#Text-Menu").offset().left
            && ev.clientY >= $("#Text-Menu").offset().top
            && ev.clientX <= ($("#Text-Menu").offset().left + $("#Text-Menu").width())
            && ev.clientY <= ($("#Text-Menu").offset().top + $("#Text-Menu").height())) {
                    return;
                }
            }
            $("#Text-Menu").hide();
        });
    };


}

$(document).ready(function () {
    hanfcef.doc_ready();
});


