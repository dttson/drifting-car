using System;

namespace DefaultNamespace
{
    public interface ICarController
    {
        bool IsMyCar { get; }
        void Activate(Action<ICarController> onFinishCallback);
        void DeActivate();
    }
}