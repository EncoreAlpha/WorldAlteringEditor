using Microsoft.Xna.Framework.Input;
using Rampastring.Tools;
using System.Collections.Generic;
using TSMapEditor.Settings;

namespace TSMapEditor.UI
{
    public class KeyboardCommands
    {
        public KeyboardCommands()
        {
            Commands = new List<KeyboardCommand>()
            {
                Undo,
                Redo,
                Save,
                ConfigureCopiedObjects,
                Copy,
                CopyCustomShape,
                Paste,
                NextTile,
                PreviousTile,
                NextTileSet,
                PreviousTileSet,
                NextSidebarNode,
                PreviousSidebarNode,
                FrameworkMode,
                NextBrushSize,
                PreviousBrushSize,
                DeleteObject,
                ToggleAutoLAT,
                ToggleMapWideOverlay,
                Toggle2DMode,
                ZoomIn,
                ZoomOut,
                ResetZoomLevel,
                RotateUnit,
                RotateUnitOneStep,
                PlaceTerrainBelow,
                FillTerrain,
                CloneObject,
                OverlapObjects,
                ViewMegamap,
                GenerateTerrain,
                ConfigureTerrainGenerator,
                PlaceTunnel,
                ToggleFullscreen,
                AdjustTileHeightUp,
                AdjustTileHeightDown,
                PlaceConnectedTile,
                RepeatConnectedTile,

                BuildingMenu,
                InfantryMenu,
                VehicleMenu,
                AircraftMenu,
                NavalMenu,
                TerrainObjectMenu,
                OverlayMenu,
                SmudgeMenu
            };

            // Theoretically not optimal for performance, but
            // cleaner this way
            if (Constants.IsFlatWorld)
                Commands.Remove(Toggle2DMode);
        }

        public void ReadFromSettings()
        {
            IniFile iniFile = UserSettings.Instance.UserSettingsIni;

            foreach (var command in Commands)
            {
                string dataString = iniFile.GetStringValue("Keybinds", command.ININame, null);
                if (string.IsNullOrWhiteSpace(dataString))
                    continue;

                command.Key.ApplyDataString(dataString);
            }
        }

        public void WriteToSettings()
        {
            IniFile iniFile = UserSettings.Instance.UserSettingsIni;

            foreach (var command in Commands)
            {
                iniFile.SetStringValue("Keybinds", command.ININame, command.Key.GetDataString());
            }
        }

        public void ClearCommandSubscriptions()
        {
            foreach (var command in Commands)
                command.ClearSubscriptions();
        }


        public static KeyboardCommands Instance { get; set; }

        public List<KeyboardCommand> Commands { get; }

        public KeyboardCommand Undo { get; } = new KeyboardCommand("撤销", "撤销", new KeyboardCommandInput(Keys.Z, KeyboardModifiers.Ctrl));
        public KeyboardCommand Redo { get; } = new KeyboardCommand("重做", "重做", new KeyboardCommandInput(Keys.Y, KeyboardModifiers.Ctrl));
        public KeyboardCommand Save { get; } = new KeyboardCommand("保存", "保存地图", new KeyboardCommandInput(Keys.S, KeyboardModifiers.Ctrl));
        public KeyboardCommand ConfigureCopiedObjects { get; } = new KeyboardCommand("配置复制对象", "配置复制对象", new KeyboardCommandInput(Keys.None, KeyboardModifiers.None), false);
        public KeyboardCommand Copy { get; } = new KeyboardCommand("复制", "复制", new KeyboardCommandInput(Keys.C, KeyboardModifiers.Ctrl));
        public KeyboardCommand CopyCustomShape { get; } = new KeyboardCommand("自定义形状复制", "自定义形状复制", new KeyboardCommandInput(Keys.C, KeyboardModifiers.Alt));
        public KeyboardCommand Paste { get; } = new KeyboardCommand("粘贴", "粘贴", new KeyboardCommandInput(Keys.V, KeyboardModifiers.Ctrl));
        public KeyboardCommand NextTile { get; } = new KeyboardCommand("下一个图块", "选择下一个图块", new KeyboardCommandInput(Keys.M, KeyboardModifiers.None));
        public KeyboardCommand PreviousTile { get; } = new KeyboardCommand("上一个图块", "选择上一个图块", new KeyboardCommandInput(Keys.N, KeyboardModifiers.None));
        public KeyboardCommand NextTileSet { get; } = new KeyboardCommand("下一个图块集", "选择下一个图块集", new KeyboardCommandInput(Keys.J, KeyboardModifiers.None));
        public KeyboardCommand PreviousTileSet { get; } = new KeyboardCommand("上一个图块集", "选择上一个图块集", new KeyboardCommandInput(Keys.H, KeyboardModifiers.None));
        public KeyboardCommand NextSidebarNode { get; } = new KeyboardCommand("下一个侧边栏选项", "选择下一个侧边栏选项", new KeyboardCommandInput(Keys.P, KeyboardModifiers.None));
        public KeyboardCommand PreviousSidebarNode { get; } = new KeyboardCommand("上一个侧边栏选项", "选择上一个侧边栏选项", new KeyboardCommandInput(Keys.O, KeyboardModifiers.None));
        public KeyboardCommand FrameworkMode { get; } = new KeyboardCommand("框架模式", "框架模式", new KeyboardCommandInput(Keys.F, KeyboardModifiers.Shift));
        public KeyboardCommand NextBrushSize { get; } = new KeyboardCommand("下一个笔刷大小", "选择下一个笔刷大小", new KeyboardCommandInput(Keys.OemPlus, KeyboardModifiers.None));
        public KeyboardCommand PreviousBrushSize { get; } = new KeyboardCommand("上一个笔刷大小", "选择上一个笔刷大小", new KeyboardCommandInput(Keys.D0, KeyboardModifiers.None));
        public KeyboardCommand DeleteObject { get; } = new KeyboardCommand("删除对象", "删除对象", new KeyboardCommandInput(Keys.Delete, KeyboardModifiers.None));
        public KeyboardCommand ToggleAutoLAT { get; } = new KeyboardCommand("开关自动-LAT", "开关自动-LAT", new KeyboardCommandInput(Keys.L, KeyboardModifiers.Ctrl));
        public KeyboardCommand ToggleMapWideOverlay { get; } = new KeyboardCommand("开关地图投影", "开关地图投影", new KeyboardCommandInput(Keys.F2, KeyboardModifiers.None));
        public KeyboardCommand Toggle2DMode { get; } = new KeyboardCommand("开关二维模式", "开关二维模式", new KeyboardCommandInput(Keys.D, KeyboardModifiers.Shift));
        public KeyboardCommand ZoomIn { get; } = new KeyboardCommand("放大", "放大", new KeyboardCommandInput(Keys.OemPlus, KeyboardModifiers.Ctrl));
        public KeyboardCommand ZoomOut { get; } = new KeyboardCommand("缩小", "缩小", new KeyboardCommandInput(Keys.OemMinus, KeyboardModifiers.Ctrl));
        public KeyboardCommand ResetZoomLevel { get; } = new KeyboardCommand("重置缩放等级", "重置缩放等级", new KeyboardCommandInput(Keys.D0, KeyboardModifiers.Ctrl));
        public KeyboardCommand RotateUnit { get; } = new KeyboardCommand("旋转单位", "旋转单位", new KeyboardCommandInput(Keys.A, KeyboardModifiers.None));
        public KeyboardCommand RotateUnitOneStep { get; } = new KeyboardCommand("单次旋转单位", "单次旋转单位", new KeyboardCommandInput(Keys.A, KeyboardModifiers.Shift));
        public KeyboardCommand PlaceTerrainBelow { get; } = new KeyboardCommand("从底部放置", "在光标底部放置地形", new KeyboardCommandInput(Keys.None, KeyboardModifiers.Alt), true);
        public KeyboardCommand FillTerrain { get; } = new KeyboardCommand("填充地形", "填充地形(1x1)", new KeyboardCommandInput(Keys.None, KeyboardModifiers.Ctrl), true);
        public KeyboardCommand CloneObject { get; } = new KeyboardCommand("复制对象", "复制对象(修改)", new KeyboardCommandInput(Keys.None, KeyboardModifiers.Shift), true);
        public KeyboardCommand OverlapObjects { get; } = new KeyboardCommand("重叠对象", "重叠对象(修改)", new KeyboardCommandInput(Keys.None, KeyboardModifiers.Alt), true);
        public KeyboardCommand ViewMegamap { get; } = new KeyboardCommand("查看全图", "查看全图预览", new KeyboardCommandInput(Keys.F12, KeyboardModifiers.None));
        public KeyboardCommand GenerateTerrain { get; } = new KeyboardCommand("生成地形", "生成地形", new KeyboardCommandInput(Keys.G, KeyboardModifiers.Ctrl));
        public KeyboardCommand ConfigureTerrainGenerator { get; } = new KeyboardCommand("配置地形生成器", "配置地形生成器", new KeyboardCommandInput(Keys.G, KeyboardModifiers.Alt));
        public KeyboardCommand PlaceTunnel { get; } = new KeyboardCommand("放置隧道", "放置隧道", new KeyboardCommandInput(Keys.OemPeriod, KeyboardModifiers.None));
        public KeyboardCommand ToggleFullscreen { get; } = new KeyboardCommand("开关全屏模式", "开关全屏模式", new KeyboardCommandInput(Keys.F11, KeyboardModifiers.None));
        public KeyboardCommand AdjustTileHeightUp { get; } = new KeyboardCommand("向上调整图块高度", "向上调整图块高度", new KeyboardCommandInput(Keys.PageUp, KeyboardModifiers.None), forActionsOnly:true);
        public KeyboardCommand AdjustTileHeightDown { get; } = new KeyboardCommand("向下调整图块高度", "向下调整图块高度", new KeyboardCommandInput(Keys.PageDown, KeyboardModifiers.None), forActionsOnly:true);
        public KeyboardCommand PlaceConnectedTile { get; } = new KeyboardCommand("放置连接图块", "放置连接图块", new KeyboardCommandInput(Keys.D, KeyboardModifiers.Alt));
        public KeyboardCommand RepeatConnectedTile { get; } = new KeyboardCommand("重复上一次绘制连接图块", "重复上一次绘制连接图块", new KeyboardCommandInput(Keys.D, KeyboardModifiers.Ctrl));

        public KeyboardCommand BuildingMenu { get; } = new KeyboardCommand("建筑菜单", "建筑菜单", new KeyboardCommandInput(Keys.D1, KeyboardModifiers.None));
        public KeyboardCommand InfantryMenu { get; } = new KeyboardCommand("步兵菜单", "步兵菜单", new KeyboardCommandInput(Keys.D2, KeyboardModifiers.None));
        public KeyboardCommand VehicleMenu { get; } = new KeyboardCommand("载具菜单", "载具菜单", new KeyboardCommandInput(Keys.D3, KeyboardModifiers.None));
        public KeyboardCommand AircraftMenu { get; } = new KeyboardCommand("飞行器菜单", "飞行器菜单", new KeyboardCommandInput(Keys.D4, KeyboardModifiers.None));
        public KeyboardCommand NavalMenu { get; } = new KeyboardCommand("海军菜单", "海军菜单", new KeyboardCommandInput(Keys.D5, KeyboardModifiers.None));
        public KeyboardCommand TerrainObjectMenu { get; } = new KeyboardCommand("地形对象菜单", "地形对象菜单", new KeyboardCommandInput(Keys.D6, KeyboardModifiers.None));
        public KeyboardCommand OverlayMenu { get; } = new KeyboardCommand("覆盖物菜单", "覆盖物菜单", new KeyboardCommandInput(Keys.D7, KeyboardModifiers.None));
        public KeyboardCommand SmudgeMenu { get; } = new KeyboardCommand("污染菜单", "污染菜单", new KeyboardCommandInput(Keys.D8, KeyboardModifiers.None));
    }
}
