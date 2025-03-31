using AIUnittestExtension.ToolWindows;
using AIUnittestExtension.ToolWindows.AI;
using Microsoft.VisualStudio.Shell.Interop;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace AIUnittestExtension
{
    public partial class MyToolWindowControl : System.Windows.Controls.UserControl
    {
        private const string DEFAULT_REQUIREMENTS =
            $"- Use xUnit and Moq for testing." +
            $"\n- **Always include `[Fact]`** before each test method." +
            $"\n- Ensure that condition factories, dictionary lookups, and in-memory logic **are not mocked**." +
            $"\n- Include both positive and negative test cases to verify expected behavior." +
            $"\n- Ensure proper assertions and exception handling." +
            $"\n- Ensure exception handling not for MaxValue and MinValue case" +
            $"\n- Do **not** include the namespace or class definition in the output." +
            $"\n- **Do not** generate test cases involving exceeding MinValue or MaxValue (e.g., MaxValue + 1, MinValue - 1)." +
            $"\n- Return **only** valid C# test methods, without enclosing them in a class." +
            $"\n- **Do not include** Markdown formatting (e.g., ` ```csharp` and ` ``` `)." +
            "\n- Use `{className}` when calling the method under test." +
            "\n- **Do not add extra properties to entities that are not used in `{methodCode}`.**" +
            "\n- If a database entity is returned, do not add fields like `Street`, `City`, etc., unless they are present in `{methodCode}`." +
            "\n- **Mock should only be used for methods inside it—not for `{className}.methodName` itself.**" +
            "\n- Ensure that mock dependencies are injected via the constructor in the format:\nvar TestClass = new TestClass(mockTest.Object);" +
            "\n- You must carefully analyze the method code to identify the exact types of all dependencies being used and create mocks with the correct interface types. (e.g: _temp => interface ITemp, _term => ITerm)" +
            "\n- YOUR RESPONSE MUST CONTAIN ONLY THE TEST METHODS THEMSELVES, with no introductory text, explanations, or code fence markers";
        private int _selectedAI = 0;

        #region CONSTRUCTOR
        public MyToolWindowControl()
        {
            InitializeComponent();
            LoadSettings();
        }
        #endregion

        #region EVENT HANDLERS
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
                SaveSettings();
            }

            await GenerateUnitTestDocumentAsync();
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

        private void AIComboBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (AIComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                int value = int.Parse(selectedItem.Tag.ToString());

                _selectedAI = value;

                if (APITextBox != null)
                {
                    LoadSetting(value);
                }            
            }
        }

        #endregion

        #region PRIVATE METHODS
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
        private void SaveSettings()
        {
            if (AIComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                var settings = ConfigManager.LoadSettings();
                int selectedAIValue = int.Parse(selectedItem.Tag.ToString());

                var updatedAISetting = new AISetting { APIKey = APITextBox.Password, Model = ModelTextBox.Text };
                if (!settings.AISetttings.Keys.Contains((ModelType)selectedAIValue))
                {
                    settings.AISetttings.Add((ModelType)selectedAIValue, updatedAISetting);
                }
                else
                {
                    settings.AISetttings[(ModelType)selectedAIValue] = updatedAISetting;
                }

                settings.OutputPath = FolderPathTextBox.Text;
                settings.Requirements = RequirementTextBox.Text;

                ConfigManager.SaveSettings(settings);
            }
        }
        private void LoadSettings()
        {
            var settings = ConfigManager.LoadSettings();
            if (settings.AISetttings.Count == 0)
            {
                //Load Default
                settings.AISetttings.Add(ModelType.Gemini , new AISetting() { Model = "gemini-2.0-flash-lite" });   
                ModelTextBox.Text = "gemini-2.0-flash-lite";
                RequirementTextBox.Text = DEFAULT_REQUIREMENTS;
                return;
            }
            
            if (AIComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                int value = int.Parse(selectedItem.Tag.ToString());
                LoadSetting(value);
            }

            FolderPathTextBox.Text = settings.OutputPath;
            RequirementTextBox.Text = settings.Requirements;
        }

        private void LoadSetting(int key)
        {
            var settings = ConfigManager.LoadSettings();
            AISetting aiSetting = null;
            switch (key)
            {
                case (int)ModelType.Gemini:
                    aiSetting = settings.AISetttings.Keys.Contains(ModelType.Gemini) ? settings.AISetttings[ModelType.Gemini] : null;
                    APITextBox.Password = aiSetting?.APIKey;
                    ModelTextBox.Text = aiSetting?.Model ?? "gemini-2.0-flash-lite";
                    break;
                case (int)ModelType.OpenAI:
                    aiSetting = settings.AISetttings.Keys.Contains(ModelType.OpenAI) ? settings.AISetttings[ModelType.OpenAI] : null;
                    APITextBox.Password = aiSetting?.APIKey;
                    ModelTextBox.Text = aiSetting?.Model ?? "gpt-4o-mini";
                    break;
                case (int)ModelType.Claude:
                    aiSetting = settings.AISetttings.Keys.Contains(ModelType.Claude) ? settings.AISetttings[ModelType.Claude] : null;
                    APITextBox.Password = aiSetting?.APIKey;
                    ModelTextBox.Text = aiSetting?.Model ?? "claude-3-haiku-20240307";
                    break;
            }
        }
        #endregion

        private async Task GenerateUnitTestDocumentAsync()
        {
            try
            {
                var docView = await VS.Documents.GetActiveDocumentViewAsync();
                if (docView?.FilePath != null && Path.GetExtension(docView.FilePath).Equals(".cs"))
                {
                    await VS.MessageBox.ShowAsync("Selected File", docView.FilePath, buttons: Microsoft.VisualStudio.Shell.Interop.OLEMSGBUTTON.OLEMSGBUTTON_OK, icon: Microsoft.VisualStudio.Shell.Interop.OLEMSGICON.OLEMSGICON_INFO);

                    string result = string.Empty;
                    await LoadingIndicator.ShowLoadingAsync(async () =>
                    {
                        IAIFactory aiFactory = new AIFactory();
                        IUnitTestHandler handler = new DotNetClassHandler(aiFactory.GetAI((ModelType)_selectedAI));
                        result = await handler.GenerateUnitTestAsync(docView.FilePath, FolderPathTextBox.Text, APITextBox.Password, ModelTextBox.Text, RequirementTextBox.Text);
                    });

                    var isSuccess = result.Contains("Generated Unit Test File:");
                    var confirmMessage = result + (isSuccess ? "\nNote: File path is copied to clipboard!" : string.Empty);
                    await VS.MessageBox.ShowAsync("Confirmation", confirmMessage, OLEMSGICON.OLEMSGICON_INFO, OLEMSGBUTTON.OLEMSGBUTTON_OK);
                    if (isSuccess)
                    {
                        System.Windows.Clipboard.SetText(result.Replace("Generated Unit Test File:", ""));
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
    }
}