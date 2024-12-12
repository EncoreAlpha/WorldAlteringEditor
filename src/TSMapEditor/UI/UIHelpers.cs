using Rampastring.XNAUI.XNAControls;
using TSMapEditor.UI.Controls;

namespace TSMapEditor.UI
{
    public static class UIHelpers
    {
        public static void AddSearchTipsBoxToControl(XNAControl control)
        {
            var lblSearchTips = new XNALabel(control.WindowManager);
            lblSearchTips.Name = nameof(lblSearchTips);
            lblSearchTips.Text = "?";
            lblSearchTips.X = control.Width - Constants.UIEmptySideSpace - lblSearchTips.Width;
            lblSearchTips.Y = (control.Height - lblSearchTips.Height) / 2;
            control.AddChild(lblSearchTips);
            var tooltip = new ToolTip(control.WindowManager, lblSearchTips);
            tooltip.Text = "搜索提示\r\n\r\n开启文本框后:\r\n- 按 ENTER 键移动到列表中的下一个匹配项\r\n- 按 ESC 键清除搜索";
        }
    }
}
