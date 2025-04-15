using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using TMPro;
using UnityEngine;

public class DataProcessing : MonoBehaviour
{

    private StreamReader reader;
    private Vector3 testingDiverPos = new Vector3(212.271439f, 119.490868f, 219.693787f);
    private Vector3 diver1Pos = new Vector3(281.881012f, 36.8699989f, 294.869995f);
    private Vector3 diver2Pos = new Vector3(87.8600006f, 23.5f, 94.5f);
    private Vector3 diver3Pos = new Vector3(50.2999992f, 40.2999992f, 442.5f);
    private Vector3 diver4Pos = new Vector3(443.540009f, 39.0499992f, 90.5899963f);
    private Vector3 diver5Pos = new Vector3(427.019989f, 60.6500015f, 438.25f);
    private Vector3 basePos = new Vector3(207.893082f, 131.025192f, 187.043793f);

    // Start is called before the first frame update
    void Start()
    {
        Dictionary<string, List<int>> ordering = new();
        ordering["11-02-2025_10-24-56.csv"] = new List<int>() { 0, 1, 5, 6 };
        ordering["11-02-2025_11-43-22.csv"] = new List<int>() { 0, 1, 5, 2, 6 };
        ordering["11-02-2025_13-52-54.csv"] = new List<int>() { 0, 1, 2, 6 };
        ordering["16-12-2024_14-07-40.csv"] = new List<int>() { 0, 1, 6 };
        ordering["19-12-2024_11-54-36.csv"] = new List<int>() { 0, 1, 3, 2, 6 };
        ordering["23-03-2025_21-33-31.csv"] = new List<int>() { 0, 1, 5, 3, 2, 4, 6 }; //jeste baze mezi 4 a 6, mozna i mezi 2 a 4
        ordering["25-03-2025_19-17-19.csv"] = new List<int>() { 0, 1, 5, 2, 4, 3, 6 }; //nejake baze mezi
        ordering["27-03-2025_10-15-21.csv"] = new List<int>() { 0, 5, 1, 4, 2, 3, 6};
        ordering["28-03-2025_13-08-25.csv"] = new List<int>() { 0, 1, 5, 2, 3, 6 };
        ordering["03-04-2025_17-02-37.csv"] = new List<int>() { 0, 2, 1, 6 };
        ordering["03-04-2025_17-27-39.csv"] = new List<int>() { 0, 1, 5, 2, 4, 3, 6 };
        ordering["13-04-2025_20-40-37.csv"] = new List<int>() { 0, 1, 5, 4, 2, 6};

        List<Vector3> positions = new List<Vector3>() { testingDiverPos, diver1Pos, diver2Pos, diver3Pos, diver4Pos, diver5Pos, basePos };

        foreach (string fileName in Directory.GetFiles(Application.persistentDataPath + "/Data"))
        {
            if (fileName.Contains("Settings")) continue;
            reader = new(fileName);
            reader.ReadLine();

            List<float> angles = new();
            List<float> horizontalAngles = new();
            List<float> verticalAngles = new();
            List<int> divers = new();
            List<float> distances = new();
            int lines = 0;
            while (!reader.EndOfStream)
            {
                lines++;
                string[] data = reader.ReadLine().Split(',');
                if (data.Length < 17) break;
                Vector3 pos = new(float.Parse(data[0], CultureInfo.InvariantCulture), float.Parse(data[1], CultureInfo.InvariantCulture), float.Parse(data[2], CultureInfo.InvariantCulture));
                //float distance = float.Parse(data[3]);
                //float time = float.Parse(data[4]);
                Quaternion rotation = new(float.Parse(data[5], CultureInfo.InvariantCulture), float.Parse(data[6], CultureInfo.InvariantCulture), float.Parse(data[7], CultureInfo.InvariantCulture), float.Parse(data[8], CultureInfo.InvariantCulture));
                if (!int.TryParse(data[16], out _)) break;
                int savedDivers = int.Parse(data[16]);
                if (savedDivers >= ordering[fileName.Split('\\')[1]].Count) print(fileName + " " + savedDivers);
                Vector3 currentTargetPos = positions[ordering[fileName.Split('\\')[1]][savedDivers]];
                Vector3 forwardVector = rotation * Vector3.forward;
                Vector3 targetVector = Vector3.Normalize(currentTargetPos - pos);
                divers.Add(savedDivers);
                float angle = Vector3.Angle(forwardVector, Vector3.Normalize(currentTargetPos - pos)); //Quaternion.Angle(rotation, Quaternion.LookRotation(Vector3.Normalize(currentTargetPos - pos)));
                angles.Add(angle);
                float horizontalAngle = Vector3.Angle(new Vector3(forwardVector.x, 0, forwardVector.z), new Vector3(targetVector.x, 0, targetVector.z));
                horizontalAngles.Add(horizontalAngle);
                float verticalAngle = Vector3.Angle(new Vector3(0, forwardVector.y, forwardVector.z), new Vector3(0, targetVector.y, targetVector.z));
                verticalAngles.Add(verticalAngle);
                distances.Add(Vector3.Distance(currentTargetPos, pos));
                
            }

            reader.Close();

            //File.Create(Application.persistentDataPath + "/Data/Processed/" + fileName.Split('\\')[1].Split('.')[0] + ".txt").Close();
            StreamWriter writer = new(Application.persistentDataPath + "/Data/Processed/" + fileName.Split('\\')[1].Split('.')[0] + "_processed.csv");
            writer.Flush();
            for(int i = 0; i < angles.Count; i++)
            {
                string rewrittenAngle = angles[i].ToString().Replace(',', '.');
                string rewrittenHorizontalAngle = horizontalAngles[i].ToString().Replace(',', '.');
                string rewrittenVerticalAngle = verticalAngles[i].ToString().Replace(',', '.');
                string rewrittenDistance = distances[i].ToString().Replace(',', '.');
                writer.WriteLine(rewrittenAngle + "," + rewrittenHorizontalAngle + "," + rewrittenVerticalAngle + "," + rewrittenDistance + "," + divers[i]);
            }
            writer.Close();
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
