using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class parse : MonoBehaviour
{
    // Assign the competition .txt or .xyz file in the Inspector
    public TextAsset competitionFile;

    public List<Vector3> ParseFile()
    {
        List<Vector3> positions = new List<Vector3>();

        if (competitionFile == null)
        {
            Debug.LogError("Competition file not assigned!");
            return positions;
        }

        // Conversion from inches to Unity meters
        float ScaleFactor = 0.0254f;

        // Read all text from the asset
        string content = competitionFile.text;

        // Split by new lines to get the coordinates
        string[] lines = content.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < lines.Length; i++)
        {
            // Remove extra whitespaces and split by space or tab
            string line = lines[i].Trim();
            string[] coords = line.Split(new[] { ' ', '\t' }, System.StringSplitOptions.RemoveEmptyEntries);

            if (coords.Length >= 3)
            {
                // Parse the X, Y, and Z floats
                float x = float.Parse(coords[0]);
                float y = float.Parse(coords[1]);
                float z = float.Parse(coords[2]);

                Vector3 pos = new Vector3(x, y, z);
                positions.Add(pos * ScaleFactor);
            }
        }

        return positions;
    }
}