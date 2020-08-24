using System;
using System.Collections.Generic;
using Fractions;
using Newtonsoft.Json;

namespace ServicesPetriNet.Core
{
    // Transition has Links that help to:
    // - check executabilety
    // - Colloect input data
    // Transition has an Action type with an Action function,
    // Action function a structure like this:
    // - public Tout Action(Tin ps)
    // - public void  Action(T1in ps, ..., TNin ps, out T1out,..., out TNout)
    public class Transition : INode
    {
        public Type Action;
        public List<Link> Links = new List<Link>();
        public Fraction TimeScale = 1;
        [JsonIgnore]
        public Group From { get; set; }
    }
}
