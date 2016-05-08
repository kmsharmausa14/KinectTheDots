using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using System.Drawing;
using System.IO;
using System.Windows.Media.Media3D;


namespace KinectTheDots
{
    public class DotPuzzle
    {
        public DotPuzzle()
        {
            this.Dots = new List<Point>();
        }

        public List<Point> Dots { get; set; }
    }
}
