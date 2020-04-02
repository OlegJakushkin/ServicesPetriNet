using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dynamitey;


namespace ServicesPetriNet
{
    namespace Core
    {
        public class MarkType : IMarkType
        {
            public static MarkType Create<T>(params int[] fields)
            {
                Type t = typeof(T);
                var o = Dynamic.InvokeConstructor(t);
                int argId = 0;
                foreach (var fieldInfo in t.GetFields(
                    BindingFlags.DeclaredOnly |
                    BindingFlags.Public |
                    BindingFlags.Instance
                )) {
                    if (fieldInfo.FieldType == typeof(int)) {
                        if (argId < fields.Length) {
                            Dynamic.InvokeSet(o, fieldInfo.Name, fields[argId++]);
                        } else {
                            break;
                        }
                    } else {
                        throw new
                            Exception(
                                "Mark Type Shall Contain only Int Fields! Only int fields are operated upon in CPNs"
                            );
                    }
                }

                return (MarkType) o;
            }

            public Dictionary<string, int> GetData()
            {
                Dictionary<string, int> allIntegerFields = new Dictionary<string, int>();

                // DeclaredOnly: only get fields declared by this type, not the ones declared by base classes
                // Public | Instance: Only get non-static, public fields
                foreach (FieldInfo fieldInfo in GetType()
                    .GetFields(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)) {
                    if (fieldInfo.FieldType == typeof(int)) {
                        allIntegerFields.Add(fieldInfo.Name, (int) fieldInfo.GetValue(this));
                    } else {
                        throw new
                            Exception(
                                "Mark Type Shall Contain only Int Fields! Only int fields are operated upon in CPNs"
                            );
                    }
                }

                return allIntegerFields;
            }

            public MarkType(IMarkType parent = null)
            {
                Parts = new List<IPart>();
                Parent = parent;
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
