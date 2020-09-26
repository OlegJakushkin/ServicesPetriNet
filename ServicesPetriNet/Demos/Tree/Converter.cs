using System;
using System.Collections.Generic;
using ServicesPetriNet.Core;

namespace ServicesPetriNet
{
    public class Converter<T> : ActionBase
        where T : MarkType, new()
    {
        public static Type From => typeof(T);

        public virtual List<Package> Action(T input)
        {
            var result = new List<Package> {
                new Package {
                    Number = 0,
                    Size = 1
                }
            };
            Host.From.Decompose(input, result.AsParts());
            return result;
        }
    }
}