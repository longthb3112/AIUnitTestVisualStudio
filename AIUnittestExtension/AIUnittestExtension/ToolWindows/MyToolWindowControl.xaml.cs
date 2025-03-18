using AIUnittestExtension.ToolWindows;
using Community.VisualStudio.Toolkit;
using Microsoft.Win32;
using System.CodeDom.Compiler;
using System.CodeDom;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Microsoft.CodeDom.Providers.DotNetCompilerPlatform;

namespace AIUnittestExtension
{
    public partial class MyToolWindowControl : System.Windows.Controls.UserControl
    {
        public MyToolWindowControl()
        {
            InitializeComponent();

            // Load settings
            var settings = ConfigManager.LoadSettings();
            if (!string.IsNullOrEmpty(settings.APIKey) || !string.IsNullOrEmpty(settings.Model))
            {
                APITextBox.Password = settings.APIKey;
                ModelTextBox.Text = settings.Model;
                FolderPathTextBox.Text = settings.OutputPath;
            }
        }

        private async void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var errMsg = Validate();

            if (!string.IsNullOrEmpty(errMsg))
            {
                await VS.MessageBox.ShowAsync(string.Empty, errMsg, buttons: Microsoft.VisualStudio.Shell.Interop.OLEMSGBUTTON.OLEMSGBUTTON_OK, icon: Microsoft.VisualStudio.Shell.Interop.OLEMSGICON.OLEMSGICON_CRITICAL);
                return;
            }
            else
            {
                //Save settings
                var updatedSettings = new ToolWindowSetting
                {
                    APIKey = APITextBox.Password,
                    Model = ModelTextBox.Text,
                    OutputPath = FolderPathTextBox.Text
                };
                ConfigManager.SaveSettings(updatedSettings);
            }
            
            try
            {
                var docView = await VS.Documents.GetActiveDocumentViewAsync();
                if (docView?.FilePath != null && Path.GetExtension(docView.FilePath).Equals(".cs"))
                {
                    await VS.MessageBox.ShowAsync(string.Empty, docView.FilePath, buttons: Microsoft.VisualStudio.Shell.Interop.OLEMSGBUTTON.OLEMSGBUTTON_OK, icon: Microsoft.VisualStudio.Shell.Interop.OLEMSGICON.OLEMSGICON_INFO);

                    string result = string.Empty;
                    await LoadingIndicator.ShowLoadingAsync(async () =>
                    {
                        IUnitTestHandler handler = new DotNetClassHandler();
                        result = await handler.GenerateUnitTestAsync(docView.FilePath, FolderPathTextBox.Text, APITextBox.Password, ModelTextBox.Text);
                    });

                    await VS.MessageBox.ShowAsync(string.Empty, result, buttons: Microsoft.VisualStudio.Shell.Interop.OLEMSGBUTTON.OLEMSGBUTTON_OK, icon: Microsoft.VisualStudio.Shell.Interop.OLEMSGICON.OLEMSGICON_INFO);
                }
                else
                {
                    await VS.MessageBox.ShowAsync(string.Empty, "Please select a .cs file to generate unit test", buttons: Microsoft.VisualStudio.Shell.Interop.OLEMSGBUTTON.OLEMSGBUTTON_OK, icon: Microsoft.VisualStudio.Shell.Interop.OLEMSGICON.OLEMSGICON_CRITICAL);
                }
            }
            catch (Exception ex)
            {
                await VS.MessageBox.ShowAsync(string.Empty, ex.Message, buttons: Microsoft.VisualStudio.Shell.Interop.OLEMSGBUTTON.OLEMSGBUTTON_OK, icon: Microsoft.VisualStudio.Shell.Interop.OLEMSGICON.OLEMSGICON_CRITICAL);
            }
            
        }
      
        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    FolderPathTextBox.Text = dialog.SelectedPath;
                }
            }
        }

        private string Validate()
        {
            const string CHATGPT_API_KEY_REQUIRED = "ChatGPT api key is required.";
            const string CHATGPT_MODEL_REQUIRED = "ChatGPT model is required.";
            const string OUTPUT_FOLDER_REQUIRED = "Output folder for Unittest is required.";
            var errorMessage = string.Empty;
            if (string.IsNullOrEmpty(APITextBox.Password))
            {
                return CHATGPT_API_KEY_REQUIRED;
            }
            if (string.IsNullOrEmpty(ModelTextBox.Text))
            {
                return CHATGPT_MODEL_REQUIRED;
            }
            if (string.IsNullOrEmpty(FolderPathTextBox.Text))
            {
                return OUTPUT_FOLDER_REQUIRED;
            }

            return string.Empty;
        }

    }
}