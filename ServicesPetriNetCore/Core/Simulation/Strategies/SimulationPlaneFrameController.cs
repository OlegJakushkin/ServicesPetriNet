using System;
using System.IO;
using DiffMatchPatch;
using Newtonsoft.Json;

namespace ServicesPetriNet.Core
{
    public class SimulationPlaneFrameController<T> : IFrameController<T>
    {
        private readonly string _path;
        public Frames f;

        public JsonSerializerSettings JsonSettings = new JsonSerializerSettings {
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full,
            TypeNameHandling = TypeNameHandling.Objects,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            PreserveReferencesHandling = PreserveReferencesHandling.All,
        };

        public SimulationPlaneFrameController(string path, bool preserve = true)
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
            string txt;

            var latest = frame == -1;
            if (!latest)
                txt = f.diffs[frame];
            else txt = f.LastState;

            return JsonConvert.DeserializeObject<T>(txt, JsonSettings);
        }

        public void SaveState(T TopGroup)
        {
            var ng = JsonConvert.SerializeObject(TopGroup, Formatting.None, JsonSettings);

            f.diffs.Add(ng);
            f.LastState = ng;
            f.frames += 1;
        }

        public void IterateOverFrames(Action<T> act, int tillFrame = -1)
        {
            var latest = tillFrame == -1 ? f.frames : tillFrame;
            for (var i = 0; i < latest; i++) {
                act(JsonConvert.DeserializeObject<T>(f.diffs[i], JsonSettings));
            }
        }

        public void Save()
        {
            var s = JsonConvert.SerializeObject(f, Formatting.None, JsonSettings);
            File.WriteAllText(_path, s);
        }
    }
}
