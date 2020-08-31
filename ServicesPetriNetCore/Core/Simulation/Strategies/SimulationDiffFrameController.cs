using System;
using System.IO;
using DiffMatchPatch;
using Newtonsoft.Json;

namespace ServicesPetriNet.Core
{
    public class SimulationDiffFrameController<T> : IFrameController<T>
    {
        private readonly string _path;
        public Frames f;

        public JsonSerializerSettings JsonSettings = new JsonSerializerSettings {
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full,
            TypeNameHandling = TypeNameHandling.Objects,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            PreserveReferencesHandling = PreserveReferencesHandling.All
        };

        public SimulationDiffFrameController(string path, bool preserve = true)
        {
            _path = path;
            if (preserve) {
                var state = File.ReadAllText(path);
                f = JsonConvert.DeserializeObject<Frames>(state);
                if (f == null) throw new Exception("Unreadable file!");
            } else {
                f = new Frames();
                Save();
            }
        }

        public T GetState(int frame = -1)
        {
            var dmp = DiffMatchPatchModule.Default;
            var txt = f.diffs[0];

            var latest = frame == -1;
            if (!latest)
                for (var i = 1; i < frame; i++) {
                    var dstDelta = dmp.DiffFromDelta(txt, f.diffs[f.frames]);
                    txt = dmp.DiffText2(dstDelta);
                }
            else txt = f.LastState;

            return JsonConvert.DeserializeObject<T>(txt, JsonSettings);
        }

        public void SaveState(T TopGroup)
        {
            var ng = JsonConvert.SerializeObject(TopGroup, Formatting.None, JsonSettings);

            if (f.frames == 0) {
                f.diffs.Add(ng);
            } else {
                var dmp = DiffMatchPatchModule.Default;
                var txt = f.LastState;
                var diffs = dmp.DiffMain(txt, ng);
                var srcDelta = dmp.DiffToDelta(diffs);
                f.diffs.Add(srcDelta);
            }

            f.LastState = ng;
            f.frames += 1;
        }

        public void IterateOverFrames(Action<T> act, int tillFrame = -1)
        {
            var dmp = DiffMatchPatchModule.Default;
            var txt = f.diffs[0];
            act(JsonConvert.DeserializeObject<T>(txt, JsonSettings));

            var latest = tillFrame == -1 ? f.frames : tillFrame;
            for (var i = 1; i < latest; i++) {
                var dstDelta = dmp.DiffFromDelta(txt, f.diffs[f.frames]);
                txt = dmp.DiffText2(dstDelta);
                act(JsonConvert.DeserializeObject<T>(txt, JsonSettings));
            }
        }

        public void Save()
        {
            var s = JsonConvert.SerializeObject(f, Formatting.None, JsonSettings);
            File.WriteAllText(_path, s);
        }
    }
}
