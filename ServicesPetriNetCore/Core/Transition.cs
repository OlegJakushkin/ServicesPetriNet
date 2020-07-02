using System;
using System.Collections.Generic;

namespace ServicesPetriNet.Core {
    public struct Transition : INode
    {
        public List<Link> Links;
        public Type Action;
    }
}
