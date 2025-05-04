using System;

namespace DefaultNamespace
{
    public interface ICarController
    {
        string CarName { get; }
        bool IsMyCar { get; }
        void Activate(Action<ICarController> onFinishCallback);
        void DeActivate();
    }
}