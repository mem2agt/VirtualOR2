using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class ReadCSV : MonoBehaviour
{
    public TextAsset textAssetData;

    [System.Serializable]
    public class Position
    {
        public string bodypart;
        public List<float> xList = new List<float>();
        public List<float> yList = new List<float>();
        public List<float> likelihoodList = new List<float>();
    }

    public Position[] myBodyPartList;

    void Start()
    {
        LoadCSVData();
    }

    void LoadCSVData()
    {
        string[] data = textAssetData.text.Split(new char[] { '\n' });

        // Extract labels from the first row
        string[] rawLabels = data[0].Split(',');
        int numBodyParts = (rawLabels.Length) / 3; // Subtract 1 because the first entry is empty

        // Initialize the array to hold positions
        myBodyPartList = new Position[numBodyParts];

        // Parse each body part label
        for (int i = 0; i < numBodyParts; i++)
        {
            myBodyPartList[i] = new Position();
            myBodyPartList[i].bodypart = rawLabels[i * 3 + 1].Trim(); // Skip the first empty entry
        }

        // Parse x, y, and likelihood for each body part from the third row onwards
        for (int row = 2; row < data.Length; row++)
        {
            string[] coordinates = data[row].Split(',');

            // Parse coordinates and likelihood for each body part
            for (int i = 0; i < numBodyParts; i++)
            {
                int dataIndex = i * 3;
                myBodyPartList[i].xList.Add(float.Parse(coordinates[dataIndex]));
                myBodyPartList[i].yList.Add(float.Parse(coordinates[dataIndex + 1]));
                myBodyPartList[i].likelihoodList.Add(float.Parse(coordinates[dataIndex + 2]));
            }
        }
    }
}