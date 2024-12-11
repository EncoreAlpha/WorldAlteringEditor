﻿using Rampastring.XNAUI;
using System;
using TSMapEditor.Models;
using TSMapEditor.Rendering;
using TSMapEditor.UI.Controls;

namespace TSMapEditor.UI.Windows
{
    public class FindWaypointWindow : INItializableWindow
    {
        public FindWaypointWindow(WindowManager windowManager, Map map, IMapView mapView) : base(windowManager)
        {
            this.map = map;
            this.mapView = mapView;
        }

        private readonly Map map;
        private readonly IMapView mapView;

        private EditorNumberTextBox tbWaypoint;

        public override void Initialize()
        {
            Name = nameof(FindWaypointWindow);
            base.Initialize();

            tbWaypoint = FindChild<EditorNumberTextBox>(nameof(tbWaypoint));
            FindChild<EditorButton>("btnFind").LeftClick += BtnFind_LeftClick;
        }

        public void Open()
        {
            tbWaypoint.Text = string.Empty;
            Show();
        }

        private void BtnFind_LeftClick(object sender, EventArgs e)
        {
            int waypointNumber = tbWaypoint.Value;

            Waypoint waypoint = map.Waypoints.Find(wp => wp.Identifier == waypointNumber);
            if (waypoint == null)
            {
                EditorMessageBox.Show(WindowManager, "未找到路径点",
                    "路径点 #" + waypointNumber + " 在地图上不存在！", MessageBoxButtons.OK);

                return;
            }

            Hide();
            mapView.Camera.CenterOnCell(waypoint.Position);
        }
    }
}
