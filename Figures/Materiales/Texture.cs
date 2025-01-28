using System.Windows.Media;
using System.Windows.Media.Media3D;


namespace Figures.Materials
{
    public class Texture
    {
        private Color[,] _colors;

        public Texture(Color[,] colors)
        {
            _colors = colors;
        }

        public virtual Color GetColor(double u, double v)
        {


            int x = (int)(u * _colors.GetLength(0));
            int y = (int)(v * _colors.GetLength(1));


            if (x < 0 || x >= _colors.GetLength(0) || y < 0 || y >= _colors.GetLength(1))
            {

                return Colors.Black;
            }

            return _colors[x, y];
        }
    }
  
}
