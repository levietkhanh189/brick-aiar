using UnityEngine;
using UnityEngine.UI;
using Battlehub.RTHandles;
using System.Collections.Generic;
public class LegoPanel : MonoBehaviour
{
    public PrefabSpawnPoint spawnPoint;
    public List<PrefabSpawnPoint> legoSpawnPoints;
    public void ShowAllLego(List<GameObject> legoItems)
    {
        foreach (var legoItem in legoItems)
        {
            var legoSpawnPoint = Instantiate(spawnPoint.gameObject, transform).GetComponent<PrefabSpawnPoint>();
            legoSpawnPoint.SetPrefab(legoItem);
            legoItem.transform.gameObject.SetActive(false);
            legoSpawnPoints.Add(legoSpawnPoint);
        }
    }
}
