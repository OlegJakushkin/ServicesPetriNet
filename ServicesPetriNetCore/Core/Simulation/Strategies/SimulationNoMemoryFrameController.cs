using System;
using System.IO;
using Newtonsoft.Json;

namespace ServicesPetriNet.Core
{
    public class SimulationNoMemoryFrameController<T> : IFrameController<T>
    {
        private T TopGroup;

        public T GetState(int frame = -1)
        {
            if (frame == -1) {
                return TopGroup;
            } 
            throw new Exception("Not implemented in this FrameController, please use Diff or Plain ones");
        }

        public void SaveState(T TopGroup)
        {
            this.TopGroup = TopGroup;
        }

        public void IterateOverFrames(Action<T> act, int tillFrame = -1)
        {
            throw new Exception("Not implemented in this FrameController, please use Diff or Plain ones");
        }

        public void Save()
        {
            throw new Exception("Not implemented in this FrameController, please use Diff or Plain ones");
        }
    }
}
