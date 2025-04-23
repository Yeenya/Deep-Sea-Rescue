using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

/*
 * Developed by Jan Borecký, 2024-2025
 * This script handles the whole data processing which is happening in the DataScene.
 */
public class DataProcessing : MonoBehaviour
{
    private StreamReader reader;

    // For thesis purposes, the setup of the map and the divers was always the same, so the positions are hardcoded.
    // If you plan to change the map / position of divers, you need to manually change the positions
    // or move this script into MainScene and reference the positions from the divers themselves.
    private Vector3 testingDiverPos = new(212.271439f, 119.490868f, 219.693787f);
    private Vector3 diver1Pos = new(281.881012f, 36.8699989f, 294.869995f);
    private Vector3 diver2Pos = new(87.8600006f, 23.5f, 94.5f);
    private Vector3 diver3Pos = new(50.2999992f, 40.2999992f, 442.5f);
    private Vector3 diver4Pos = new(443.540009f, 39.0499992f, 90.5899963f);
    private Vector3 diver5Pos = new(427.019989f, 60.6500015f, 438.25f);
    private Vector3 basePos = new(207.893082f, 131.025192f, 187.043793f);

    void Start()
    {
        Dictionary<string, List<int>> ordering = Player.ordering;

        List<Vector3> positions = new() { testingDiverPos, diver1Pos, diver2Pos, diver3Pos, diver4Pos, diver5Pos, basePos };

        // Loop over every replay file in the Data folder in the persistent data path (on Windows: .../Users/your_username/AppData/LocalLow/DefaultCompany/Deep Sea Rescue/Data)
        foreach (string fileName in Directory.GetFiles(Application.persistentDataPath + "/Data"))
        {
            // We do not care about settings when processing data. However, if you plan to change the diver positions (as mentioned above),
            // you can load the positions from the settings files.
            if (fileName.Contains("Settings")) continue; 
            reader = new(fileName);
            reader.ReadLine(); // Skip the first line containing the headers.

            List<float> angles = new();
            List<float> horizontalAngles = new();
            List<float> verticalAngles = new();
            List<int> divers = new();
            List<float> distances = new();
            int lines = 0;
            while (!reader.EndOfStream)
            {
                lines++;
                string[] data = reader.ReadLine().Split(','); // It is a CSV file, so we split the line by commas. See the files for the data structuring and order.
                if (data.Length < 17) break; // If the line does not contain all data it should, then end the process.

                Vector3 pos = new(float.Parse(data[0], CultureInfo.InvariantCulture), float.Parse(data[1], CultureInfo.InvariantCulture), float.Parse(data[2], CultureInfo.InvariantCulture));
                Quaternion rotation = new(float.Parse(data[5], CultureInfo.InvariantCulture), float.Parse(data[6], CultureInfo.InvariantCulture), float.Parse(data[7], CultureInfo.InvariantCulture), float.Parse(data[8], CultureInfo.InvariantCulture));
                if (!int.TryParse(data[16], out _)) break; // The number of saved divers might not load properly, so this ensures that it does or the data processing ends.
                int savedDivers = int.Parse(data[16]);

                Vector3 currentTargetPos = positions[ordering[fileName.Split('\\')[1]][savedDivers]]; // Get the correct diver's position based on the order in which they were saved.
                Vector3 forwardVector = rotation * Vector3.forward;
                Vector3 targetVector = Vector3.Normalize(currentTargetPos - pos);
                divers.Add(savedDivers);

                // Calculate the angular difference between the direction the player is facing with the submarine and the direction to the diver.
                float angle = Vector3.Angle(forwardVector, Vector3.Normalize(currentTargetPos - pos)); 
                angles.Add(angle);
                float horizontalAngle = Vector3.Angle(new Vector3(forwardVector.x, 0, forwardVector.z), new Vector3(targetVector.x, 0, targetVector.z)); // Flatten the vectors to the XZ plane.
                horizontalAngles.Add(horizontalAngle);
                float verticalAngle = Vector3.Angle(new Vector3(0, forwardVector.y, forwardVector.z), new Vector3(0, targetVector.y, targetVector.z)); // Flatten the vectors to the YZ plane.
                verticalAngles.Add(verticalAngle);

                distances.Add(Vector3.Distance(currentTargetPos, pos));
                
            }

            reader.Close();

            // Create a new CSV file named after the original file with "_processed" suffix.
            StreamWriter writer = new(Application.persistentDataPath + "/Data/Processed/" + fileName.Split('\\')[1].Split('.')[0] + "_processed.csv");
            writer.Flush(); // Clear the file before writing if it already existed.
            for (int i = 0; i < angles.Count; i++)
            {
                // Write the computed values to the new CSV file.
                string rewrittenAngle = angles[i].ToString().Replace(',', '.');
                string rewrittenHorizontalAngle = horizontalAngles[i].ToString().Replace(',', '.');
                string rewrittenVerticalAngle = verticalAngles[i].ToString().Replace(',', '.');
                string rewrittenDistance = distances[i].ToString().Replace(',', '.');
                writer.WriteLine(rewrittenAngle + "," + rewrittenHorizontalAngle + "," + rewrittenVerticalAngle + "," + rewrittenDistance + "," + divers[i]);
            }
            writer.Close();
        }
    }
}
