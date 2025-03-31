using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIUnittestExtension.ToolWindows.AI
{
    public interface IAIFactory
    {
        IAI GetAI(ModelType aiType);
    }
    public class AIFactory : IAIFactory
    {
        private readonly Dictionary<ModelType, IAI> aiDictionary = null;
        public AIFactory()
        {
            aiDictionary = new Dictionary<ModelType, IAI>
            {
                { ModelType.Gemini, new Gemini() },
                { ModelType.OpenAI, new ChatGPT() }
            };
        }
        public IAI GetAI(ModelType aiType)
        {
            IAI ai = null;
            aiDictionary.TryGetValue(aiType, out ai);
            return ai;
        }
    }
}
