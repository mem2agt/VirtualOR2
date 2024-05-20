using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ReadRotation : MonoBehaviour
{
    public TextAsset textAssetData;

    [System.Serializable]
    public class Rotation
    {
        public float real;
        public float i;
        public float j;
        public float k;
    }

    public Rotation[] myQuatList;

    void Start()
    {
        LoadQuatData();
    }

    void LoadQuatData()
    {
        string[] data = textAssetData.text.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

        // Extract labels from the first row
        string[] labels = data[0].Split(',');

        // Initialize the array to hold rotations
        myQuatList = new Rotation[data.Length - 1];

        // Parse real, i, j, and k 
        for (int row = 1; row < data.Length; row++)
        {
            string[] coordinates = data[row].Split(',');

            // Initialize a new Rotation object for this row
            Rotation rotation = new Rotation();

            // Parse coordinates and assign to Rotation object
            rotation.real = float.Parse(coordinates[0]);
            rotation.i = float.Parse(coordinates[1]);
            rotation.j = float.Parse(coordinates[2]);
            rotation.k = float.Parse(coordinates[3]);

            // Assign Rotation object to the array
            myQuatList[row - 1] = rotation;
        }
    }
}
