using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreTweet;
using CoreTweet.Streaming;
using TonNurako.Widgets;
using TonNurako.Widgets.Xm;

namespace Motitter
{
    class Program : LayoutWindow<TonNurako.Widgets.Xm.MainWindow>
    {
        TonNurako.Widgets.Xm.ScrolledWindow scrollWindow;
        TonNurako.Widgets.Xm.RowColumn timelineView;

        List<IChild> timelineChildren;

        CoreTweet.Tokens Tokens {
            get; set;
        }

        public override void ShellCreated() {
            timelineChildren = new List<IChild>();

            this.Title = "もちったー";
            this.Layout.MenuBar = CreateMenu();

            var frame = new Form();
            frame.Width = 320;
            frame.Height = 480;
            this.Layout.Children.Add(frame);

            var command = new Form();
            frame.Children.Add(command);
            var imp = new Text();
            imp.MaxLength = 60;
            imp.Rows = 2;
            imp.Width = 220;
            imp.EditMode = EditMode.Multi;
            imp.WordWrap = true;
            imp.ResizeHeight = true;
            command.Children.Add(imp);

            // ﾁーﾄﾎﾞﾀﾝ
            var btn = new PushButtonGadget();
            btn.LeftAttachment = AttachmentType.Widget;
            btn.RightAttachment = AttachmentType.Form;
            btn.LeftWidget = imp;
            btn.LabelString = "チート";
            btn.ActivateEvent +=  (x,y) => {
                var msg = imp.Value;
                Status _status =  this.Tokens.Statuses.Update(status => msg);
                imp.Value = "";
            };
            command.Children.Add(btn);


            scrollWindow = new ScrolledWindow();
            scrollWindow.TopAttachment = AttachmentType.Widget;
            scrollWindow.TopWidget = command;

            scrollWindow.RightAttachment =
            scrollWindow.LeftAttachment =
            scrollWindow.BottomAttachment = AttachmentType.Form;
            scrollWindow.ScrollingPolicy = ScrollingPolicy.Automatic;
            scrollWindow.ScrolledWindowChildType = ScrolledWindowChildType.WorkArea;
            frame.Children.Add(scrollWindow);

            timelineView = new RowColumn();
            timelineView.LeftAttachment = AttachmentType.Form;
            timelineView.RightAttachment = AttachmentType.Form;
            timelineView.Resizable = true;
            scrollWindow.Children.Add(timelineView);


            // ここは自分で入れてね
            Tokens = CoreTweet.Tokens.Create(
                "<ConsumerKey>", "<ConsumerSecret>", "<AccessToken>", "<AccessSecret>");
            
            this.RealizedEvent += (x,y) => {
                LoadTimeLine();
                this.IconPixmap = TonNurako.GC.Pixmap.FromBuffer(this,  Motitter.Properties.Resources.icon_xpm);                
            };
        }

        /// <summary>
        /// ﾒﾆｭー生成
        /// </summary>
        /// <returns></returns>
        IWidget CreateMenu()
        {
            TonNurako.Widgets.Xm.MenuBar smbar;
            smbar = new TonNurako.Widgets.Xm.MenuBar();
            this.Layout.Children.Add(smbar);

            // PDM
            var pdm = new TonNurako.Widgets.Xm.PulldownMenu();
            pdm.Name = "PDM";
            smbar.Children.Add(pdm);

            var cb1 = new TonNurako.Widgets.Xm.CascadeButton();
            cb1.Name = "CB";
            cb1.LabelString = "メニュー(M)";
            cb1.Mnemonic = TonNurako.Data.KeySym.FromName("M");
            cb1.SubMenuId = pdm;
            smbar.Children.Add(cb1);

            pdm.Children.Add(
             ((Func<PushButtonGadget>)(() => {
                 var t = new PushButtonGadget();
                 t.LabelString = "終了";
                 t.ActivateEvent += (X, Y) => {
                    this.Destroy();
                 };
                 return t;
             }))());

            // help
            var helpm = new TonNurako.Widgets.Xm.PulldownMenu();
            helpm.Name = "HELP";
            smbar.Children.Add(helpm);

            var helpb = new TonNurako.Widgets.Xm.CascadeButtonGadget();
            helpb.LabelString = "ヘルプ(H)";
            helpb.Mnemonic = TonNurako.Data.KeySym.FromName("H");
            helpb.SubMenuId = helpm;
            smbar.Children.Add(helpb);
            helpm.Children.Add(
             ((Func<PushButtonGadget>)(() => {
                 var t = new PushButtonGadget();
                 t.LabelString = "これについて";
                 t.ActivateEvent += (X, Y) => {
                     var d = new InformationDialog();
                     d.WidgetCreatedEvent += (x, y) => {
                         d.Items.Cancel.Visible = false;
                         d.Items.Help.Visible = false;
                     };
                     d.WidgetManagedEvent += (x, y) => {
                        d.SymbolPixmap = TonNurako.GC.Pixmap.FromBuffer(this, Properties.Resources.icon_xpm);
                     };
                     d.DialogTitle = "トンヌラコ";
                     d.DialogStyle = DialogStyle.ApplicationModal;
                     d.MessageString = "トンヌラコ";
                     d.OkLabelString = "わかった";

                     this.Layout.Children.Add(d);
                     d.Visible = true;

                 };
                 return t;
             }))());
            smbar.MenuHelpWidget = helpb;
            return smbar;
        }


        static readonly int MAX_TIMELINE = 50;
        /// <summary>
        /// ﾀｲﾑﾗｲﾝ生成
        /// </summary>
        private int AddTimeline(string name, string status, bool append) {
            if (timelineChildren.Count > MAX_TIMELINE) {
                var count = timelineChildren.Count - MAX_TIMELINE;
                foreach (var item in timelineChildren.GetRange(MAX_TIMELINE, count)) {
                    item.Destroy();
                }
                timelineChildren.RemoveRange(MAX_TIMELINE, count);
            }

            var rc = new RowColumn();
            if (! append) {
                rc.RowColumnConstraint.PositionIndex = 0;
            }
            var label = new Text();
            label.Editable = false;
            label.EditMode = EditMode.Multi;
            label.Rows = 2;
            label.BorderWidth = 0;
            label.ShadowThickness = 0;
            label.WordWrap = true;
            label.ResizeHeight = true;
            label.LeftAttachment = AttachmentType.Form;
            label.RightAttachment = AttachmentType.Form;
            label.Value = $"[{name}]\n{status}";

            rc.Children.Add(label);
            rc.Children.Add(new SeparatorGadget());

            this.timelineView.Children.Add(rc);

            if (append) {
                timelineChildren.Add(rc);
            }
            else {
                timelineChildren.Insert(0, rc);
            }
            return 0;
        }


        private void LoadTimeLine()
        {
            if(0 != timelineView.Children.Count()) {
                timelineView.DestroyChildren();
            }
            try {
                foreach (var status in this.Tokens.Statuses.HomeTimeline(count => 2)) {
                    AddTimeline(status.User.ScreenName, status.Text, true);
                }
            }catch(Exception e) {
                var d = new ErrorDialog();
                d.WidgetCreatedEvent += (x, y) => {
                    d.Items.Cancel.Visible = false;
                    d.Items.Help.Visible = false;
                };
                d.DialogTitle = "エラー";
                d.DialogStyle = DialogStyle.ApplicationModal;
                d.MessageString = $"{e.GetType()}\n{e.Message}";
                d.AutoUnmanage = false;
                d.OkLabelString = "わかった";
                d.OkEvent += (w, p) => {
                    this.Destroy();                    
                };
                this.Layout.Children.Add(d);
                d.Visible = true;                
                return;
            }

           Task.Run(()=> {
                foreach(var m in Tokens.Streaming.User()
                            .OfType<StatusMessage>()
                            .Select(x => x.Status)
                            ) {
                    this.AppContext.Invoke(() => {
                        AddTimeline(m.User.ScreenName, m.Text, false);
                    });
                }
            });

        }

        static void Main(string[] args) {
           // Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
            var app = new TonNurako.ApplicationContext();
            app.Name = "TnkMotitter";
            app.FallbackResource.Add("*fontList", "-misc-fixed-medium-r-normal--14-*-*-*-*-*-*-*:");
            app.FallbackResource.Add("*geometry", "+100+100");
            app.FallbackResource.Add("*title", "ﾁｯﾀー");
            var shell = new Program();
            shell.Name = "Motitter";
            
            TonNurako.Application.Run(app, shell);
        }
    }
}
