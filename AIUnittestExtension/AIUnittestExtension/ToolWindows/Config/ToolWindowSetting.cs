using System.Collections.Generic;

namespace AIUnittestExtension
{
    public class ToolWindowSetting
    {
        public Dictionary<ModelType, AISetting> AISetttings { get; set; } = new Dictionary<ModelType, AISetting>();
        public string OutputPath { get; set; }
        public string Requirements { get; set; }
    }

    public class AISetting
    {
        public string APIKey { get; set; }
        public string Model { get; set; }
    }

    public enum ModelType
    {
        Gemini = 1,
        OpenAI = 2,
        
    }
}
