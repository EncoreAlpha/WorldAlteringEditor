using Microsoft.Xna.Framework;
using Rampastring.Tools;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System;
using System.IO;
using TSMapEditor.Settings;
using TSMapEditor.UI.Controls;
using TSMapEditor.UI.Windows;
using TSMapEditor.UI.Windows.MainMenuWindows;
using MessageBoxButtons = TSMapEditor.UI.Windows.MessageBoxButtons;

#if WINDOWS
using System.Windows.Forms;
using Microsoft.Win32;
#endif

namespace TSMapEditor.UI
{
    public class MainMenu : EditorPanel
    {
        private const string DirectoryPrefix = "<目录> ";
        private const int BrowseButtonWidth = 70;

        public MainMenu(WindowManager windowManager) : base(windowManager)
        {
        }

        private string gameDirectory;

        private EditorTextBox tbGameDirectory;
        private EditorButton btnBrowseGameDirectory;
        private EditorTextBox tbMapPath;
        private EditorButton btnBrowseMapPath;
        private EditorButton btnLoad;
        private FileBrowserListBox lbFileList;

        private SettingsPanel settingsPanel;

        private int loadingStage;

        public override void Initialize()
        {
            bool hasRecentFiles = UserSettings.Instance.RecentFiles.GetEntries().Count > 0;

            Name = nameof(MainMenu);
            Width = 570;
            Height = WindowManager.RenderResolutionY;

            var lblGameDirectory = new XNALabel(WindowManager);
            lblGameDirectory.Name = nameof(lblGameDirectory);
            lblGameDirectory.X = Constants.UIEmptySideSpace;
            lblGameDirectory.Y = Constants.UIEmptyTopSpace;
            lblGameDirectory.Text = "游戏目录:";
            AddChild(lblGameDirectory);

            tbGameDirectory = new EditorTextBox(WindowManager);
            tbGameDirectory.Name = nameof(tbGameDirectory);
            tbGameDirectory.AllowSemicolon = true;
            tbGameDirectory.X = Constants.UIEmptySideSpace;
            tbGameDirectory.Y = lblGameDirectory.Bottom + Constants.UIVerticalSpacing;
            tbGameDirectory.Width = Width - Constants.UIEmptySideSpace * 3 - BrowseButtonWidth;
            tbGameDirectory.Text = UserSettings.Instance.GameDirectory;
            if (string.IsNullOrWhiteSpace(tbGameDirectory.Text))
            {
                ReadGameInstallDirectoryFromRegistry();
            }

#if DEBUG
            // When debugging we might often switch between configs - make it a bit more convenient
            string expectedPath = Path.Combine(tbGameDirectory.Text, Constants.ExpectedClientExecutableName);
            if (!File.Exists(expectedPath))
            {
                ReadGameInstallDirectoryFromRegistry();
            }
#endif

            tbGameDirectory.TextChanged += TbGameDirectory_TextChanged;
            AddChild(tbGameDirectory);

            btnBrowseGameDirectory = new EditorButton(WindowManager);
            btnBrowseGameDirectory.Name = nameof(btnBrowseGameDirectory);
            btnBrowseGameDirectory.Width = BrowseButtonWidth;
            btnBrowseGameDirectory.Text = "浏览...";
            btnBrowseGameDirectory.Y = tbGameDirectory.Y;
            btnBrowseGameDirectory.X = tbGameDirectory.Right + Constants.UIEmptySideSpace;
            btnBrowseGameDirectory.Height = tbGameDirectory.Height;
            AddChild(btnBrowseGameDirectory);
            btnBrowseGameDirectory.LeftClick += BtnBrowseGameDirectory_LeftClick;

            var lblMapPath = new XNALabel(WindowManager);
            lblMapPath.Name = nameof(lblMapPath);
            lblMapPath.X = Constants.UIEmptySideSpace;
            lblMapPath.Y = tbGameDirectory.Bottom + Constants.UIEmptyTopSpace;
            lblMapPath.Text = "要加载的地图文件的路径（可以是相对于游戏目录的路径）:";
            AddChild(lblMapPath);

            tbMapPath = new EditorTextBox(WindowManager);
            tbMapPath.Name = nameof(tbMapPath);
            tbMapPath.AllowSemicolon = true;
            tbMapPath.X = Constants.UIEmptySideSpace;
            tbMapPath.Y = lblMapPath.Bottom + Constants.UIVerticalSpacing;
            tbMapPath.Width = Width - Constants.UIEmptySideSpace * 3 - BrowseButtonWidth;
            tbMapPath.Text = UserSettings.Instance.LastScenarioPath;
            AddChild(tbMapPath);

            btnBrowseMapPath = new EditorButton(WindowManager);
            btnBrowseMapPath.Name = nameof(btnBrowseMapPath);
            btnBrowseMapPath.Width = BrowseButtonWidth;
            btnBrowseMapPath.Text = "浏览...";
            btnBrowseMapPath.Y = tbMapPath.Y;
            btnBrowseMapPath.X = tbMapPath.Right + Constants.UIEmptySideSpace;
            btnBrowseMapPath.Height = tbMapPath.Height;
            AddChild(btnBrowseMapPath);
            btnBrowseMapPath.LeftClick += BtnBrowseMapPath_LeftClick;

            btnLoad = new EditorButton(WindowManager);
            btnLoad.Name = nameof(btnLoad);
            btnLoad.Width = 150;
            btnLoad.Text = "载入";
            btnLoad.Y = Height - btnLoad.Height - Constants.UIEmptyBottomSpace;
            btnLoad.X = Width - btnLoad.Width - Constants.UIEmptySideSpace;
            AddChild(btnLoad);
            btnLoad.LeftClick += BtnLoad_LeftClick;

            var btnCreateNewMap = new EditorButton(WindowManager);
            btnCreateNewMap.Name = nameof(btnCreateNewMap);
            btnCreateNewMap.Width = 150;
            btnCreateNewMap.Text = "新建地图...";
            btnCreateNewMap.X = Constants.UIEmptySideSpace;
            btnCreateNewMap.Y = btnLoad.Y;
            AddChild(btnCreateNewMap);
            btnCreateNewMap.LeftClick += BtnCreateNewMap_LeftClick;

            var lblCopyright = new XNALabel(WindowManager);
            lblCopyright.Name = nameof(lblCopyright);
            lblCopyright.Text = "Created by Rampastring";
            lblCopyright.TextColor = UISettings.ActiveSettings.SubtleTextColor;
            AddChild(lblCopyright);
            lblCopyright.CenterOnControlVertically(btnCreateNewMap);
            lblCopyright.X = btnCreateNewMap.Right + ((btnLoad.X - btnCreateNewMap.Right) - lblCopyright.Width) / 2;

            int directoryListingY = tbMapPath.Bottom + Constants.UIVerticalSpacing * 2;

            if (hasRecentFiles)
            {
                const int recentFilesHeight = 150;

                var lblRecentFiles = new XNALabel(WindowManager);
                lblRecentFiles.Name = nameof(lblRecentFiles);
                lblRecentFiles.X = Constants.UIEmptySideSpace;
                lblRecentFiles.Y = directoryListingY;
                lblRecentFiles.Text = "最近打开文件:";
                AddChild(lblRecentFiles);

                var recentFilesPanel = new RecentFilesPanel(WindowManager);
                recentFilesPanel.X = lblRecentFiles.X;
                recentFilesPanel.Y = lblRecentFiles.Bottom + Constants.UIVerticalSpacing;
                recentFilesPanel.Width = Width - (Constants.UIEmptySideSpace * 2);
                recentFilesPanel.Height = recentFilesHeight - lblRecentFiles.Height - (Constants.UIVerticalSpacing * 2);
                recentFilesPanel.FileSelected += RecentFilesPanel_FileSelected;
                AddChild(recentFilesPanel);

                directoryListingY = recentFilesPanel.Bottom + Constants.UIVerticalSpacing;
            }

            var lblDirectoryListing = new XNALabel(WindowManager);
            lblDirectoryListing.Name = nameof(lblDirectoryListing);
            lblDirectoryListing.X = Constants.UIEmptySideSpace;
            lblDirectoryListing.Y = directoryListingY;
            lblDirectoryListing.Text = "或者从下面选择地图文件:";
            AddChild(lblDirectoryListing);

            lbFileList = new FileBrowserListBox(WindowManager);
            lbFileList.Name = nameof(lbFileList);
            lbFileList.X = Constants.UIEmptySideSpace;
            lbFileList.Y = lblDirectoryListing.Bottom + Constants.UIVerticalSpacing;
            lbFileList.Width = Width - (Constants.UIEmptySideSpace * 2);
            lbFileList.Height = btnLoad.Y - Constants.UIEmptyTopSpace - lbFileList.Y;
            lbFileList.FileSelected += LbFileList_FileSelected;
            lbFileList.FileDoubleLeftClick += LbFileList_FileDoubleLeftClick;
            AddChild(lbFileList);

            settingsPanel = new SettingsPanel(WindowManager);
            settingsPanel.Name = nameof(settingsPanel);
            settingsPanel.X = Width;
            settingsPanel.Y = Constants.UIEmptyTopSpace;
            settingsPanel.Height = Height - Constants.UIEmptyTopSpace - Constants.UIEmptyBottomSpace;
            AddChild(settingsPanel);
            Width += settingsPanel.Width + Constants.UIEmptySideSpace;

            string directoryPath = string.Empty;

            if (!string.IsNullOrWhiteSpace(tbGameDirectory.Text))
            {
                directoryPath = tbGameDirectory.Text;

                if (!string.IsNullOrWhiteSpace(tbMapPath.Text))
                {
                    if (Path.IsPathRooted(tbMapPath.Text))
                    {
                        directoryPath = Path.GetDirectoryName(tbMapPath.Text);
                    }
                    else
                    {
                        directoryPath = Path.GetDirectoryName(tbGameDirectory.Text + tbMapPath.Text);
                    }
                }

                directoryPath = directoryPath.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
            }

            lbFileList.DirectoryPath = directoryPath;

            base.Initialize();

            if (Program.args.Length > 0 && !string.IsNullOrWhiteSpace(Program.args[0]))
            {
                if (CheckGameDirectory())
                {
                    tbMapPath.Text = Program.args[0];
                    loadingStage++;
                }
            }
        }

        private void RecentFilesPanel_FileSelected(object sender, FileSelectedEventArgs e)
        {
            tbMapPath.Text = e.FilePath;
            BtnLoad_LeftClick(this, EventArgs.Empty);
        }

        private void LbFileList_FileSelected(object sender, FileSelectionEventArgs e)
        {
            tbMapPath.Text = e.FilePath;
        }

        private void BtnCreateNewMap_LeftClick(object sender, EventArgs e)
        {
            if (!CheckGameDirectory())
                return;

            ApplySettings();
            WindowManager.RemoveControl(this);
            var createMapWindow = new CreateNewMapWindow(WindowManager, false);
            createMapWindow.OnCreateNewMap += CreateMapWindow_OnCreateNewMap;
            WindowManager.AddAndInitializeControl(createMapWindow);
        }

        private void CreateMapWindow_OnCreateNewMap(object sender, CreateNewMapEventArgs e)
        {
            string error = MapSetup.InitializeMap(gameDirectory, true, null, e, WindowManager);
            if (!string.IsNullOrWhiteSpace(error))
                throw new InvalidOperationException("创建新地图失败！错误信息: " + error);

            MapSetup.LoadTheaterGraphics(WindowManager, gameDirectory);
            ((CreateNewMapWindow)sender).OnCreateNewMap -= CreateMapWindow_OnCreateNewMap;
        }

        private void ReadGameInstallDirectoryFromRegistry()
        {
            try
            {
                RegistryKey key;

                if (Constants.InstallPathAtHKLM)
                {
                    key = Registry.LocalMachine.OpenSubKey(Constants.GameRegistryInstallPath);
                }
                else
                {
                    key = Registry.CurrentUser.OpenSubKey(Constants.GameRegistryInstallPath);
                }

                object value = key.GetValue("InstallPath", string.Empty);
                if (!(value is string valueAsString))
                {
                    tbGameDirectory.Text = string.Empty;
                }
                else
                {
                    if (File.Exists(valueAsString))
                        tbGameDirectory.Text = Path.GetDirectoryName(valueAsString);
                    else
                        tbGameDirectory.Text = valueAsString;
                }

                key.Close();
            }
            catch (Exception ex)
            {
                tbGameDirectory.Text = string.Empty;
                Logger.Log("从 Windows 注册表中读取游戏安装路径失败！错误信息: " + ex.Message);
            }
        }

        private void TbGameDirectory_TextChanged(object sender, EventArgs e)
        {
            lbFileList.DirectoryPath = tbGameDirectory.Text;
        }

        private void LbFileList_FileDoubleLeftClick(object sender, EventArgs e)
        {
            BtnLoad_LeftClick(this, EventArgs.Empty);
        }

        private bool CheckGameDirectory()
        {
            if (!File.Exists(Path.Combine(tbGameDirectory.Text, Constants.ExpectedClientExecutableName)))
            {
                EditorMessageBox.Show(WindowManager,
                    "游戏目录无效",
                    $"{Constants.ExpectedClientExecutableName} 未找到，请检查您输入的游戏目录是否正确。",
                    MessageBoxButtons.OK);

                return false;
            }

            gameDirectory = tbGameDirectory.Text;
            if (!gameDirectory.EndsWith("/") && !gameDirectory.EndsWith("\\"))
                gameDirectory += "/";

            return true;
        }

        private void ApplySettings()
        {
            settingsPanel.ApplySettings();

            UserSettings.Instance.GameDirectory.UserDefinedValue = tbGameDirectory.Text;
            UserSettings.Instance.LastScenarioPath.UserDefinedValue = tbMapPath.Text;
            UserSettings.Instance.RecentFiles.PutEntry(tbMapPath.Text);

            bool fullscreenWindowed = UserSettings.Instance.FullscreenWindowed.GetValue();
            bool borderless = UserSettings.Instance.Borderless.GetValue();
            if (fullscreenWindowed && !borderless)
                throw new InvalidOperationException("如果启用了全屏模式，则不能勾选无边框。");

            WindowManager.CenterControlOnScreen(this);

            _ = UserSettings.Instance.SaveSettingsAsync();
        }

        private void BtnBrowseGameDirectory_LeftClick(object sender, EventArgs e)
        {
#if WINDOWS
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = tbGameDirectory.Text;
                openFileDialog.Filter =
                    $"Game executable|{Constants.ExpectedClientExecutableName}";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    tbGameDirectory.Text = Path.GetDirectoryName(openFileDialog.FileName);
                }
            }
#endif
        }

        private void BtnBrowseMapPath_LeftClick(object sender, EventArgs e)
        {
#if WINDOWS
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = tbMapPath.Text;
                openFileDialog.Filter = Constants.OpenFileDialogFilter.Replace(':', ';');
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    tbMapPath.Text = openFileDialog.FileName;
                    BtnLoad_LeftClick(this, new EventArgs());
                }
            }
#endif
        }

        private void BtnLoad_LeftClick(object sender, EventArgs e)
        {
            if (!CheckGameDirectory())
                return;

            UserSettings.Instance.GameDirectory.UserDefinedValue = gameDirectory;

            string mapPath = Path.Combine(gameDirectory, tbMapPath.Text);
            if (Path.IsPathRooted(tbMapPath.Text))
                mapPath = tbMapPath.Text;

            if (!File.Exists(mapPath))
            {
                EditorMessageBox.Show(WindowManager,
                    "地图路径无效",
                    "未找到指定的地图文件。请重新检查地图文件的路径。",
                    MessageBoxButtons.OK);

                return;
            }

            loadingStage = 1;
        }

        public override void Update(GameTime gameTime)
        {
            if (loadingStage == 3)
                LoadMap(tbMapPath.Text);
            else if (loadingStage == 5)
                LoadTheater();

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if (loadingStage > 0)
            {
                loadingStage++;
            }
        }

        private void LoadMap(string mapPath)
        {
            string error = MapSetup.InitializeMap(gameDirectory, false, mapPath, null, WindowManager);

            if (error == null)
            {
                ApplySettings();

                var messageBox = new EditorMessageBox(WindowManager, "载入中", "请稍后,正在载入地图...", MessageBoxButtons.None);
                var dp = new DarkeningPanel(WindowManager);
                AddChild(dp);
                dp.AddChild(messageBox);

                return;
            }

            loadingStage = 0;
            EditorMessageBox.Show(WindowManager, "加载文件出错", error, MessageBoxButtons.OK);
        }

        private void LoadTheater()
        {
            MapSetup.LoadTheaterGraphics(WindowManager, gameDirectory);
            WindowManager.RemoveControl(this);
        }
    }
}
