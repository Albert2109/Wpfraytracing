using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace Figures.Materials
{
    public class MyDiffuseMaterial : MyMaterial
    {
        public MyDiffuseMaterial(Color color) : base(color) { }

        public override Color CalculateColor(Point3D position, Vector3D normal, Vector3D viewDir, List<LightMaterial> lights)
        {
            Color finalColor = Colors.Black;

            foreach (var light in lights)
            {
                Vector3D lightDir = (light.Position - position);
                lightDir.Normalize();

                double diffuseIntensity = Math.Max(0, Vector3D.DotProduct(normal, lightDir));
                Color diffuseColor = MultiplyColor(Color, diffuseIntensity);

                finalColor = AddColors(finalColor, diffuseColor);
            }

            return finalColor;
        }

        protected new Color MultiplyColor(Color color, double factor)
        {
            return Color.FromScRgb(color.ScA * (float)factor, color.ScR, color.ScG, color.ScB);
        }

        protected new Color AddColors(Color color1, Color color2)
        {
            return Color.FromScRgb(
                Math.Min(1.0f, color1.ScA + color2.ScA),
                Math.Min(1.0f, color1.ScR + color2.ScR),
                Math.Min(1.0f, color1.ScG + color2.ScG),
                Math.Min(1.0f, color1.ScB + color2.ScB)
            );
        }

        public override System.Windows.Media.Media3D.Material GetMaterial()
        {
            return new DiffuseMaterial(new SolidColorBrush(Color));
        }

        public System.Windows.Media.Media3D.Material GetMaterialWithImage(string imagePath)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath) || !System.IO.File.Exists(imagePath))
                {
                    throw new ArgumentException("Invalid image path.");
                }

                ImageBrush imageBrush = new ImageBrush();
                imageBrush.ImageSource = new BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute));
                imageBrush.Stretch = Stretch.UniformToFill; 
                imageBrush.Opacity = 1.0; 
                return new DiffuseMaterial(imageBrush);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading image: {ex.Message}");
                return new DiffuseMaterial(new SolidColorBrush(Colors.Gray)); 
            }
        }
    }
}
