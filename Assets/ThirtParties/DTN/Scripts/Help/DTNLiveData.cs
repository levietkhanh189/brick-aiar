using System;
using System.Collections.Generic;
using UnityEngine;

public class DTNLiveData<T>
{
    private T _value;
    private List<System.Action<T>> callbacks = new List<Action<T>>();
    public string name;
    System.Action<DTNLiveData<T>> _SaveFunction;
    Func<DTNLiveData<T>, T> _GetFunction;

    public DTNLiveData(string name, System.Action<DTNLiveData<T>> SaveFunction, Func<DTNLiveData<T>,T> GetFunction)
    {
        this.name = name;
        _SaveFunction = SaveFunction;
        _GetFunction = GetFunction;
        
    }

    public void Set(T value)
    {
        _value = value;
        if (_SaveFunction != null) _SaveFunction(this);
    }

    public void Set(T value, bool notify)
    {
        Set(value);
        Notify();
    }

    public T Get()
    {
        if (_GetFunction != null)
        {
            _value = _GetFunction(this);
            _GetFunction = null;
        }
        return _value;
    }

    public void Notify()
    {
       Get();
       foreach (System.Action<T> action in callbacks)
        {
            action?.Invoke(_value);
        }
    }

    public System.Action<T> Binding(System.Action<T> callback)
    {
        callbacks.Add(callback);
        callback(Get());
        return callback;
    }

    public void UnBinding(System.Action<T> callback)
    {
        callbacks.Remove(callback);
    }
}
