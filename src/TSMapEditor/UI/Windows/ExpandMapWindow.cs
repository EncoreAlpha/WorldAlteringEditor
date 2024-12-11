using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System;
using TSMapEditor.GameMath;
using TSMapEditor.Models;
using TSMapEditor.UI.Controls;

namespace TSMapEditor.UI.Windows
{
    /// <summary>
    /// A window that allows the user to expand the map.
    /// </summary>
    public class ExpandMapWindow : INItializableWindow
    {
        public ExpandMapWindow(WindowManager windowManager, Map map) : base(windowManager)
        {
            this.map = map;
        }

        private readonly Map map;

        private XNALabel lblCurrentMapSize;
        private EditorNumberTextBox tbExpandNorth;
        private EditorNumberTextBox tbExpandSouth;
        private EditorNumberTextBox tbExpandEast;
        private EditorNumberTextBox tbExpandWest;

        public override void Initialize()
        {
            Name = nameof(ExpandMapWindow);
            base.Initialize();

            lblCurrentMapSize = FindChild<XNALabel>(nameof(lblCurrentMapSize));
            tbExpandNorth = FindChild<EditorNumberTextBox>(nameof(tbExpandNorth));
            tbExpandSouth = FindChild<EditorNumberTextBox>(nameof(tbExpandSouth));
            tbExpandEast = FindChild<EditorNumberTextBox>(nameof(tbExpandEast));
            tbExpandWest = FindChild<EditorNumberTextBox>(nameof(tbExpandWest));
            FindChild<EditorButton>("btnApply").LeftClick += BtnApply_LeftClick;
        }

        private void BtnApply_LeftClick(object sender, EventArgs e)
        {
            int newWidth = map.Size.X + tbExpandEast.Value + tbExpandWest.Value;
            if (newWidth <= 0 || newWidth > Constants.MaxMapWidth)
            {
                EditorMessageBox.Show(WindowManager, "无效宽度",
                    $"给定的值将使地图的宽度 {newWidth}.\r\n它应该介于 1 和 {Constants.MaxMapWidth}之间",
                    MessageBoxButtons.OK);

                return;
            }

            int newHeight = map.Size.Y + tbExpandNorth.Value + tbExpandSouth.Value;
            if (newHeight <= 0 || newHeight > Constants.MaxMapHeight)
            {
                EditorMessageBox.Show(WindowManager, "无效长度",
                    $"给定的值将使地图的长度 {newHeight}.\r\n它应该介于 0 和 {Constants.MaxMapHeight}之间",
                    MessageBoxButtons.OK);

                return;
            }

            int expandNorth = tbExpandNorth.Value;
            int expandEast = tbExpandEast.Value;
            int expandWest = tbExpandWest.Value;

            // Determine shift for expanding map to north.
            // Expanding to the south doesn't require any changes to existing coords.
            // These shifts consider the in-game compass; shifting east means
            // increasing the X coord, while shifting south means increasing the Y coord.
            int eastShift = expandNorth;
            int southShift = expandNorth;

            // Determine shift for expanding map to east
            southShift += expandEast;

            // Determine shift for expanding map to west
            eastShift += expandWest;

            map.Resize(new Point2D(newWidth, newHeight), eastShift, southShift);
            Hide();
        }

        public void Open()
        {
            lblCurrentMapSize.Text = $"当前地图大小： {map.Size.X}x{map.Size.Y}";
            Show();
        }
    }
}
