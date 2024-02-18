using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TargetSetup : MonoBehaviour
{
    [SerializeField] private float MinDistance;
    [SerializeField] private float MaxDistance;

    [SerializeField] private float Distance;

    private void Start()
    {
        Distance = Random.Range(MinDistance, MaxDistance);
        transform.position = new Vector3(Distance, 0, 0);
    }
}
