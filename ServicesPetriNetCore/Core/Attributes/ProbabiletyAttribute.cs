using System;
using Accord.Statistics.Distributions;
using Accord.Statistics.Distributions.Univariate;

namespace ServicesPetriNet.Core.Attributes
{
    public interface IProbabiletyAttribute
    {
        IDistribution Distribution { get; }
    }

    //Used to query if enablable transition can fire
    //https://github.com/accord-net/framework/wiki/Distributions
    [AttributeUsage(AttributeTargets.Field)]
    public class ProbabiletyAttribute : Attribute, IProbabiletyAttribute
    {
        public ProbabiletyAttribute(double Probabilety = 0.5)
        {
            Distribution = new BinomialDistribution(2, Probabilety);
        }

        public ProbabiletyAttribute(params int[] values)
        {
            Distribution = new BinomialDistribution();
            Distribution.Fit(values);
        }

        public IDistribution Distribution { get; }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class PrioretyAttribute : Attribute
    {
        public PrioretyAttribute(int Priorety = 999) { Priorety = Priorety; }

        public int Priorety { get; }
    }


    [AttributeUsage(AttributeTargets.Field)]
    public class TimeScaleAttribute : Attribute
    {
        public TimeScaleAttribute(int Scale = 1) { Scale = Scale; }

        public int Scale { get; }
    }
}
