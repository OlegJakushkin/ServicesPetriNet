using Newtonsoft.Json;
using ServicesPetriNet.Core;

namespace ServicesPetriNet
{
    public interface IAction
    {
        Transition Host { get; set; }
    }

    public class ActionBase : IAction
    {
        [JsonIgnore]
        public Transition Host { get; set; }
    }
}
