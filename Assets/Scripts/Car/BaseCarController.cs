using System;
using DefaultNamespace;
using UnityEngine;

public abstract class BaseCarController: MonoBehaviour, ICarController
{
    public abstract bool IsMyCar { get; }
    
    protected Action<ICarController> onFinish;
    protected bool isActivate = false;

    public virtual void Activate(Action<ICarController> onFinishCallback)
    {
        this.onFinish = onFinishCallback;
        isActivate = true;
    }

    public virtual void DeActivate()
    {
        isActivate = false;
    }
    
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("FinishTrigger")) 
            onFinish?.Invoke(this);
    }
}