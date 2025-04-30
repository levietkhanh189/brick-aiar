using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class DTNInitView : MonoBehaviour
{
    public static DTNInitView _instance;
    public DTNViewInfoSystem ViewInfoSystem;

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        
    }

    public DTNView Init(DTNView view, Transform parent = null)
    {
        return Init(view.GetType(), parent);
    }

    public DTNView Init<T>(Transform parent = null)
    {
        return Init(typeof(T), parent);
    }

    public DTNView Init(System.Type type, Transform parent = null)
    {

        return Init(type.FullName, parent);
    }

    public DTNView Init(string nameView, Transform parent = null)
    {
        GameObject cloneView = DTNPoolingGameManager.Instance.GenerateObject(Resources.Load(ViewInfoSystem.GetStringAddress(nameView)) as GameObject, parent);
        cloneView.SetActive(false);

        return cloneView.GetComponent<DTNView>();
    }
}