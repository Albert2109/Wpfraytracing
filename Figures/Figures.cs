using System;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Figures.Materials;
using static Figures.RayTracerLight;

namespace Figures
{
    public abstract class Figures
    {
        public event EventHandler<MouseButtonEventArgs> MouseDown;
        public event EventHandler DeleteClicked;
        public GeometryModel3D Model { get; protected set; }
        public MyMaterial Material { get; set; }
        protected virtual void OnMouseDown(MouseButtonEventArgs e)
        {
            MouseDown?.Invoke(this, e);
        }

        protected virtual void OnDeleteClicked()
        {
            DeleteClicked?.Invoke(this, EventArgs.Empty);
        }

        public void ChangeColor(Color color)
        {
            Material.SetColor(color);
            Model.Material = Material.GetMaterial();
        }

        public Color GetLighterColor()
        {
            Color color = Material.GetColor();
            byte r = (byte)Math.Min(color.R + 30, 255);
            byte g = (byte)Math.Min(color.G + 30, 255);
            byte b = (byte)Math.Min(color.B + 30, 255);
            return Color.FromRgb(r, g, b);
        }

        public abstract void Draw(MyMaterial material);
        public abstract bool IntersectRay(Ray ray, out Point3D hitPoint);


        public virtual void InvokeDeleteClicked()
        {
            OnDeleteClicked();
        }
    }
}
