using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Figures.Materials;
using HelixToolkit.Wpf;

namespace Figures
{
    public class Cylinder : Circle
    {
        public const double TwoPi = 2 * Math.PI;
        public double Height { get; set; }
        public MyMaterial material { get; set; }

        public Cylinder(Point3D center, double radius, double height, int numDivisions)
            : base(center, radius, numDivisions)
        {
            Height = height;
        }

        public override void Draw(MyMaterial material)
        {
            DrawBaseCircle(material);
            DrawSideSurface(material);
        }

        public void Draw(MeshGeometry3D mesh, MyMaterial material)
        {
            DrawBaseCircle(mesh, material);
            DrawSideSurface(mesh, material);
        }

        private void DrawBaseCircle(MyMaterial material)
        {
            base.Draw(material);
        }

        private void DrawBaseCircle(MeshGeometry3D mesh, MyMaterial material)
        {
            base.Draw(mesh, material);
        }

        private void DrawSideSurface(MyMaterial material)
        {
            Vector3D axis = new Vector3D(0, 0, Height);
            AddCylinder(Mesh, Center, axis, Radius, NumDivisions);
        }

        private void DrawSideSurface(MeshGeometry3D mesh, MyMaterial material)
        {
            Vector3D axis = new Vector3D(0, 0, Height);
            AddCylinder(mesh, Center, axis, Radius, NumDivisions);
        }

        private void AddCylinder(MeshGeometry3D mesh,
                                  Point3D end_point, Vector3D axis,
                                  double radius, int num_sides)
        {
            Vector3D v1;
            if ((axis.Z < -0.01) || (axis.Z > 0.01))
                v1 = new Vector3D(axis.Z, axis.Z, -axis.X - axis.Y);
            else
                v1 = new Vector3D(-axis.Y - axis.Z, axis.X, axis.X);
            Vector3D v2 = Vector3D.CrossProduct(v1, axis);

            v1 *= (radius / v1.Length);
            v2 *= (radius / v2.Length);

            double theta = 0;
            double dtheta = 2 * Math.PI / num_sides;
            for (int i = 0; i < num_sides; i++)
            {
                Point3D p1 = end_point +
                    Math.Cos(theta) * v1 +
                    Math.Sin(theta) * v2;
                theta += dtheta;
                Point3D p2 = end_point +
                    Math.Cos(theta) * v1 +
                    Math.Sin(theta) * v2;
                AddTriangle(mesh, end_point, p1, p2);
            }

            Point3D end_point2 = end_point + axis;
            theta = 0;
            for (int i = 0; i < num_sides; i++)
            {
                Point3D p1 = end_point2 +
                    Math.Cos(theta) * v1 +
                    Math.Sin(theta) * v2;
                theta += dtheta;
                Point3D p2 = end_point2 +
                    Math.Cos(theta) * v1 +
                    Math.Sin(theta) * v2;
                AddTriangle(mesh, end_point2, p2, p1);
            }

            theta = 0;
            for (int i = 0; i < num_sides; i++)
            {
                Point3D p1 = end_point +
                    Math.Cos(theta) * v1 +
                    Math.Sin(theta) * v2;
                theta += dtheta;
                Point3D p2 = end_point +
                    Math.Cos(theta) * v1 +
                    Math.Sin(theta) * v2;

                Point3D p3 = p1 + axis;
                Point3D p4 = p2 + axis;

                AddTriangle(mesh, p1, p3, p2);
                AddTriangle(mesh, p2, p3, p4);
            }
        }

        private void AddTriangle(MeshGeometry3D mesh, Point3D p1, Point3D p2, Point3D p3)
        {
            int index = mesh.Positions.Count;
            mesh.Positions.Add(p1);
            mesh.Positions.Add(p2);
            mesh.Positions.Add(p3);
            mesh.TriangleIndices.Add(index++);
            mesh.TriangleIndices.Add(index++);
            mesh.TriangleIndices.Add(index);
        }

        public void ChangeColor(Color color)
        {
            material.SetColor(color);
            Model.Material = material.GetMaterial();
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


            Point3D baseCenter = Center;
            Vector3D baseNormal = new Vector3D(0, -1, 0);
            if (RayTracerLight.RayPlaneIntersection(ray, baseCenter, baseNormal, out Point3D baseHitPoint))
            {
                Vector3D distanceVector = baseHitPoint - baseCenter;
                if (distanceVector.Length <= Radius)
                {
                    hitPoint = baseHitPoint;
                    return true;
                }
            }

            Point3D topCenter = Center + new Vector3D(0, 0, Height);
            if (RayTracerLight.RayPlaneIntersection(ray, topCenter, baseNormal, out Point3D topHitPoint))
            {
                Vector3D distanceVector = topHitPoint - topCenter;
                if (distanceVector.Length <= Radius)
                {
                    hitPoint = topHitPoint;
                    return true;
                }
            }


            Vector3D cylinderAxis = new Vector3D(0, Height, 0);
            Vector3D diff = ray.Origin - baseCenter;
            double m = (Radius / Height) * (Radius / Height);
            double k = 1 + m;
            double a = k * Vector3D.DotProduct(ray.Direction, ray.Direction) - Vector3D.DotProduct(ray.Direction, cylinderAxis) * Vector3D.DotProduct(ray.Direction, cylinderAxis);
            double b = k * 2 * Vector3D.DotProduct(ray.Direction, diff) - 2 * Vector3D.DotProduct(ray.Direction, cylinderAxis) * Vector3D.DotProduct(diff, cylinderAxis);
            double c = k * Vector3D.DotProduct(diff, diff) - Vector3D.DotProduct(diff, cylinderAxis) * Vector3D.DotProduct(diff, cylinderAxis) - Radius * Radius;

            double discriminant = b * b - 4 * a * c;
            if (discriminant >= 0)
            {
                double t1 = (-b - Math.Sqrt(discriminant)) / (2 * a);
                double t2 = (-b + Math.Sqrt(discriminant)) / (2 * a);

                if (t1 > 0 || t2 > 0)
                {
                    double t = t1 > 0 ? t1 : t2;
                    hitPoint = ray.Origin + t * ray.Direction;
                    return true;
                }
            }

            return false;
        }
    }

}
