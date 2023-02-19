using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zeitmessung.Models;

namespace Zeitmessung;

public static class DataLoader
{


    public static void GetLastRuns(ref List<Run> runs)
    {
        Run r = new();
        r.StartTime = DateTime.Now;
        runs.Add(r);

    }
    public static List<Run> GetAllRuns()
    {
        List<Run> runs = new();

        return runs;
    }

}
