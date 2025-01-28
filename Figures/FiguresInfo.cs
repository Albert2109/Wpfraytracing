using Figures.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using System.Windows.Media;

namespace Figures
{


    public class FigureInfo
    {
        public string Name { get; set; }
        public string FigureType { get; set; } 
        public Point3D Position { get; set; }
        public Vector3D Rotation { get; set; }
        public Vector3D Scale { get; set; }
        public Color Color { get; set; }
        public ModelVisual3D ModelVisual { get; set; }
        public MyMaterial CustomMaterial { get; set; }
        public object FigureObject { get; set; } 
    }



}
