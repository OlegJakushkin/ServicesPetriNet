using Newtonsoft.Json;

namespace ServicesPetriNet.Core
{
    public class Place : INode
    {
        [JsonIgnore]
        public Group From { get; set; }
    }
}
