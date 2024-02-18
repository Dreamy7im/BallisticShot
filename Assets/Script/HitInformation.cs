using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitInformation : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Target"))
        {
            Debug.Log("Succesfull Hit!");
        }
    }
}
