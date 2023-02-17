using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Zeitmessung.Models
{
    public class Position
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool isEnabled { get; set; }
        public bool isDrawn { get; set; }

        public Position(int id, string name)
        {
            Id = id;    
            Name = name;
            isEnabled = true;
            isDrawn = false;
        }

        public Visibility GetVisibility()
        {
            return isEnabled ? Visibility.Visible : Visibility.Hidden;
        }
    }   
}
