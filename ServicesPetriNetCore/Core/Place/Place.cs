using System.Linq;
using Newtonsoft.Json;

namespace ServicesPetriNet.Core
{
    public class Place : INode
    {
        [JsonIgnore]
        public string Name
        {
            get
            {
                return From.Descriptor.Places.First(pair => pair.Value.Value == this).Key;
            }
        }

        [JsonIgnore]
        public Group From { get; set; }
    }
}
