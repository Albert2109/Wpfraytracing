using System;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Figures.Materials;

namespace Figures
{
    public class Circle:Figures
    {
        public event EventHandler<MouseButtonEventArgs> MouseDown;
        public event EventHandler DeleteClicked;
        public MeshGeometry3D Mesh { get; protected set; }
        public GeometryModel3D Model { get; protected set; }
        public Point3D Center { get; set; }
        public double Radius { get; set; }
        public int NumDivisions { get; set; }
        public MyMaterial Material { get; set; }

        public Circle(Point3D center, double radius, int numDivisions)
        {
            Center = center;
            Radius = radius;
            NumDivisions = numDivisions;
        }

        public override void Draw(MyMaterial material)
        {
            this.Material = material;
            Mesh = new MeshGeometry3D();

            for (int i = 0; i < NumDivisions; i++)
            {
                double angle = 2 * Math.PI * i / NumDivisions;
                double x = Center.X + Radius * Math.Cos(angle);
                double y = Center.Y + Radius * Math.Sin(angle);
                Mesh.Positions.Add(new Point3D(x, y, Center.Z));
            }

            Mesh.Positions.Add(Mesh.Positions[0]);

            for (int i = 0; i < NumDivisions; i++)
            {
                Mesh.TriangleIndices.Add(i);
                Mesh.TriangleIndices.Add((i + 1) % NumDivisions);
                Mesh.TriangleIndices.Add(NumDivisions);
            }

            Model = new GeometryModel3D(Mesh, material.GetMaterial());
        }

        public virtual void Draw(MeshGeometry3D mesh, MyMaterial material)
        {
            double angleStep = 2 * Math.PI / NumDivisions;
            for (int i = 0; i <= NumDivisions; i++)
            {
                double angle = i * angleStep;
                Point3D point = new Point3D(Center.X + Radius * Math.Cos(angle), Center.Y, Center.Z + Radius * Math.Sin(angle));
                mesh.Positions.Add(point);
                mesh.Positions.Add(Center);
            }
        }

        protected virtual void OnMouseDown(MouseButtonEventArgs e)
        {
            MouseDown?.Invoke(this, e);
        }

        protected virtual void OnDeleteClicked()
        {
            DeleteClicked?.Invoke(this, EventArgs.Empty);
        }

        public virtual void InvokeDeleteClicked()
        {
            OnDeleteClicked();
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

        public override bool IntersectRay(RayTracerLight.Ray ray, out Point3D hitPoint)
        {
            hitPoint = new Point3D();

      
            RayTracerLight.Ray localRay = TransformRayToLocal(ray);

        
            Vector3D normal = new Vector3D(0, 0, 1);
            if (RayTracerLight.RayPlaneIntersection(localRay, Center, normal, out Point3D planeHitPoint))
            {
                Vector3D distanceVector = planeHitPoint - Center;
                if (distanceVector.Length <= Radius)
                {
                    hitPoint = planeHitPoint;
                    return true;
                }
            }
            return false;
        }

        private RayTracerLight.Ray TransformRayToLocal(RayTracerLight.Ray ray)
        {
            Matrix3D inverseTransform = Model.Transform.Value;
            inverseTransform.Invert();

            Point3D localOrigin = inverseTransform.Transform(ray.Origin);
            Vector3D localDirection = inverseTransform.Transform(ray.Direction);
            localDirection.Normalize();

            return new RayTracerLight.Ray(localOrigin, localDirection);
        }

        public void ApplyTransform(Transform3D transform)
        {
            var transformGroup = new Transform3DGroup();
            transformGroup.Children.Add(Model.Transform);
            transformGroup.Children.Add(transform);

            for (int i = 0; i < Mesh.Positions.Count; i++)
            {
                Mesh.Positions[i] = transformGroup.Transform(Mesh.Positions[i]);
            }

            Model.Transform = transformGroup;

            UpdateTransformedCenterAndRadius();
        }
        public Point3D GetTransformedCenter()
        {
            if (Model.Transform is Transform3DGroup transformGroup)
            {
                return transformGroup.Transform(Center);
            }
            return Center;
        }

        public double GetTransformedRadius()
        {
            if (Model.Transform is Transform3DGroup transformGroup)
            {
                var transformedRadiusPoint = transformGroup.Transform(new Point3D(Center.X + Radius, Center.Y, Center.Z));
                return (transformedRadiusPoint - GetTransformedCenter()).Length;
            }
            return Radius;
        }
        public void UpdatePositions(Point3D transformedCenter, double transformedRadius)
        {
            Center = transformedCenter;
            Radius = transformedRadius;
        }

        private void UpdateTransformedCenterAndRadius()
        {
            var transform = Model.Transform;
            var transformedCenter = transform.Transform(Center);
            var transformedRadiusPoint = transform.Transform(new Point3D(Center.X + Radius, Center.Y, Center.Z));
            double transformedRadius = (transformedRadiusPoint - transformedCenter).Length;

            Center = transformedCenter;
            Radius = transformedRadius;
        }
    }
}
