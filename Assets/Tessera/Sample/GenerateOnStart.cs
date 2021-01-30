using System.Collections;
using System.Collections.Generic;
using Tessera;
using UnityEngine;

// This script simply kicks off the generator.
// By default the generator doesn't do anything, which is not useful for a sample
public class GenerateOnStart : MonoBehaviour
{
    void Start()
    {
        var generator = GetComponent<TesseraGenerator>();

        generator.Generate();
    }
}
