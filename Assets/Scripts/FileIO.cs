using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Xml.Serialization;
using System;
using UnityEngine;

public static class FileIO
{
    private const string saveFolder = @"\Savedata\";

    public static string GetSavePath()
    {
        string savePath = Application.dataPath + saveFolder;
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }
        return savePath;
    }

    public static void BinarySerialize(string fileName, object data)
    {
        BinaryFormatter serializer = new BinaryFormatter();
        MemoryStream stream = new MemoryStream();
        serializer.Serialize(stream, data);

        FileStream fs = File.Open(GetSavePath() + fileName, FileMode.Create);
        using (BinaryWriter writer = new BinaryWriter(fs))
        {
            writer.Write(stream.ToArray());
        }
    }

    public static object BinaryDeserialize(string fileName)
    {
        BinaryFormatter serializer = new BinaryFormatter();
        MemoryStream stream;

        FileStream fs = File.Open(GetSavePath() + fileName, FileMode.Open);
        using (BinaryReader reader = new BinaryReader(fs))
        {
            stream = new MemoryStream(reader.ReadBytes((int)fs.Length));
        }

        return serializer.Deserialize(stream);
    }

    /*public static void XMLSerialize(string fileName, object data)
    {
        XmlSerializer serializer = new XmlSerializer(data.GetType());
        TextWriter writer = new StreamWriter(GetSavePath() + fileName);
        serializer.Serialize(writer, data);
        writer.Close();
    }*/
}
