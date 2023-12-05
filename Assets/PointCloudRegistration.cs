using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

public class PointCloudRegistration : MonoBehaviour
{
    public GameObject pointPrefab; // Reference to the point cloud prefab
    public Text resultText; // Text to display transformation parameters

    private List<Vector3> pointsP;
    private List<Vector3> pointsQ;
    private bool useRigidTransformation = true;

    void Start()
    {
        // Load point clouds from files (you need to set the file paths)
        LoadPointCloud("File1.txt", out pointsP);
        LoadPointCloud("File2.txt", out pointsQ);

        // Initialize the point clouds in the scene
        InstantiatePointCloud(pointsP, Color.red);
        InstantiatePointCloud(pointsQ, Color.blue);

        // Set up UI buttons
        Button rigidButton = GameObject.Find("RigidButton").GetComponent<Button>();
        rigidButton.onClick.AddListener(() => SetRegistrationMethod(true));

        Button scaleButton = GameObject.Find("ScaleButton").GetComponent<Button>();
        scaleButton.onClick.AddListener(() => SetRegistrationMethod(false));

        Button originalButton = GameObject.Find("OriginalButton").GetComponent<Button>();
        originalButton.onClick.AddListener(() => ShowOriginalAndAlignedPoints());

        Button transformedButton = GameObject.Find("TransformedButton").GetComponent<Button>();
        transformedButton.onClick.AddListener(() => ShowTransformedPoints());

        // Initial registration using rigid transformation
        RegisterPointClouds();
    }

    void LoadPointCloud(string filePath, out List<Vector3> points)
    {
         points = new List<Vector3>();

        try
        {
            string[] lines = File.ReadAllLines(filePath);

            // Skip the first line (num_pts)
            for (int i = 1; i < lines.Length; i++)
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

    void SetRegistrationMethod(bool useRigid)
    {
        useRigidTransformation = useRigid;
        RegisterPointClouds();
    }

    void ShowOriginalAndAlignedPoints()
    {
        ClearPointClouds();
        InstantiatePointCloud(pointsP, Color.red);
        InstantiatePointCloud(pointsQ, Color.blue);
        RegisterPointClouds();
    }

    void ShowTransformedPoints()
    {
        ClearPointClouds();
        InstantiatePointCloud(pointsQ, Color.blue);
        RegisterPointClouds();
    }

    void ClearPointClouds()
    {
        GameObject[] pointCloudObjects = GameObject.FindGameObjectsWithTag("PointCloud");
        foreach (GameObject pointCloudObject in pointCloudObjects)
        {
            Destroy(pointCloudObject);
        }
    }

    void RegisterPointClouds()
    {
        // Perform registration using selected method (rigid or rigid with scale)
        // Implement your registration logic here

        // For demonstration purposes, let's just show a message in the result text
        string registrationMethod = useRigidTransformation ? "Rigid" : "Rigid with Scale";
        resultText.text = $"Registration Method: {registrationMethod}\nReconstructed Transformation Parameters: ...\nScale Parameters: ...";
    }
}
