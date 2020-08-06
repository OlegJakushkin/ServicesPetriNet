using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dynamitey;


namespace ServicesPetriNet
{
    namespace Core
    {
        public static class MarksController
        {
            public static List<MarkType> Marks = new List<MarkType>();

            public static List<MarkType> GetPlaceMarks(Place p)
            {
                return Marks.Where(t =>  t.Host.Equals( p) ).ToList();
            }

            //ToDo: toexpansion
        }

        public class MarkType : IMarkType
        {
            public INode Host { get; set; }

            public static MarkType Create<T>(params object[] fields)
            {
                Type t = typeof(T);
                var o = Dynamic.InvokeConstructor(t); // Activator.CreateInstance(t);

                int argId = 0;
                foreach (var fieldInfo in t.GetFields(
                    BindingFlags.DeclaredOnly |
                    BindingFlags.Public |
                    BindingFlags.Instance
                )) {
                    var isMark = fieldInfo.FieldType == typeof(MarkType);
                    var isInt = fieldInfo.FieldType == typeof(int);;
                    if (!( isMark || isInt)) {
                        throw new Exception("Only nested mark types and int types are allowed");
                    }
                    if (argId < fields.Length) {
                        Dynamic.InvokeSet(o, fieldInfo.Name, fields[argId++]);
                    } else {
                        break;
                    }
                }
                var result = (MarkType)o;
                return result;
            }

            public MarkType(IMarkType parent = null)
            {
                Parts = new List<IPart>();
                Parent = parent;
                MarksController.Marks.Add(this);
            }

            public bool HasParent => Parent != null;
            public IMarkType Parent { get; set; }
            public bool Decomposed { get; set; }

            public bool Decompose(List<IPart> @into)
            {
                if (Decomposed) {
                    return false;
                }

                var counter = 0;
                foreach (var e in @into) {
                    e.Data.Parent = this;
                    e.Number = counter++;
                    e.From = @into.Count;
                }

                Parts.AddRange(@into);
                Decomposed = true;
                return true;
            }

            public (bool, IMarkType) Combine(List<IPart> what)
            {
                if (!Decomposed) {
                    return (false, null);
                }

                var d = @what.GroupBy(p => p.GetType()).ToDictionary(gdc => gdc.Key, gdc => gdc.ToList());

                foreach (var p in d) {
                    if (p.Value.Count > 0) {
                        var totall = p.Value.First().From;
                        var result = p.Value.Count >= totall;
                        if (result) {
                            var lastPartId = 0;
                            var seq = Enumerable.Range(0, totall).ToList();
                            var consistant = seq.All(n => p.Value.Any(part => part.Number == n));
                            if (consistant) {
                                Decomposed = false;
                                return (true, this);
                            }
                        }
                    }
                }

                return (false, null);
            }

            public List<IPart> Parts { get; }
        }
    }
}
