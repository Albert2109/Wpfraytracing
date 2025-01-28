using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Figures.Materials;
using HelixToolkit.Wpf;

namespace Figures
{
    public class Torus : Circle
    {
        public new GeometryModel3D Model { get; private set; }
        public double Radius2 { get; set; }
        public int Segments2 { get; set; }
        public MyMaterial Material { get; set; }

        public Torus(Point3D center, double radius, double radius2, int numDivisions, int segments2) : base(center, radius, numDivisions)
        {
            Radius2 = radius2;
            Segments2 = segments2;
        }

        public void Draw(MyMaterial material)
        {
            this.Material = material ?? throw new ArgumentNullException(nameof(material));
            Mesh = GenerateTorusMesh(Center, Radius, Radius2, NumDivisions, Segments2);
            Model = new GeometryModel3D(Mesh, material.GetMaterial())
            {
                BackMaterial = new DiffuseMaterial(new SolidColorBrush(material.Color))
            };
        }

        private MeshGeometry3D GenerateTorusMesh(Point3D center, double radius, double radius2, int numDivisions, int segments2)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();

            for (int i = 0; i <= segments2; i++)
            {
                double theta = i * 2 * Math.PI / segments2;
                double cosTheta = Math.Cos(theta);
                double sinTheta = Math.Sin(theta);

                for (int j = 0; j <= numDivisions; j++)
                {
                    double phi = j * 2 * Math.PI / numDivisions;
                    double cosPhi = Math.Cos(phi);
                    double sinPhi = Math.Sin(phi);

                    double x = center.X + (radius2 + radius * cosPhi) * cosTheta;
                    double y = center.Y + radius * sinPhi;
                    double z = center.Z + (radius2 + radius * cosPhi) * sinTheta;

                    Point3D point = new Point3D(x, y, z);
                    mesh.Positions.Add(point);
                }
            }

            for (int i = 0; i < segments2; i++)
            {
                for (int j = 0; j < numDivisions; j++)
                {
                    int currentIndex = i * (numDivisions + 1) + j;
                    int nextIndex = currentIndex + numDivisions + 1;

                    mesh.TriangleIndices.Add(currentIndex);
                    mesh.TriangleIndices.Add(currentIndex + 1);
                    mesh.TriangleIndices.Add(nextIndex);

                    mesh.TriangleIndices.Add(nextIndex);
                    mesh.TriangleIndices.Add(currentIndex + 1);
                    mesh.TriangleIndices.Add(nextIndex + 1);
                }
            }

            return mesh;
        }

        public void ChangeColor(Color color)
        {
            if (Material == null) throw new NullReferenceException("Material is not initialized.");
            Material.SetColor(color);
            Model.Material = Material.GetMaterial();
        }

        public Color GetLighterColor()
        {
            if (Material == null) throw new NullReferenceException("Material is not initialized.");
            Color color = Material.GetColor();
            byte r = (byte)Math.Min(color.R + 30, 255);
            byte g = (byte)Math.Min(color.G + 30, 255);
            byte b = (byte)Math.Min(color.B + 30, 255);
            return Color.FromRgb(r, g, b);
        }

        public bool IntersectRay(RayTracerLight.Ray ray, out Point3D hitPoint)
        {
            hitPoint = new Point3D();

            if (Model == null) throw new NullReferenceException("Model is not initialized.");

         
            RayTracerLight.Ray localRay = TransformRayToLocal(ray);

            Vector3D L = Center - localRay.Origin;
            double tca = Vector3D.DotProduct(L, localRay.Direction);
            if (tca < 0) return false;

            double d2 = Vector3D.DotProduct(L, L) - tca * tca;
            if (d2 > Radius2 * Radius2) return false;

            double thc = Math.Sqrt(Radius2 * Radius2 - d2);
            double t0 = tca - thc;
            double t1 = tca + thc;

            if (t0 > t1)
            {
                double temp = t0;
                t0 = t1;
                t1 = temp;
            }

            if (t0 < 0)
            {
                t0 = t1;
                if (t0 < 0) return false;
            }

            hitPoint = localRay.Origin + t0 * localRay.Direction;
            return true;
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
            if (Model.Transform != null)
            {
                transformGroup.Children.Add(Model.Transform);
            }
            transformGroup.Children.Add(transform);

            for (int i = 0; i < Mesh.Positions.Count; i++)
            {
                Mesh.Positions[i] = transformGroup.Transform(Mesh.Positions[i]);
            }

            Model.Transform = transformGroup;

            UpdateTransformedCenterAndRadius();
        }

        public void UpdatePositions(Point3D transformedCenter, double transformedRadius, double transformedRadius2)
        {
            Center = transformedCenter;
            Radius = transformedRadius;
            Radius2 = transformedRadius2;
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

        public double GetTransformedRadius2()
        {
            if (Model.Transform is Transform3DGroup transformGroup)
            {
                var transformedRadius2Point = transformGroup.Transform(new Point3D(Center.X + Radius2, Center.Y, Center.Z));
                return (transformedRadius2Point - GetTransformedCenter()).Length;
            }
            return Radius2;
        }
        private void UpdateTransformedCenterAndRadius()
        {
            var transform = Model.Transform;
            var transformedCenter = transform.Transform(Center);
            var transformedRadiusPoint = transform.Transform(new Point3D(Center.X + Radius, Center.Y, Center.Z));
            double transformedRadius = (transformedRadiusPoint - transformedCenter).Length;
            var transformedRadius2Point = transform.Transform(new Point3D(Center.X + Radius2, Center.Y, Center.Z));
            double transformedRadius2 = (transformedRadius2Point - transformedCenter).Length;

            Center = transformedCenter;
            Radius = transformedRadius;
            Radius2 = transformedRadius2;
        }
    }
}
