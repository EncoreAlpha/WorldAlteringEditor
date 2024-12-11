using Rampastring.Tools;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System;
using System.IO;
using TSMapEditor.Models;
using TSMapEditor.Scripts;
using TSMapEditor.UI.Controls;

namespace TSMapEditor.UI.Windows
{
    public class RunScriptWindow : INItializableWindow
    {
        public RunScriptWindow(WindowManager windowManager, Map map) : base(windowManager)
        {
            this.map = map;
        }

        public event EventHandler ScriptRun;

        private readonly Map map;

        private EditorListBox lbScriptFiles;

        private string scriptPath;

        public override void Initialize()
        {
            Name = nameof(RunScriptWindow);
            base.Initialize();

            lbScriptFiles = FindChild<EditorListBox>(nameof(lbScriptFiles));
            FindChild<EditorButton>("btnRunScript").LeftClick += BtnRunScript_LeftClick;
        }

        private void BtnRunScript_LeftClick(object sender, EventArgs e)
        {
            if (lbScriptFiles.SelectedItem == null)
                return;

            string filePath = (string)lbScriptFiles.SelectedItem.Tag;
            if (!File.Exists(filePath))
            {
                EditorMessageBox.Show(WindowManager, "找不到文件",
                    "所选文件不存在！可能被删除了？", MessageBoxButtons.OK);

                return;
            }

            scriptPath = filePath;

            (string error, string confirmation) = ScriptRunner.GetDescriptionFromScript(map, filePath);

            if (error != null)
            {
                Logger.Log("Compilation error when attempting to run fetch script description: " + error);
                EditorMessageBox.Show(WindowManager, "错误",
                    "编译脚本失败！请检查语法，或联系作者寻求支持。" + Environment.NewLine + Environment.NewLine +
                    "返回的错误信息: " + error, MessageBoxButtons.OK);
                return;
            }

            if (confirmation == null)
            {
                EditorMessageBox.Show(WindowManager, "错误", "脚本没有提供说明！", MessageBoxButtons.OK);
                return;
            }

            confirmation = Renderer.FixText(confirmation, Constants.UIDefaultFont, Width).Text;

            var messageBox = EditorMessageBox.Show(WindowManager, "您确定吗?",
                confirmation, MessageBoxButtons.YesNo);
            messageBox.YesClickedAction = (_) => ApplyCode();
        }

        private void ApplyCode()
        {
            if (scriptPath == null)
                throw new InvalidOperationException("Pending script path is null!");

            string result = ScriptRunner.RunScript(map, scriptPath);
            result = Renderer.FixText(result, Constants.UIDefaultFont, Width).Text;

            EditorMessageBox.Show(WindowManager, "结果", result, MessageBoxButtons.OK);
            ScriptRun?.Invoke(this, EventArgs.Empty);
        }

        public void Open()
        {
            lbScriptFiles.Clear();

            string directoryPath = Path.Combine(Environment.CurrentDirectory, "Config", "Scripts");

            if (!Directory.Exists(directoryPath))
            {
                Logger.Log("WAE scipts directory not found!");
                EditorMessageBox.Show(WindowManager, "错误", "未找到脚本目录！\r\n\r\n预期目录: " + directoryPath, MessageBoxButtons.OK);
                return;
            }

            var iniFiles = Directory.GetFiles(directoryPath, "*.cs");

            foreach (string filePath in iniFiles)
            {
                lbScriptFiles.AddItem(new XNAListBoxItem(Path.GetFileName(filePath)) { Tag = filePath });
            }

            Show();
        }
    }
}
