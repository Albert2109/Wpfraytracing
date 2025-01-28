using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Figures.Materials
{
    public class LightMaterial
    {
        public Point3D Position { get; set; }
        public Color Color { get; set; }

        public LightMaterial(Point3D position, Color color)
        {
            Position = position;
            Color = color;
        }
    }
}
