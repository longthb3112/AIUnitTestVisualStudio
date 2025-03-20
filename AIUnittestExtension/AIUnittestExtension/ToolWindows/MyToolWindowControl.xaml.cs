using AIUnittestExtension.ToolWindows;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System.IO;
using System.Windows;
using System.Windows.Forms;

namespace AIUnittestExtension
{
    public partial class MyToolWindowControl : System.Windows.Controls.UserControl
    {
        private const string DEFAULT_REQUIREMENTS = 
            $"- Use xUnit for testing." +
            $"\n- **Always include `[Fact]`** before each test method." +
            $"\n- Use StaticMock **only** for database-related operations (e.g., `DbContext`, `DbSet<T>`, stored procedures)." +
            "\n- **Do not** mock factory methods, utility functions, or non-database logic (e.g., `{className}.Create`)." +
            $"\n- **Only mock static methods that interact with the database.**" +
            $"\n- Ensure that condition factories, dictionary lookups, and in-memory logic **are not mocked**." +
            $"\n- Include both positive and negative test cases to verify expected behavior." +
            $"\n- Ensure proper assertions and exception handling." +
            $"\n- Do **not** include the namespace or class definition in the output." +
            $"\n- **Do not** generate test cases involving exceeding MinValue or MaxValue (e.g., MaxValue + 1, MinValue - 1)." +
            $"\n- Return **only** valid C# test methods, without enclosing them in a class." +
            $"\n- **Do not include** Markdown formatting (e.g., ` ```csharp` and ` ``` `)." +
            "\n- Use `{className}` when calling the method under test." +
            "\n- **Do not add extra properties to entities that are not used in `{methodCode}`.**" +
            "\n- If a database entity is returned, do not add fields like `Street`, `City`, etc., unless they are present in `{methodCode}`." +
            "\n- **StaticMock.Mock.Setup() should only be used for methods inside it—not for `{className}.methodName` itself.**";
        public MyToolWindowControl()
        {
            InitializeComponent();
            LoadSettings();
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
                    OutputPath = FolderPathTextBox.Text,
                    Requirements = RequirementTextBox.Text
                };
                ConfigManager.SaveSettings(updatedSettings);
            }
            
            try
            {
                var docView = await VS.Documents.GetActiveDocumentViewAsync();
                if (docView?.FilePath != null && Path.GetExtension(docView.FilePath).Equals(".cs"))
                {
                    await VS.MessageBox.ShowAsync("Selected File", docView.FilePath, buttons: Microsoft.VisualStudio.Shell.Interop.OLEMSGBUTTON.OLEMSGBUTTON_OK, icon: Microsoft.VisualStudio.Shell.Interop.OLEMSGICON.OLEMSGICON_INFO);

                    string result = string.Empty;
                    await LoadingIndicator.ShowLoadingAsync(async () =>
                    {
                        IUnitTestHandler handler = new DotNetClassHandler();
                        result = await handler.GenerateUnitTestAsync(docView.FilePath, FolderPathTextBox.Text, APITextBox.Password, ModelTextBox.Text, RequirementTextBox.Text);
                    });

                    var messageResult = await VS.MessageBox.ShowAsync("Confirmation", result, OLEMSGICON.OLEMSGICON_INFO, OLEMSGBUTTON.OLEMSGBUTTON_OK);

                    if (messageResult == VSConstants.MessageBoxResult.IDOK && result != null && result.Contains("Generated Unit Test File:"))
                    {
                        System.Windows.Clipboard.SetText(result.Replace("Generated Unit Test File:",""));
                        await VS.MessageBox.ShowAsync("Success", "File path is copied to clipboard!", OLEMSGICON.OLEMSGICON_INFO, OLEMSGBUTTON.OLEMSGBUTTON_OK);
                    }
                }
                else
                {
                    await VS.MessageBox.ShowAsync(string.Empty, "Please select a .cs file to generate unit test", buttons: Microsoft.VisualStudio.Shell.Interop.OLEMSGBUTTON.OLEMSGBUTTON_OK, icon: Microsoft.VisualStudio.Shell.Interop.OLEMSGICON.OLEMSGICON_CRITICAL);
                }
            }
            catch (Exception ex)
            {
                LoadingIndicator.Hide();
                await VS.MessageBox.ShowErrorAsync(string.Empty, ex.Message);
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

        private  void DefaultButton_Click(object sender, RoutedEventArgs e)
        {
            RequirementTextBox.Text = DEFAULT_REQUIREMENTS;
        }
        private string Validate()
        {
            const string CHATGPT_API_KEY_REQUIRED = "ChatGPT api key is required.";
            const string CHATGPT_MODEL_REQUIRED = "ChatGPT model is required.";
            const string OUTPUT_FOLDER_REQUIRED = "Output folder for Unittest is required.";
            const string PROMPT_REQUIRED = "Prompt is required";
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
            if (string.IsNullOrEmpty(RequirementTextBox.Text))
            {
                return PROMPT_REQUIRED;
            }

            return string.Empty;
        }
        private void LoadSettings()
        {
            var settings = ConfigManager.LoadSettings();
            if (!string.IsNullOrEmpty(settings.APIKey) || !string.IsNullOrEmpty(settings.Model))
            {
                APITextBox.Password = settings.APIKey;
                ModelTextBox.Text = settings.Model;
                FolderPathTextBox.Text = settings.OutputPath;
                RequirementTextBox.Text = settings.Requirements;
            }
            else
            {
                ModelTextBox.Text = "gpt-4o-mini";
                RequirementTextBox.Text = DEFAULT_REQUIREMENTS;
            }
        }

    }
}