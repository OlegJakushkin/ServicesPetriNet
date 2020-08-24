using Newtonsoft.Json;

namespace ServicesPetriNet.Core
{
    public interface INode
    {
        [JsonIgnore]
        Group From { get; set; }
    }
}
