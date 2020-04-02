using System;
using System.Collections.Generic;

namespace ServicesPetriNet.Core {
    public class Transition : Node
    {
        public List<Extensions.Link> Links;
        public Type Action;
    }
}
