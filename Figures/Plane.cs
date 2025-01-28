using System;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using Figures.Materials;

namespace Figures
{
    public class Plane:Figures
    {
        public event EventHandler<MouseButtonEventArgs> MouseDown;
        public event EventHandler DeleteClicked;
        public Point3D P0 { get; private set; }
        public Point3D P1 { get; private set; }
        public Point3D P2 { get; private set; }
        public Point3D P3 { get; private set; }
        public MeshGeometry3D Mesh { get; private set; }
        public GeometryModel3D Model { get; private set; }
        private MyMaterial material;

        public Plane(Point3D p0, Point3D p1, Point3D p2, Point3D p3, MyMaterial material)
        {
            P0 = p0;
            P1 = p1;
            P2 = p2;
            P3 = p3;
            this.material = material;

            Mesh = new MeshGeometry3D();
            InitializeMesh();

            Model = new GeometryModel3D
            {
                Geometry = Mesh,
                Material = material.GetMaterial()
            };
        }

        public void ApplyTransform(Transform3D transform)
        {
            P0 = transform.Transform(P0);
            P1 = transform.Transform(P1);
            P2 = transform.Transform(P2);
            P3 = transform.Transform(P3);
            InitializeMesh(); 
            Model.Geometry = Mesh; 
        }

        private void InitializeMesh()
        {
            Mesh.Positions.Clear();
            Mesh.TriangleIndices.Clear();

            Mesh.Positions.Add(P0);
            Mesh.Positions.Add(P1);
            Mesh.Positions.Add(P2);
            Mesh.Positions.Add(P3);

            Mesh.TriangleIndices.Add(0);
            Mesh.TriangleIndices.Add(1);
            Mesh.TriangleIndices.Add(2);

            Mesh.TriangleIndices.Add(1);
            Mesh.TriangleIndices.Add(3);
            Mesh.TriangleIndices.Add(2);
        }
        public override void Draw(MyMaterial material)
        {
            Material = material;
            InitializeMesh();
            Model.Material = material.GetMaterial();
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

        public void UpdatePositions(Point3D p0, Point3D p1, Point3D p2, Point3D p3)
        {
            P0 = p0;
            P1 = p1;
            P2 = p2;
            P3 = p3;
            InitializeMesh();
        }

        public (Point3D, Point3D, Point3D, Point3D) GetTransformedPositions()
        {
            var transform = Model.Transform;
            var transformedP0 = transform.Transform(P0);
            var transformedP1 = transform.Transform(P1);
            var transformedP2 = transform.Transform(P2);
            var transformedP3 = transform.Transform(P3);
            return (transformedP0, transformedP1, transformedP2, transformedP3);
        }

        public override bool IntersectRay(RayTracerLight.Ray ray, out Point3D hitPoint)
        {
            hitPoint = new Point3D();

            Vector3D dir = ray.Direction;
            Point3D orig = ray.Origin;

            Vector3D edge1 = P1 - P0;
            Vector3D edge2 = P3 - P0;
            Vector3D h = Vector3D.CrossProduct(dir, edge2);
            double a = Vector3D.DotProduct(edge1, h);

            if (a > -0.00001 && a < 0.00001)
                return false;

            double f = 1.0 / a;
            Vector3D s = orig - P0;
            double u = f * Vector3D.DotProduct(s, h);

            if (u < 0.0 || u > 1.0)
                return false;

            Vector3D q = Vector3D.CrossProduct(s, edge1);
            double v = f * Vector3D.DotProduct(dir, q);

            if (v < 0.0 || u + v > 1.0)
                return false;

            double t = f * Vector3D.DotProduct(edge2, q);

            if (t > 0.00001)
            {
                hitPoint = orig + t * dir;
                return true;
            }

            return false;
        }

        public Vector3D GetNormal()
        {
            Vector3D edge1 = P1 - P0;
            Vector3D edge2 = P3 - P0;
            return Vector3D.CrossProduct(edge1, edge2);
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
    }
}
