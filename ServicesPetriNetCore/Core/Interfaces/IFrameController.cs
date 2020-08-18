using System;

namespace ServicesPetriNet.Core
{
    public interface IFrameController<T>
    {
        void Save();
        void IterateOverFrames(Action<T> act, int tillFrame = -1);
        void SaveState(T TopGroup);
        T GetState(int frame = -1);
    }
}
