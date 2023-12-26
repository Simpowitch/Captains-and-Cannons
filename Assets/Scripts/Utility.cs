using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;

//Simon & Samuel
//Save, load and delete a session (map)
//Shuffle a list

public static class Utility
{
    public static List<T> ShuffleList<T>(List<T> list)
    {
        int count = list.Count;
        for (var i = 0; i < count - 1; ++i)
        {
            int r = UnityEngine.Random.Range(i, count);
            T tmp = list[i];
            list[i] = list[r];
            list[r] = tmp;
        }
        return list;
    }
    public static T ReturnRandom<T>(T[] array)
    {
        if (array != null && array.Length > 0)
        {
            return array[Random.Range(0, array.Length)];
        }
        Debug.LogWarning("Array empty");
        return default;
    }
    #region MapSaver
    public static string mapPath = "/map.map";

    public static bool SaveMap(Map map, MapMovement movementStatus)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + mapPath;
        FileStream stream = new FileStream(path, FileMode.Create);

        MapData data = new MapData(map, movementStatus);

        formatter.Serialize(stream, data);
        stream.Close();
        Debug.Log("Map saved on " + path);

        return true;
    }

    public static MapData LoadMap()
    {
        string path = Application.persistentDataPath + mapPath;
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            MapData data = formatter.Deserialize(stream) as MapData;
            stream.Close();
            Debug.Log("Map loaded from " + path);

            return data;
        }
        else
        {
            Debug.Log("LOAD: Map file not found in " + path);
            return null;
        }
    }

    public static bool DeleteMap()
    {
        string path = Application.persistentDataPath + mapPath;
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("Map deleted from " + path);
            return true;
        }
        else
        {
            Debug.Log("No map to delete found in " + path);
            return false;
        }
    }
    #endregion
}