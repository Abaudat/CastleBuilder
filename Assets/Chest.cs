using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour
{
    private PlayManager playManager;

    private void Awake()
    {
        playManager = FindObjectOfType<PlayManager>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            playManager.Success();
        }
    }
}
