using System;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Figures.Materials;


namespace Figures
{
    public class Cone : Circle
    {
        private const double TwoPi = 2 * Math.PI;
        private const double Half = 0.5;
        public double Height { get; set; }
        public new Model3DGroup Model { get; private set; }
        protected MeshGeometry3D OuterConeMesh { get; private set; }
        protected MeshGeometry3D InnerConeMesh { get; private set; }
        public MyMaterial material { get; private set; }

        public Cone(Point3D center, double radius, int numDivisions, double height) : base(center, radius, numDivisions)
        {
            Height = height;
            Model = new Model3DGroup();
            Draw(new MyMaterial(Colors.Black));
        }

        public override void Draw(MyMaterial material)
        {
            this.material = material;
            OuterConeMesh = new MeshGeometry3D();
            InnerConeMesh = new MeshGeometry3D();

            DrawConeBody();
            DrawConeBase();
            CreateGeometryModel(material);
        }

        private void DrawConeBody()
        {
            for (int i = 0; i < NumDivisions; i++)
            {
                double angle = TwoPi * i / NumDivisions;
                double angleNext = TwoPi * (i + 1) / NumDivisions;

                double x1 = Center.X + Radius * Math.Cos(angle);
                double y1 = Center.Y - Height * Half;
                double z1 = Center.Z + Radius * Math.Sin(angle);

                double x2 = Center.X + Radius * Math.Cos(angleNext);
                double y2 = Center.Y - Height * Half;
                double z2 = Center.Z + Radius * Math.Sin(angleNext);

                double x3 = Center.X;
                double y3 = Center.Y + Height * Half;
                double z3 = Center.Z;

                OuterConeMesh.Positions.Add(new Point3D(x1, y1, z1));
                OuterConeMesh.Positions.Add(new Point3D(x2, y2, z2));
                OuterConeMesh.Positions.Add(new Point3D(x3, y3, z3));

                OuterConeMesh.TriangleIndices.Add(i * 3);
                OuterConeMesh.TriangleIndices.Add(i * 3 + 1);
                OuterConeMesh.TriangleIndices.Add(i * 3 + 2);

                InnerConeMesh.Positions.Add(new Point3D(x2, y2, z2));
                InnerConeMesh.Positions.Add(new Point3D(x1, y1, z1));
                InnerConeMesh.Positions.Add(new Point3D(x3, y3, z3));

                InnerConeMesh.TriangleIndices.Add(i * 3);
                InnerConeMesh.TriangleIndices.Add(i * 3 + 1);
                InnerConeMesh.TriangleIndices.Add(i * 3 + 2);
            }
        }

        private void DrawConeBase()
        {
            int baseCenterIndex = OuterConeMesh.Positions.Count;
            Point3D baseCenter = new Point3D(Center.X, Center.Y - Height * Half, Center.Z);

            OuterConeMesh.Positions.Add(baseCenter);
            InnerConeMesh.Positions.Add(baseCenter);

            for (int i = 0; i < NumDivisions; i++)
            {
                double angle = TwoPi * i / NumDivisions;
                double x = Center.X + Radius * Math.Cos(angle);
                double y = Center.Y - Height * Half;
                double z = Center.Z + Radius * Math.Sin(angle);

                OuterConeMesh.Positions.Add(new Point3D(x, y, z));
                InnerConeMesh.Positions.Add(new Point3D(x, y, z));
            }

            for (int i = 0; i < NumDivisions; i++)
            {
                int currentOuterIndex = baseCenterIndex + i + 1;
                int nextOuterIndex = baseCenterIndex + (i + 1) % NumDivisions + 1;

                OuterConeMesh.TriangleIndices.Add(baseCenterIndex);
                OuterConeMesh.TriangleIndices.Add(currentOuterIndex);
                OuterConeMesh.TriangleIndices.Add(nextOuterIndex);

                InnerConeMesh.TriangleIndices.Add(baseCenterIndex);
                InnerConeMesh.TriangleIndices.Add(nextOuterIndex);
                InnerConeMesh.TriangleIndices.Add(currentOuterIndex);
            }
        }

        protected void CreateGeometryModel(MyMaterial material)
        {
            GeometryModel3D outerGeometryModel = new GeometryModel3D(OuterConeMesh, material.GetMaterial());
            GeometryModel3D innerGeometryModel = new GeometryModel3D(InnerConeMesh, material.GetMaterial());

            Model.Children.Add(outerGeometryModel);
            Model.Children.Add(innerGeometryModel);
        }

        public void ChangeColor(Color newColor)
        {
            material = new MyMaterial(newColor);
            foreach (GeometryModel3D model in Model.Children)
            {
                model.Material = material.GetMaterial();
            }
        }

        public Color GetLighterColor()
        {
            var color = material.Color;
            return Color.FromArgb(
                color.A,
                (byte)Math.Min(255, color.R + 30),
                (byte)Math.Min(255, color.G + 30),
                (byte)Math.Min(255, color.B + 30));
        }


        public bool IntersectRay(RayTracerLight.Ray ray, out Point3D hitPoint)
        {
            hitPoint = new Point3D();

           
            Point3D baseCenter = new Point3D(Center.X, Center.Y - Height * 0.5, Center.Z);
            Vector3D baseNormal = new Vector3D(0, -1, 0);
            if (RayTracerLight.RayPlaneIntersection(ray, baseCenter, baseNormal, out Point3D baseHitPoint))
            {
                Vector3D distanceVector = baseHitPoint - baseCenter;
                if (distanceVector.Length <= Radius)
                {
                    hitPoint = baseHitPoint;
                    Debug.WriteLine($"Ray intersects cone base at {hitPoint}");
                    return true;
                }
            }

         
            Vector3D coneAxis = new Vector3D(0, Height, 0);
            Vector3D diff = ray.Origin - Center;
            double m = Radius / Height;
            double k = 1 + m * m;
            double a = k * Vector3D.DotProduct(ray.Direction, ray.Direction) - Vector3D.DotProduct(ray.Direction, coneAxis) * Vector3D.DotProduct(ray.Direction, coneAxis);
            double b = k * 2 * Vector3D.DotProduct(ray.Direction, diff) - 2 * Vector3D.DotProduct(ray.Direction, coneAxis) * Vector3D.DotProduct(diff, coneAxis);
            double c = k * Vector3D.DotProduct(diff, diff) - Vector3D.DotProduct(diff, coneAxis) * Vector3D.DotProduct(diff, coneAxis) - Radius * Radius;

            double discriminant = b * b - 4 * a * c;
            if (discriminant >= 0)
            {
                double t1 = (-b - Math.Sqrt(discriminant)) / (2 * a);
                double t2 = (-b + Math.Sqrt(discriminant)) / (2 * a);

                if (t1 > 0 || t2 > 0)
                {
                    double t = t1 > 0 ? t1 : t2;
                    hitPoint = ray.Origin + t * ray.Direction;
                    Debug.WriteLine($"Ray intersects cone side at {hitPoint}");
                    return true;
                }
            }

            Debug.WriteLine("Ray does not intersect cone");
            return false;
        }


    }
}
