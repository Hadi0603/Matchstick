using System;
using UnityEngine;

public class SpotOccupied : MonoBehaviour
{
    [HideInInspector]
    public bool isOccupied = false;

    private void Start()
    {
        isOccupied = transform.childCount != 0;
    }
}