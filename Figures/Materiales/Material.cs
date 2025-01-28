using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Figures.Materials
{
    public class MyMaterial
    {
        private DiffuseMaterial material;
        public Color Color { get; set; }
        public Texture Texture { get; set; }

        public MyMaterial(Color color)
        {
            Color = color;
        }

        public virtual Color CalculateColor(Point3D position, Vector3D normal, Vector3D viewDir, List<LightMaterial> lights)
        {
            Color finalColor = Color;

            foreach (var light in lights)
            {
                Vector3D lightDir = (light.Position - position);
                lightDir.Normalize();

                double diffuseIntensity = Math.Max(0, Vector3D.DotProduct(normal, lightDir));
                Color diffuseColor = MultiplyColor(GetTextureColor(0, 0), diffuseIntensity);

                finalColor = AddColors(finalColor, diffuseColor);
            }

            return finalColor;
        }

        public virtual System.Windows.Media.Media3D.Material GetMaterial()
        {
            return new System.Windows.Media.Media3D.DiffuseMaterial(new SolidColorBrush(Color));
        }

        protected Color GetTextureColor(double u, double v)
        {
            if (!(Texture is null))
            {
                return Texture.GetColor(u, v);
            }
            return Color.FromRgb(0, 0, 0);
        }

        protected Color MultiplyColor(Color color, double factor)
        {
            byte r = (byte)(color.R * factor);
            byte g = (byte)(color.G * factor);
            byte b = (byte)(color.B * factor);
            return Color.FromRgb(r, g, b);
        }

        protected Color AddColors(Color color1, Color color2)
        {
            byte r = (byte)Math.Min(255, color1.R + color2.R);
            byte g = (byte)Math.Min(255, color1.G + color2.G);
            byte b = (byte)Math.Min(255, color1.B + color2.B);
            return Color.FromRgb(r, g, b);
        }
        public void SetColor(Color color)
        {
            material.Brush = new SolidColorBrush(color);
        }
        public Color GetColor()
        {
            return ((SolidColorBrush)material.Brush).Color;
        }

    }
}
