using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FLAD.Models;

[Serializable]
public class Run
{
    public TimeSpan Time { get; set; }
    public string TimeString { get { return Time.ToString(@"m\:ss\.ff"); } }
    public DateTime StartTime { get; set; }
    public string StartTimeString { get { return StartTime.ToString(@"dd.MM.yyyy HH:mm"); } }
}
