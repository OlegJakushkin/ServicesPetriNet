using System;

namespace ServicesPetriNet.Core.Attributes {
    [AttributeUsage(AttributeTargets.Method)]
    public class ProbabiletyAttribute : Attribute
    {
        public ProbabiletyAttribute(double Probabilety, Type Distribution)
        {
            throw new NotImplementedException();
        }

        public ProbabiletyAttribute(double Probabilety) { throw new NotImplementedException(); }
    }
}
