using System;
using Accord.Statistics.Distributions;
using Accord.Statistics.Distributions.Univariate;
using Newtonsoft.Json;

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

        [JsonIgnore]
        public IDistribution Distribution { get; }
    }
}
