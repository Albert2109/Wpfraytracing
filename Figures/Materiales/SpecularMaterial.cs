using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Figures.Materials
{
    public class MySpecularMaterial : MyMaterial
    {
        public double Shininess { get; set; }
        public double SpecularIntensity { get; set; }
        public double ReflectionIntensity { get; set; }

        public MySpecularMaterial(Color color, double shininess, double specularIntensity, double reflectionIntensity) : base(color)
        {
            Shininess = shininess;
            SpecularIntensity = specularIntensity;
            ReflectionIntensity = reflectionIntensity;
        }

        public override System.Windows.Media.Media3D.Material GetMaterial()
        {
            var diffuseBrush = new SolidColorBrush(Color);
            var specularBrush = new SolidColorBrush(Color.FromArgb((byte)(255 * SpecularIntensity), 255, 255, 255));

            var diffuseMaterial = new DiffuseMaterial(diffuseBrush);
            var specularMaterial = new SpecularMaterial(specularBrush, Shininess);

            var materialGroup = new MaterialGroup();
            materialGroup.Children.Add(diffuseMaterial);
            materialGroup.Children.Add(specularMaterial);

          
            if (ReflectionIntensity > 0)
            {
                var reflectionBrush = new SolidColorBrush(Color.FromArgb((byte)(255 * ReflectionIntensity), 255, 255, 255));
                var reflectionMaterial = new DiffuseMaterial(reflectionBrush);
                materialGroup.Children.Add(reflectionMaterial);
            }

            return materialGroup;
        }

        public override Color CalculateColor(Point3D position, Vector3D normal, Vector3D viewDir, List<LightMaterial> lights)
        {
            Color finalColor = Colors.Black;

            foreach (var light in lights)
            {
                Vector3D lightDir = (light.Position - position);
                lightDir.Normalize();

              
                double diffIntensity = Math.Max(0, Vector3D.DotProduct(normal, lightDir));
                Color diffColor = MultiplyColor(light.Color, diffIntensity);

               
                Vector3D reflectDir = Reflect(-lightDir, normal);
                double specIntensity = Math.Pow(Math.Max(0, Vector3D.DotProduct(viewDir, reflectDir)), Shininess) * SpecularIntensity;
                Color specColor = MultiplyColor(light.Color, specIntensity);

                finalColor = AddColors(finalColor, diffColor);
                finalColor = AddColors(finalColor, specColor);

              
                if (ReflectionIntensity > 0)
                {
                    double reflectionIntensity = ReflectionIntensity * Math.Max(0, Vector3D.DotProduct(viewDir, reflectDir));
                    Color reflectionColor = MultiplyColor(light.Color, reflectionIntensity);
                    finalColor = AddColors(finalColor, reflectionColor);
                }
            }

            return finalColor;
        }

        private Vector3D Reflect(Vector3D v, Vector3D n)
        {
            return v - 2 * Vector3D.DotProduct(v, n) * n;
        }

        private Color MultiplyColor(Color color, double factor)
        {
            byte r = (byte)Math.Min(255, color.R * factor);
            byte g = (byte)Math.Min(255, color.G * factor);
            byte b = (byte)Math.Min(255, color.B * factor);
            return Color.FromRgb(r, g, b);
        }

        private Color AddColors(Color color1, Color color2)
        {
            byte r = (byte)Math.Min(255, color1.R + color2.R);
            byte g = (byte)Math.Min(255, color1.G + color2.G);
            byte b = (byte)Math.Min(255, color1.B + color2.B);
            return Color.FromRgb(r, g, b);
        }
    }
}
