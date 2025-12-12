using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlatesCounterVisual : MonoBehaviour
{
    
    
    [SerializeField] private Transform CounterTopPoint;
    [SerializeField] private Transform PlateVisualPrefab;
    [SerializeField] private PlatesCounter PlatesCounter;
    private List<GameObject> plateVisualGameObjectList;
    private void Awake()
    {
        plateVisualGameObjectList = new List<GameObject>();
    }

    private void Start()
    {
        PlatesCounter.OnPlateSpawned += PlatesCounter_OnPlatesSpawned;
        PlatesCounter.OnPlateRemoved += PlatesCounter_OnPlatesRemoved;
    }
    private void PlatesCounter_OnPlatesSpawned(object sender, System.EventArgs e)
    {
        Transform plateVisualTrasform = Instantiate(PlateVisualPrefab, CounterTopPoint);
        float plateOffsetZ = 0.3f;
        plateVisualTrasform.localPosition = new Vector3(0, plateOffsetZ * plateVisualGameObjectList.Count, 0);
        plateVisualGameObjectList.Add(plateVisualTrasform.gameObject);
    }
    private void PlatesCounter_OnPlatesRemoved(object sender, System.EventArgs e)
    {
        GameObject plateGameObject = plateVisualGameObjectList[plateVisualGameObjectList.Count - 1];
        plateVisualGameObjectList.Remove(plateGameObject);
        Destroy(plateGameObject);
    }
}
