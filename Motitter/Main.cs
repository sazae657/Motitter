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

            this.Title = "モチッター";
            this.IconPixmap = TonNurako.GC.Pixmap.FromBuffer(this,  Motitter.Properties.Resources.icon_xpm);

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

            // チートボタン
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
            };
        }

        static readonly int MAX_TIMELINE = 50;

        private int AddTimeline(string name, string status, bool append) {
            if (timelineChildren.Count > MAX_TIMELINE) {
                var count = timelineChildren.Count - MAX_TIMELINE;
                foreach (var item in timelineChildren.GetRange(MAX_TIMELINE, count)) {
                    item.Destroy();
                }
                timelineChildren.RemoveRange(5, count);
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
            label.Value = "[" + name + "]\n" + status;

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

            foreach (var status in this.Tokens.Statuses.HomeTimeline(count => 2)) {
                AddTimeline(status.User.ScreenName, status.Text, true);
            }

           new Task(()=> {
                foreach(var m in Tokens.Streaming.User()
                            .OfType<StatusMessage>()
                            .Select(x => x.Status)
                            ) {
                    this.AppContext.Invoke(() => {
                        AddTimeline(m.User.ScreenName, m.Text, false);
                    });
                }
            }).Start();

        }

        static void Main(string[] args) {
           // Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
            var app = new TonNurako.ApplicationContext();
            app.Name = "TnkMotitter";
            app.FallbackResource.Add("*fontList", "-misc-fixed-medium-r-normal--14-*-*-*-*-*-*-*:");
            app.FallbackResource.Add("*geometry", "+100+100");
            app.FallbackResource.Add("*title", "チッター");

            TonNurako.Application.Run(app, new Program());
        }
    }
}
