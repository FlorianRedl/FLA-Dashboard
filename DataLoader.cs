using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zeitmessung.Models;
using System.Xml.Serialization;
using System.IO;
using System.Diagnostics;
using System.Windows;

namespace Zeitmessung;

public static class DataLoader
{

    private static string _path = AppDomain.CurrentDomain.BaseDirectory + @"\Data.xml";

    public static List<Run> LoadAllRuns(ref List<Run> runs)
    {
        LoadXmlFile(ref runs);
        return runs;
    }

    public static void SaveAllRuns(ref List<Run> runs)
    {
        SaveXmlFile(runs);
    }



    private static void LoadXmlFile(ref List<Run> runs)
    {
        try
        {

            if (File.Exists(_path))
            {
                using (var stream = File.OpenRead(_path))
                {
                    var xmlSerializer = new XmlSerializer(typeof(List<Run>));
                    runs = xmlSerializer.Deserialize(stream) as List<Run>;
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("LoadXmlFile  " + ex.ToString());
            throw;
        }
    }

    private static void SaveXmlFile(List<Run> runs)
    {
        try
        {
            var xmlSeralizer = new XmlSerializer(typeof(List<Run>));
            var writer = new StreamWriter(_path);
            using (writer)
            {
                xmlSeralizer.Serialize(writer, runs);
                //writer.Flush();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("SaveXmlFile  " + ex.ToString());
        }
    }
}
