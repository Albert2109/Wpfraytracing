using System;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Figures.Materials;

namespace Figures
{
    public class Sphere : Circle
    {
        public GeometryModel3D Model { get; private set; }
        private MyMaterial _material;

        public Sphere(Point3D center, double radius, int numDivisions, MyMaterial material)
            : base(center, radius, numDivisions)
        {
            _material = material;
            Draw(material);
        }

        public override void Draw(MyMaterial material)
        {
            var outerMesh = GenerateSphereMesh(Center, Radius, NumDivisions);
            var innerMesh = GenerateSphereMesh(Center, Radius * 0.8, NumDivisions);

            Model = CombineMeshes(outerMesh, innerMesh);
            Model.Material = material.GetMaterial();
        }

        private MeshGeometry3D GenerateSphereMesh(Point3D center, double radius, int numDivisions)
        {
            var mesh = new MeshGeometry3D();

            for (int i = 0; i <= numDivisions; i++)
            {
                double phi = Math.PI * i / numDivisions;
                for (int j = 0; j <= numDivisions; j++)
                {
                    double theta = 2 * Math.PI * j / numDivisions;
                    AddVertexToMesh(mesh, center, radius, phi, theta);
                }
            }

            for (int i = 0; i < numDivisions; i++)
            {
                for (int j = 0; j < numDivisions; j++)
                {
                    AddTriangleIndicesToMesh(mesh, i, j, numDivisions);
                }
            }

            return mesh;
        }

        private void AddVertexToMesh(MeshGeometry3D mesh, Point3D center, double radius, double phi, double theta)
        {
            double x = center.X + radius * Math.Sin(phi) * Math.Cos(theta);
            double y = center.Y + radius * Math.Sin(phi) * Math.Sin(theta);
            double z = center.Z + radius * Math.Cos(phi);
            mesh.Positions.Add(new Point3D(x, y, z));
        }

        private void AddTriangleIndicesToMesh(MeshGeometry3D mesh, int i, int j, int numDivisions)
        {
            int currentIndex = i * (numDivisions + 1) + j;
            int nextIndex = currentIndex + 1;
            int nextRowIndex = (i + 1) * (numDivisions + 1) + j;

            mesh.TriangleIndices.Add(currentIndex);
            mesh.TriangleIndices.Add(nextRowIndex);
            mesh.TriangleIndices.Add(nextIndex);

            mesh.TriangleIndices.Add(nextIndex);
            mesh.TriangleIndices.Add(nextRowIndex);
            mesh.TriangleIndices.Add(nextRowIndex + 1);
        }

        private GeometryModel3D CombineMeshes(MeshGeometry3D outerMesh, MeshGeometry3D innerMesh)
        {
            var combinedMesh = new MeshGeometry3D
            {
                Positions = outerMesh.Positions
            };

            int innerMeshIndexOffset = combinedMesh.Positions.Count;
            AddInnerMeshPositions(combinedMesh, innerMesh);
            AddTriangleIndicesToCombinedMesh(combinedMesh, outerMesh, innerMesh, innerMeshIndexOffset);

            return new GeometryModel3D(combinedMesh, null);
        }

        private void AddInnerMeshPositions(MeshGeometry3D combinedMesh, MeshGeometry3D innerMesh)
        {
            foreach (var innerPos in innerMesh.Positions)
            {
                combinedMesh.Positions.Add(innerPos);
            }
        }

        private void AddTriangleIndicesToCombinedMesh(MeshGeometry3D combinedMesh, MeshGeometry3D outerMesh, MeshGeometry3D innerMesh, int innerMeshIndexOffset)
        {
            foreach (var triangleIndex in outerMesh.TriangleIndices)
            {
                combinedMesh.TriangleIndices.Add(triangleIndex);
            }

            foreach (var triangleIndex in innerMesh.TriangleIndices)
            {
                combinedMesh.TriangleIndices.Add(triangleIndex + innerMeshIndexOffset);
            }
        }

        public bool IntersectRay(RayTracerLight.Ray ray, out Point3D hitPoint)
        {
            hitPoint = new Point3D();
            var localRay = TransformRayToLocal(ray);
            return IntersectRaySphere(localRay, out hitPoint);
        }

        private RayTracerLight.Ray TransformRayToLocal(RayTracerLight.Ray ray)
        {
            Matrix3D inverseTransform = Model.Transform.Value;
            inverseTransform.Invert();

            var localOrigin = inverseTransform.Transform(ray.Origin);
            var localDirection = inverseTransform.Transform(ray.Direction);
            localDirection.Normalize();

            return new RayTracerLight.Ray(localOrigin, localDirection);
        }

        private bool IntersectRaySphere(RayTracerLight.Ray ray, out Point3D hitPoint)
        {
            hitPoint = new Point3D();
            Vector3D oc = ray.Origin - Center;

            double a = Vector3D.DotProduct(ray.Direction, ray.Direction);
            double b = 2.0 * Vector3D.DotProduct(oc, ray.Direction);
            double c = Vector3D.DotProduct(oc, oc) - Radius * Radius;
            double discriminant = b * b - 4 * a * c;

            if (discriminant > 0)
            {
                double t = (-b - Math.Sqrt(discriminant)) / (2.0 * a);
                if (t > 0)
                {
                    hitPoint = ray.Origin + t * ray.Direction;
                    return true;
                }

                t = (-b + Math.Sqrt(discriminant)) / (2.0 * a);
                if (t > 0)
                {
                    hitPoint = ray.Origin + t * ray.Direction;
                    return true;
                }
            }
            return false;
        }

        public void ChangeColor(Color color)
        {
            _material.SetColor(color);
            Model.Material = _material.GetMaterial();
        }

        public Color GetLighterColor()
        {
            var color = _material.GetColor();
            return GetLighterColor(color);
        }

        private Color GetLighterColor(Color color)
        {
            byte r = (byte)Math.Min(color.R + 30, 255);
            byte g = (byte)Math.Min(color.G + 30, 255);
            byte b = (byte)Math.Min(color.B + 30, 255);
            return Color.FromRgb(r, g, b);
        }

        public void ApplyTransform(Transform3D transform)
        {
            var transformGroup = new Transform3DGroup();
            if (Model.Transform != null)
            {
                transformGroup.Children.Add(Model.Transform);
            }
            transformGroup.Children.Add(transform);

            TransformMeshPositions(transformGroup);
            Model.Transform = transformGroup;

            UpdateTransformedCenterAndRadius();
        }

        private void TransformMeshPositions(Transform3DGroup transformGroup)
        {
            for (int i = 0; i < Mesh.Positions.Count; i++)
            {
                Mesh.Positions[i] = transformGroup.Transform(Mesh.Positions[i]);
            }
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
