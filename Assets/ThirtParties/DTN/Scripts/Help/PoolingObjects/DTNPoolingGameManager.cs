using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DTNPoolingGameManager : DTNMono
{
    public static DTNPoolingGameManager Instance;
    public Hashtable CategoryList = new Hashtable();
    public HashSet<DTNPoolingGameItem> hashSet = new HashSet<DTNPoolingGameItem>();
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
            
    }

    public GameObject GenerateObject(GameObject template, Transform transform,float destroy)
    {
        int key = template.GetHashCode();
        DTNPoolingGameItem nCom = null;
        if (_IsEmpty(key))
        {
            GameObject nGameObject = Instantiate(template, Vector3.zero, Quaternion.identity, transform);
            nCom = nGameObject.AddComponent<DTNPoolingGameItem>();
        }
        else
        {
            nCom = _Get(key);
        }
        nCom.isUsing = true;
        nCom.key = key;
        nCom.transform.parent = transform;
        nCom.transform.localPosition = Vector3.zero;
        nCom.transform.localRotation = Quaternion.identity;
        nCom.gameObject.SetActive(true);
        DestroyObject(nCom.gameObject, destroy);
        return nCom.gameObject;
    }

    public GameObject GenerateObject(GameObject template, Transform transform)
    {
        int key = template.GetHashCode();
        DTNPoolingGameItem nCom = null;
        if (_IsEmpty(key))
        {
            GameObject nGameObject = Instantiate(template, Vector3.zero, Quaternion.identity , transform);
            nCom = nGameObject.AddComponent<DTNPoolingGameItem>();
        }
        else
        {
            nCom = _Get(key);
        }
        nCom.isUsing = true;
        nCom.key = key;
        nCom.transform.parent = transform;
        nCom.transform.localPosition = Vector3.zero;
        nCom.transform.localRotation = Quaternion.identity;
        nCom.gameObject.SetActive(true);
        return nCom.gameObject;
    }
    public GameObject GenerateObject(GameObject template,Vector3 pos,Quaternion rot, Transform transform)
    {
        int key = template.GetHashCode();
        DTNPoolingGameItem nCom = null;
        if (_IsEmpty(key))
        {
            GameObject nGameObject = Instantiate(template, pos, rot, transform);
            nCom = nGameObject.AddComponent<DTNPoolingGameItem>();
        }
        else
        {
            nCom = _Get(key);
        }
        nCom.isUsing = true;
        nCom.key = key;
        nCom.transform.parent = transform;
        nCom.gameObject.SetActive(true);
        return nCom.gameObject;
    }

    public GameObject GenerateObject(GameObject template, Vector3 pos, Quaternion rot)
    {
        int key = template.GetHashCode();
        DTNPoolingGameItem nCom = null;
        if (_IsEmpty(key))
        {
            GameObject nGameObject = Instantiate(template, pos, rot, transform);
            nCom = nGameObject.AddComponent<DTNPoolingGameItem>();
        }
        else
        {
            nCom = _Get(key);
        }
        nCom.transform.position = pos;
        nCom.isUsing = true;
        nCom.key = key;
        nCom.gameObject.SetActive(true);
        return nCom.gameObject;
    }

    public void DestroyObject(GameObject gameObject, float after)
    {
        //DoItAfter(after, () =>
        //{
        //    DestroyObject(gameObject);
        //});
        DTNPoolingGameItem item = gameObject.GetComponent<DTNPoolingGameItem>();
        item.DestroyAfter(after);
    }

    public void DestroyObject(GameObject gameObject)
    {
        DTNPoolingGameItem item = gameObject.GetComponent<DTNPoolingGameItem>();

        if (item != null)
        {
            DestroyObject(item);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void DestroyObject(DTNPoolingGameItem gameObject, bool allChild = true)
    {
        if (allChild == true)
        {
            gameObject.DestroyAllChildIfNeed();
        }
        _Add(gameObject.key, gameObject);
    }

    private void _Add(int hashCode, DTNPoolingGameItem gameObject)
    {
        LinkedList<DTNPoolingGameItem> list = null;
        string key = "" + hashCode;
        if (CategoryList.ContainsKey(key))
        {
            list = (LinkedList<DTNPoolingGameItem>)CategoryList["" + hashCode];
        }
        else
        {
            list = new LinkedList<DTNPoolingGameItem>();
            CategoryList.Add(key, list);
        }
        if (hashSet.Contains(gameObject)) return;
        list.AddLast(gameObject);
        hashSet.Add(gameObject);
        gameObject.transform.parent = null;
        gameObject.isUsing = false;
        gameObject.gameObject.SetActive(false);
        
        gameObject.transform.parent = transform;
    }

    private bool _IsEmpty(int hashCode)
    {
        //List<DTNPoolingGameItem> list = null;
        string key = "" + hashCode;
        if (CategoryList.ContainsKey(key))
        {
            LinkedList<DTNPoolingGameItem> list = (LinkedList<DTNPoolingGameItem>)CategoryList["" + hashCode];
            return list.Count == 0;
        }
        else
        {
            return true;
        }
    }

    public DTNPoolingGameItem _Get(int hashCode)
    {
        
        string key = "" + hashCode;
        if (CategoryList.ContainsKey(key))
        {
            LinkedList<DTNPoolingGameItem> list = (LinkedList<DTNPoolingGameItem>)CategoryList["" + hashCode];
            DTNPoolingGameItem result = list.First.Value;
            list.RemoveFirst();
            hashSet.Remove(result);
            result.isUsing = true;
            result.isWaitingForDestroy = false;
            return result;
        }
        else
        {
            return null;
        }
    }
}
