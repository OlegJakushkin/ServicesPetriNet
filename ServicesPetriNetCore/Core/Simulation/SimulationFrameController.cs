using System;
using System.Collections.Generic;
using System.IO;
using DiffMatchPatch;
using Newtonsoft.Json;

namespace ServicesPetriNet.Core
{
    public class SimulationFrameController
    {
        private readonly string _path;

        public Frames f;

        public SimulationFrameController(string path)
        {
            _path = path;
            try {
                var state = File.ReadAllText(path);
                f = JsonConvert.DeserializeObject<Frames>(state);
            } catch (Exception e) {
                var s = JsonConvert.SerializeObject(f, Formatting.None);
                File.WriteAllText(_path, s);
            }
        }

        public Group GetState(int frame = -1)
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

            return JsonConvert.DeserializeObject<Group>(txt);
        }

        public void SaveState(Group TopGroup)
        {
            var ng = JsonConvert.SerializeObject(TopGroup, Formatting.None);

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

        public void IterateOverFrames(Action<Group> act, int tillFrame = -1)
        {
            var dmp = DiffMatchPatchModule.Default;
            var txt = f.diffs[0];
            act(JsonConvert.DeserializeObject<Group>(txt));

            var latest = tillFrame == -1 ? f.frames : tillFrame;
            for (var i = 1; i < latest; i++) {
                var dstDelta = dmp.DiffFromDelta(txt, f.diffs[f.frames]);
                txt = dmp.DiffText2(dstDelta);
                act(JsonConvert.DeserializeObject<Group>(txt));
            }
        }

        public void Save()
        {
            var s = JsonConvert.SerializeObject(f, Formatting.None);
            File.WriteAllText(_path, s);
        }

        public class Frames
        {
            public List<string> diffs = new List<string>();
            public int frames;
            public string LastState = "";
        }
    }
}
