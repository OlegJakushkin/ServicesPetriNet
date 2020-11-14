using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using ServicesPetriNet.Core;

namespace ServicesPetriNet
{
    public interface IAction
    {
        Transition Host { get; set; }
        Dictionary<Place, List<MarkType>> Outputs { get; set; }
        Dictionary<LinkKey, List<MarkType>> Inputs { get; set; }
        void ActionCaller();
    }

    public class ActionBase : IAction
    {
        [JsonIgnore]
        public Transition Host { get; set; }
        [JsonIgnore]
        public Dictionary<Place, List<MarkType>> Outputs { get; set; }
        [JsonIgnore]
        public Dictionary<LinkKey, List<MarkType>> Inputs { get; set; }

        //This is the main action invoker
        //If you want to work with Action Inputs and Outputs directly, please overload it
        //It is a reflection based method calling your Action(params) method implementation
        public virtual void ActionCaller()
        {
            var method = Host.Action.GetMethod(nameof(Action));

            var named = Inputs.Where((pair, i) => pair.Key.LinkName != "")
                .ToList();

            //Fill all named paramerters
            var ps = method.GetParameters().Select(
                param =>
                {
                    object result = param;
                    if (Inputs.Any(pair => pair.Key.LinkName == param.Name))
                    {
                        if (param.IsOut) throw new Exception("Link LinkName shall be of action input parameter ");
                        var k = Inputs.First(pair => pair.Key.LinkName == param.Name);

                        if (param.ParameterType.IsList())
                        {
                            result = k.Value;
                        }
                        else
                        {
                            result = k.Value.First();
                            k.Value.RemoveAt(0);
                        }

                        Inputs.Remove(k.Key);
                    }

                    return result;
                }
            ).ToArray();

            //Fill all other paramerters
            for (var index = 0; index < ps.Length; index++)
            {
                var o = ps[index];
                if (o is ParameterInfo)
                {
                    var param = (ParameterInfo)o;
                    if (param.IsOut)
                    {
                        ps[index] = null;
                        continue;
                    }

                    if (param.ParameterType.IsList())
                    {
                        var key = new LinkKey
                        {
                            LinkName = "",
                            MarkType = param.ParameterType
                        };
                        var value = Inputs.TryGetValue(key, out var pValue)
                            ? pValue
                            : throw new InvalidOperationException(
                                $"Parameter of type {param.ParameterType.Name} was not found"
                            );
                        var elementType = param.ParameterType.GenericTypeArguments.First();

                        var lo = value.ReflectionCast(elementType);
                        //var values = value.Select(x => Dynamic.InvokeConvert(x, elementType, true)).ToList();
                        ps[index] = lo;
                        Inputs.Remove(key);
                    }
                    else
                    {
                        var listType = typeof(List<>);
                        var constructedListType = listType.MakeGenericType(param.ParameterType);
                        var key = new LinkKey
                        {
                            LinkName = "",
                            MarkType = constructedListType
                        };
                        var results = Inputs.TryGetValue(key, out var pValue)
                            ? pValue
                            : throw new InvalidOperationException(
                                $"Parameter of type {param.ParameterType.Name} was not found"
                            );
                        ps[index] = results.First();
                        results.RemoveAt(0);
                        if (results.Count == 0) Inputs.Remove(key);
                    }
                }

            }

            var rp = method.Invoke(this, ps);

            var outs = new Dictionary<Type, List<MarkType>>();
            Action<object> release = variable =>
            {
                var marks = new List<MarkType>();
                var isArray = variable.GetType().IsList();
                Type type;
                if (isArray) type = (variable as IEnumerable).AsQueryable().ElementType;
                else type = variable.GetType();

                if (outs.TryGetValue(type, out var value) &&
                    value != null) marks = value;

                if (variable.GetType().IsList()) marks.AddRange((variable as IEnumerable).Cast<MarkType>().ToList());
                else marks.Add((MarkType)variable);

                outs[type] = marks;
            };

            for (var index = 0; index < method.GetParameters().Length; index++)
            {
                var o = method.GetParameters()[index];
                if (o.IsOut)
                {
                    var variable = ps[index];
                    release(variable);
                }
            }

            if (method.ReturnType != typeof(void)) release(rp);

            Host.Links.Where(l => l.To.GetType() == typeof(Place)).ToList().ForEach(
                l =>
                {
                    var marks = new List<MarkType>();
                    if (outs.TryGetValue(l.What, out var value) &&
                        value != null) marks = value;
                    else throw new Exception("Transitions Action had to return Tout: " + l.What);
                    var key = (Place)l.To; // Use smart place key??? // TODO move key params into place, forget about place keyu!

                    switch (l.CountStrategy)
                    {
                        case Link.Count.One:
                            {
                                var m = marks.First();
                                //m.Host = key;
                                marks.Remove(m);

                                if (Outputs.TryGetValue(key, out var added))
                                {
                                    added.Add(m);
                                }
                                else
                                {
                                    added = new List<MarkType> {
                                    m
                                };
                                    Outputs.Add(key, added);
                                }

                                break;
                            }
                        case Link.Count.All:
                            {
                                //marks.MoveMarksTo(key);

                                if (Outputs.TryGetValue(key, out var added)) added.AddRange(marks);
                                else Outputs.Add(key, marks);


                                marks.Clear();

                                break;
                            }
                        case Link.Count.Some:
                            {
                                var ms = marks.Take(l.CountStrategyAmmount).ToList();
                                //ms.MoveMarksTo(l.To);

                                if (Outputs.TryGetValue(key, out var added)) added.AddRange(ms);
                                else Outputs.Add(key, ms);
                                marks.RemoveAll(m => ms.Contains(m));
                                break;
                            }
                        case Link.Count.None:
                            {
                                throw new Exception("Error: A Transition link to outgoing place shall not be null");
                            }
                        default: throw new ArgumentOutOfRangeException();
                    }
                }
            );
        }

        public void PerformAction()
        {
        }
    }
}
