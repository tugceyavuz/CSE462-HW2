using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine.UIElements;
using System;

public class scr_displayAll : MonoBehaviour
{
    public string objectName = "Sphere";
    public void EraseAllObjects()
    {
        GameObject[] objects = GameObject.FindObjectsOfType<GameObject>();

        foreach (GameObject obj in objects)
        {
            if (obj.name == objectName || obj.name == $"{objectName}(Clone)")
            {
                Destroy(obj);
            }
        }
    }
    public string filePath;

    private List<Vector3> pointsP;
    private List<Vector3> pointsQ;
    private List<Vector3> pointsR;
    public GameObject pointPrefab;

    public void OnClick()
    {
        EraseAllObjects();

        LoadPointCloud("File1.txt", out pointsP);
        LoadPointCloud("File2.txt", out pointsQ);
        LoadPointCloud(filePath, out pointsR);

        // Initialize the point clouds in the scene
        InstantiatePointCloud(pointsP, Color.red);
        InstantiatePointCloud(pointsQ, Color.blue);
        InstantiatePointCloud(pointsR, Color.green);

    }

    void LoadPointCloud(string filePath, out List<Vector3> points)
    {
        points = new List<Vector3>();

        try
        {
            string[] lines = File.ReadAllLines(filePath);

            // Check if there is at least one line
            if (lines.Length > 0)
            {
                // Read the number of points from the first line
                if (int.TryParse(lines[0], out int numPoints))
                {
                    Debug.Log(numPoints);
                    // Loop through the lines starting from the second line (index 1)
                    for (int i = 1; i < lines.Length && i <= numPoints; i++)
                    {
                        string[] values = lines[i].Split(' ');

                        if (values.Length >= 3)
                        {
                            float x = float.Parse(values[0]);
                            float y = float.Parse(values[1]);
                            float z = float.Parse(values[2]);
                            points.Add(new Vector3(x, y, z));
                        }
                    }
                }
                else
                {
                    Debug.LogError("Error parsing the number of points from the first line.");
                }
            }
            else
            {
                Debug.LogError("File is empty.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error reading file {filePath}: {e.Message}");
        }
    }

    void InstantiatePointCloud(List<Vector3> points, Color color)
    {
        foreach (Vector3 point in points)
        {
            GameObject pointObj = Instantiate(pointPrefab, point, Quaternion.identity);
            pointObj.GetComponent<Renderer>().material.color = color;
        }
    }
}
