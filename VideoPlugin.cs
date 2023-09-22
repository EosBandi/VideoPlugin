//----------------------------------------------------------
// VideoPlugin - Mission Planner Plugin for displaying low latency video NOT in the HUD
// V1.0 by EosBandi - 2023.09.22
//
// Requires https://github.com/Eosbandi/Mvp.net 
//
// Video URL can be set in CAM_URL parameter in config.xml The setting will be there after the first run
// but you can add it upfront :
// <CAM_URL>full url for video stream</CAM_URL>
//
// Clone https://github.com/EosBandi/Mvp.net and https://github.com/EosBandi/VideoPlugin repositories
// then add both projects to the "Plugins" folder in the Mission Planner Solution root. (Not the plugins folder in the Mission Planner project!)
// Build Mvp.net then VideoPlugin then build and start MissionPlanner as usual
// 
// Note: This is a POC code, for customisations contact me at eosbandi@eosbandi.com
//----------------------------------------------------------

using MissionPlanner;
using MissionPlanner.Plugin;
using MissionPlanner.Utilities;
using System;
using System.Windows.Forms;
using System.Linq;

namespace VideoPlugin
{

    public class VideoPlugin : Plugin
    {

        public Mpv.NET.Player.MpvPlayer player;
        SplitContainer mainHSc;
        public Panel videoPanel;
        
        public bool videoDoubleSized = false;
        private bool videoPlayerSmall = true;
        private bool initDone = false;

        public override string Name
        {
            get { return "VideoPlugin"; }
        }

        public override string Version
        {
            get { return "1.0"; }
        }

        public override string Author
        {
            get { return "Andras Schaffer (EOSBandi)"; }
        }

        //[DebuggerHidden]
        public override bool Init()
		//Init called when the plugin dll is loaded
        {

            loopratehz = 1;  //Loop runs every second (The value is in Hertz, so 2 means every 500ms, 0.1f means every 10 second...) 
            return true;	 // If it is false then plugin will not load
        }

        public override bool Loaded()
		//Loaded called after the plugin dll successfully loaded
        {
                try
                {
                    mainHSc = MainV2.instance.FlightData.Controls.Find("MainH", true).First() as SplitContainer;

                    videoPanel = new System.Windows.Forms.Panel() { Name = "videoPanel", Height = 240, Width = 320, Anchor = (AnchorStyles.Bottom | AnchorStyles.Left), Visible = true };
                    videoPanel.Location = new System.Drawing.Point(0, mainHSc.Height-265);
                    videoPanel.DoubleClick += VideoPanel_DoubleClicked;

                    mainHSc.Panel2.Controls.Add(videoPanel);
                    videoPanel.BringToFront();

                    player = new Mpv.NET.Player.MpvPlayer(videoPanel.Handle, AppDomain.CurrentDomain.BaseDirectory + @"plugins\\" + "mpv-1.dll");
                    player.LoadConfig(AppDomain.CurrentDomain.BaseDirectory + @"plugins\\" + "mpvnet.conf");
                    player.AutoPlay = true;
                    string url = Settings.Instance.GetString("CAM_URL", @"https://cdn.flowplayer.com/a30bd6bc-f98b-47bc-abf5-97633d4faea0/hls/de3f6ca7-2db3-4689-8160-0f574a5996ad/playlist.m3u8");
                    player.Load(url);

                }
                catch (Mpv.NET.API.MpvAPIException ex)
                {
                    Console.WriteLine(ex);
                }

            ToolStripMenuItem menuitem = new System.Windows.Forms.ToolStripMenuItem() { Text = "Switch Video/Map" };
            menuitem.Click += VideoSwitchMenuitem_Click;
            Host.FDMenuMap.Items.Add(menuitem);


            return true;     //If it is false plugin will not start (loop will not called)
        }

        public override bool Loop()
        //Loop is called in regular intervalls (set by loopratehz)
        {


            //Since the Loaded() method can be called before the GUI init (resize) finished, we have to know when the GUI init is finished
            //Apparently the last step in the GUI init is to set the MenuArdupilot.Image value, we check for it.

            MainV2.instance.BeginInvoke((MethodInvoker)(() =>
            {
                if (Host.MainForm.MenuArduPilot.Image != null && player != null && initDone == false)
                {
                    SplitContainer sc1 = MainV2.instance.FlightData.Controls.Find("splitContainer1", true).First() as SplitContainer;
                    int y = sc1.Height;
                    videoPanel.Location = new System.Drawing.Point(0, y - 265);
                    initDone = true;

                }
            }));

            return true;	//Return value is not used
        }

        public override bool Exit()
		//Exit called when plugin is terminated (usually when Mission Planner is exiting)
        {
            player.Dispose();
            return true;	//Return value is not used
        }
        private void VideoSwitchMenuitem_Click(object sender, EventArgs e)
        {
            SplitContainer sc = Host.MainForm.FlightData.Controls.Find("splitContainer1", true).FirstOrDefault() as SplitContainer;

            if (videoPlayerSmall)
            {
                //Currently it is in a small window, set map to small and video to big

                //Stop player
                player.Stop();
                videoPanel.Controls.Add(Host.FDGMapControl);
                player.SetMpvHost(sc.Panel2.Handle);
                string url = Settings.Instance.GetString("CAM_URL", @"https://cdn.flowplayer.com/a30bd6bc-f98b-47bc-abf5-97633d4faea0/hls/de3f6ca7-2db3-4689-8160-0f574a5996ad/playlist.m3u8");
                player.Load(url);

                videoPlayerSmall = false;

            }
            else
            {
                player.Stop();
                sc.Panel2.Controls.Add(Host.FDGMapControl);
                player.SetMpvHost(videoPanel.Handle);
                string url = Settings.Instance.GetString("CAM_URL", @"https://cdn.flowplayer.com/a30bd6bc-f98b-47bc-abf5-97633d4faea0/hls/de3f6ca7-2db3-4689-8160-0f574a5996ad/playlist.m3u8");
                player.Load(url);

                videoPlayerSmall = true;
            }


        }



        private void VideoPanel_DoubleClicked(object sender, EventArgs e)
        {
            SplitContainer sc1 = MainV2.instance.FlightData.Controls.Find("splitContainer1", true).First() as SplitContainer;

            if (videoDoubleSized)
            {
                videoPanel.Height = 240;
                videoPanel.Width = 320;
                videoDoubleSized = false;
                int y = sc1.Height;
                videoPanel.Location = new System.Drawing.Point(0, y - 265);
            }
            else
            {
                videoPanel.Height = 480;
                videoPanel.Width = 640;
                videoDoubleSized = true;
                int y = sc1.Height;
                videoPanel.Location = new System.Drawing.Point(0, y - 505);
            }
        }




    }
}