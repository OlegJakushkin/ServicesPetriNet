﻿using System;
using System.Collections.Generic;
using Accord.Statistics;
using Accord.Statistics.Distributions;
using Accord.Statistics.Distributions.Univariate;

namespace ServicesPetriNet.Core.Attributes {

    public interface IProbabiletyAttribute
    {
        IDistribution Distribution { get; }
    }

    //Used to query if enablable transition can fire
    //https://github.com/accord-net/framework/wiki/Distributions
    [AttributeUsage(AttributeTargets.Field)]
    public class ProbabiletyAttribute : Attribute, IProbabiletyAttribute 
    {
        public IDistribution Distribution { get; }

        public ProbabiletyAttribute(double Probabilety = 0.5)
        {
            Distribution = new BinomialDistribution(
                2, Probabilety);
        }

        public ProbabiletyAttribute(params int[] values)
        {
            Distribution = new BinomialDistribution();
            Distribution.Fit(values);
        }

    }

    [AttributeUsage(AttributeTargets.Field)]
    public class PrioretyAttribute : Attribute
    {
        public int Priorety { get; }

        public PrioretyAttribute(int Priorety = 999)
        {
            Priorety = Priorety;
        }

    }


    [AttributeUsage(AttributeTargets.Field)]
    public class TimeScaleAttribute : Attribute
    {
        public int Scale { get; }

        public TimeScaleAttribute(int Scale = 1)
        {
            Scale = Scale;
        }

    }
}
