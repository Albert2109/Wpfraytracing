using System;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Figures.Materials;

namespace Figures
{
    public class Cube : Plane
    {
        public Model3DGroup Model { get; private set; }
        public Plane[] Faces { get; private set; }
        private MyMaterial material;
        private Point3D[] vertices;

        public Cube(Point3D center, double size, MyMaterial material)
            : base(new Point3D(center.X - size / 2, center.Y - size / 2, center.Z - size / 2),
                   new Point3D(center.X + size / 2, center.Y - size / 2, center.Z - size / 2),
                   new Point3D(center.X + size / 2, center.Y + size / 2, center.Z - size / 2),
                   new Point3D(center.X - size / 2, center.Y + size / 2, center.Z - size / 2), material)
        {
            this.material = material ?? throw new ArgumentNullException(nameof(material));
            Model = new Model3DGroup();
            Faces = new Plane[6];
            Draw(center, size);
        }

        private void Draw(Point3D center, double size)
        {
            Model.Children.Clear();

            vertices = new Point3D[8];
            vertices[0] = new Point3D(center.X - size / 2, center.Y - size / 2, center.Z - size / 2);
            vertices[1] = new Point3D(center.X + size / 2, center.Y - size / 2, center.Z - size / 2);
            vertices[2] = new Point3D(center.X + size / 2, center.Y + size / 2, center.Z - size / 2);
            vertices[3] = new Point3D(center.X - size / 2, center.Y + size / 2, center.Z - size / 2);
            vertices[4] = new Point3D(center.X - size / 2, center.Y - size / 2, center.Z + size / 2);
            vertices[5] = new Point3D(center.X + size / 2, center.Y - size / 2, center.Z + size / 2);
            vertices[6] = new Point3D(center.X + size / 2, center.Y + size / 2, center.Z + size / 2);
            vertices[7] = new Point3D(center.X - size / 2, center.Y + size / 2, center.Z + size / 2);

            MeshGeometry3D mesh = new MeshGeometry3D();
            foreach (Point3D point in vertices)
            {
                mesh.Positions.Add(point);
            }

            mesh.TriangleIndices.Add(4); mesh.TriangleIndices.Add(5); mesh.TriangleIndices.Add(6);
            mesh.TriangleIndices.Add(4); mesh.TriangleIndices.Add(6); mesh.TriangleIndices.Add(7);

            mesh.TriangleIndices.Add(0); mesh.TriangleIndices.Add(2); mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(0); mesh.TriangleIndices.Add(3); mesh.TriangleIndices.Add(2);

            mesh.TriangleIndices.Add(3); mesh.TriangleIndices.Add(6); mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(3); mesh.TriangleIndices.Add(7); mesh.TriangleIndices.Add(6);

            mesh.TriangleIndices.Add(0); mesh.TriangleIndices.Add(1); mesh.TriangleIndices.Add(5);
            mesh.TriangleIndices.Add(0); mesh.TriangleIndices.Add(5); mesh.TriangleIndices.Add(4);

            mesh.TriangleIndices.Add(0); mesh.TriangleIndices.Add(7); mesh.TriangleIndices.Add(3);
            mesh.TriangleIndices.Add(0); mesh.TriangleIndices.Add(4); mesh.TriangleIndices.Add(7);

            mesh.TriangleIndices.Add(1); mesh.TriangleIndices.Add(2); mesh.TriangleIndices.Add(6);
            mesh.TriangleIndices.Add(1); mesh.TriangleIndices.Add(6); mesh.TriangleIndices.Add(5);

            GeometryModel3D geometry = new GeometryModel3D(mesh, material.GetMaterial());
            Model.Children.Add(geometry);

            Faces[0] = new Plane(vertices[4], vertices[5], vertices[6], vertices[7], material);
            Faces[1] = new Plane(vertices[0], vertices[1], vertices[2], vertices[3], material);
            Faces[2] = new Plane(vertices[3], vertices[2], vertices[6], vertices[7], material);
            Faces[3] = new Plane(vertices[0], vertices[4], vertices[5], vertices[1], material);
            Faces[4] = new Plane(vertices[0], vertices[3], vertices[7], vertices[4], material);
            Faces[5] = new Plane(vertices[1], vertices[2], vertices[6], vertices[5], material);
        }

        public void ChangeColor(Color color)
        {
            if (material == null)
            {
                throw new InvalidOperationException("Material is not initialized.");
            }
            material.SetColor(color);
            foreach (var model in Model.Children)
            {
                if (model is GeometryModel3D geometryModel)
                {
                    geometryModel.Material = material.GetMaterial();
                    Debug.WriteLine($"Updated cube color to: {color}");
                }
            }
        }

        public Color GetLighterColor()
        {
            Color color = material.GetColor();
            byte r = (byte)Math.Min(color.R + 30, 255);
            byte g = (byte)Math.Min(color.G + 30, 255);
            byte b = (byte)Math.Min(color.B + 30, 255);
            return Color.FromRgb(r, g, b);
        }

        public bool IntersectRay(RayTracerLight.Ray ray, out Point3D hitPoint)
        {
            hitPoint = new Point3D();
            double closestDistance = double.MaxValue;
            bool hit = false;

            if (IntersectFace(ray, vertices[0], vertices[1], vertices[5], vertices[4], ref closestDistance, ref hitPoint))
            {
                Debug.WriteLine("Hit front face");
                hit = true;
            }
            if (IntersectFace(ray, vertices[1], vertices[2], vertices[6], vertices[5], ref closestDistance, ref hitPoint))
            {
                Debug.WriteLine("Hit right face");
                hit = true;
            }
            if (IntersectFace(ray, vertices[2], vertices[3], vertices[7], vertices[6], ref closestDistance, ref hitPoint))
            {
                Debug.WriteLine("Hit back face");
                hit = true;
            }
            if (IntersectFace(ray, vertices[3], vertices[0], vertices[4], vertices[7], ref closestDistance, ref hitPoint))
            {
                Debug.WriteLine("Hit left face");
                hit = true;
            }
            if (IntersectFace(ray, vertices[4], vertices[5], vertices[6], vertices[7], ref closestDistance, ref hitPoint))
            {
                Debug.WriteLine("Hit top face");
                hit = true;
            }
            if (IntersectFace(ray, vertices[0], vertices[1], vertices[2], vertices[3], ref closestDistance, ref hitPoint))
            {
                Debug.WriteLine("Hit bottom face");
                hit = true;
            }

            return hit;
        }

        private bool IntersectFace(RayTracerLight.Ray ray, Point3D v0, Point3D v1, Point3D v2, Point3D v3, ref double closestDistance, ref Point3D hitPoint)
        {
            if (IntersectTriangle(ray, v0, v1, v2, ref closestDistance, ref hitPoint)) return true;
            if (IntersectTriangle(ray, v0, v2, v3, ref closestDistance, ref hitPoint)) return true;
            return false;
        }

        private bool IntersectTriangle(RayTracerLight.Ray ray, Point3D v0, Point3D v1, Point3D v2, ref double closestDistance, ref Point3D hitPoint)
        {
            Vector3D edge1 = v1 - v0;
            Vector3D edge2 = v2 - v0;
            Vector3D h = Vector3D.CrossProduct(ray.Direction, edge2);
            double a = Vector3D.DotProduct(edge1, h);

            if (a > -0.00001 && a < 0.00001)
                return false;

            double f = 1.0 / a;
            Vector3D s = ray.Origin - v0;
            double u = f * Vector3D.DotProduct(s, h);

            if (u < 0.0 || u > 1.0)
                return false;

            Vector3D q = Vector3D.CrossProduct(s, edge1);
            double v = f * Vector3D.DotProduct(ray.Direction, q);

            if (v < 0.0 || u + v > 1.0)
                return false;

            double t = f * Vector3D.DotProduct(edge2, q);

            if (t > 0.00001)
            {
                Point3D intersectionPoint = ray.Origin + t * ray.Direction;
                double distance = (intersectionPoint - ray.Origin).Length;
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    hitPoint = intersectionPoint;
                }
                return true;
            }

            return false;
        }
    }
}
