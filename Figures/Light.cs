using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Controls;
using HelixToolkit.Wpf;
using Figures.Materials;
using WpfVector3D = System.Windows.Media.Media3D.Vector3D;
using System.Diagnostics;
using System.Windows.Input;

namespace Figures
{
    public class RayTracerLight
    {
        public Point3D Start { get; set; }
        public int RayCount { get; set; }
        public double RayLength { get; set; }
        public Point3D Position { get; set; }
        public Color Color { get; set; }
        public event EventHandler<MouseButtonEventArgs> MouseDown;
        public event EventHandler DeleteClicked;
        public Model3DGroup Model { get; private set; }
        public Viewport3D Viewport { get; set; }
        public TextBox DebugTextBox { get; set; }

        private Dictionary<Model3D, Color> initialColors = new Dictionary<Model3D, Color>();
        private List<Ray> rays;
        private List<ModelVisual3D> rayVisuals = new List<ModelVisual3D>();

        public RayTracerLight(Point3D start, int rayCount, double rayLength, Viewport3D viewport)
        {
            Start = start;
            RayCount = rayCount;
            RayLength = rayLength;
            Viewport = viewport;
            Model = new Model3DGroup();
            rays = GenerateRays();
        }

        public List<Ray> GenerateRays()
        {
            List<Ray> rays = new List<Ray>();
            Random random = new Random();
            for (int i = 0; i < RayCount; i++)
            {
                double theta = 2 * Math.PI * random.NextDouble();
                double phi = Math.Acos(2 * random.NextDouble() - 1);

                double x = Math.Sin(phi) * Math.Cos(theta);
                double y = Math.Sin(phi) * Math.Sin(theta);
                double z = Math.Cos(phi);

                WpfVector3D direction = new WpfVector3D(x, y, z);
                direction.Normalize();
                rays.Add(new Ray(Start, direction));
            }

            return rays;
        }

        public static bool RayPlaneIntersection(Ray ray, Point3D planePoint, WpfVector3D planeNormal, out Point3D hitPoint)
        {
            hitPoint = new Point3D();
            double denominator = WpfVector3D.DotProduct(ray.Direction, planeNormal);
            if (Math.Abs(denominator) > 0.0001)
            {
                WpfVector3D difference = planePoint - ray.Origin;
                double t = WpfVector3D.DotProduct(difference, planeNormal) / denominator;
                if (t >= 0)
                {
                    hitPoint = ray.Origin + t * ray.Direction;
                    return true;
                }
            }
            return false;
        }

        private Ray TransformRay(Ray ray, Transform3D transform)
        {
            Point3D transformedOrigin = transform.Transform(ray.Origin);
            Vector3D transformedDirection = transform.Transform(ray.Direction);
            transformedDirection.Normalize(); 
            return new Ray(transformedOrigin, transformedDirection);
        }


        public void ShowRayTracingResults(List<ModelVisual3D> modelVisuals, List<Figures> figures)
        {
            DebugTextBox.AppendText("Початок трасування променів...\n");

            
            foreach (var modelVisual in modelVisuals)
            {
                var model = modelVisual.Content as GeometryModel3D;
                if (model != null)
                {
                    SaveInitialColor(model);
                    DebugTextBox.AppendText($"Трасування променів для моделі з координатами {model.Bounds}\n");
                    TraceRays(modelVisual);
                }
            }

           
            foreach (var figure in figures)
            {
                var figureGetype = figure.GetType();
                SaveInitialColor(figure.Model);
                TraceRays(figure);
                DebugTextBox.AppendText($"Фігура{figureGetype.Name} з координатами{figure.Model.Bounds}");
               
            }

            Viewport.UpdateLayout();
            Viewport.InvalidateVisual();
            DebugTextBox.AppendText("Трасування променів завершено.\n");
        }


        private void SaveInitialColor(Model3D model)
        {
            if (model is GeometryModel3D geometryModel && geometryModel.Material is DiffuseMaterial diffuseMaterial)
            {
                if (!initialColors.ContainsKey(model))
                {
                    initialColors[model] = ((SolidColorBrush)diffuseMaterial.Brush).Color;
                }
            }
        }

        private void RestoreInitialColor(Model3D model)
        {
            if (initialColors.TryGetValue(model, out var color) && model is GeometryModel3D geometryModel && geometryModel.Material is DiffuseMaterial diffuseMaterial)
            {
                diffuseMaterial.Brush = new SolidColorBrush(color);
            }
        }
        public void TraceRays(Figures figure)
        {
            Transform3D transform = figure.Model.Transform;

            foreach (var ray in rays)
            {
               
                Ray transformedRay = TransformRay(ray, transform);

                
                Debug.WriteLine($"Tracing ray from {transformedRay.Origin} in direction {transformedRay.Direction} for {figure.GetType().Name} at position {figure.Model.Transform.Value.OffsetX}, {figure.Model.Transform.Value.OffsetY}, {figure.Model.Transform.Value.OffsetZ}");

                
                if (figure.IntersectRay(transformedRay, out Point3D hitPoint))
                {
                    figure.ChangeColor(figure.GetLighterColor()); 

                    var lineVisual = CreateLineVisual(ray.Origin, hitPoint, 0.05, Colors.Red);
                    var sphereVisual = CreateSphereVisual(hitPoint, 0.1, Colors.Yellow);
                    rayVisuals.Add(lineVisual);
                    rayVisuals.Add(sphereVisual);
                    Viewport.Children.Add(lineVisual);
                    Viewport.Children.Add(sphereVisual);
                }
                else
                {
                    var lineVisual = CreateLineVisual(ray.Origin, ray.Origin + ray.Direction * RayLength, 0.05, Colors.Blue);
                    rayVisuals.Add(lineVisual);
                    Viewport.Children.Add(lineVisual);
                }
            }
        }

        public void TraceRays(ModelVisual3D modelVisual)
        {
            var model = modelVisual.Content as GeometryModel3D;
            if (model == null)
                return;

            Transform3D transform = model.Transform;
            foreach (var ray in rays)
            {
                Ray transformedRay = TransformRay(ray, transform);
                var intersection = FindNearestIntersection(transformedRay, model);
                if (intersection != null)
                {
                    DebugTextBox.AppendText($"Знайдено перетин у точці {intersection.Point}\n");
                    var lineVisual = CreateLineVisual(ray.Origin, intersection.Point, 0.05, Colors.Red);
                    var sphereVisual = CreateSphereVisual(intersection.Point, 0.1, Colors.Yellow);
                    rayVisuals.Add(lineVisual);
                    rayVisuals.Add(sphereVisual);
                    Viewport.Children.Add(lineVisual);
                    Viewport.Children.Add(sphereVisual);
                }
                else
                {
                    var lineVisual = CreateLineVisual(ray.Origin, ray.Origin + ray.Direction * RayLength, 0.05, Colors.Blue);
                    rayVisuals.Add(lineVisual);
                    Viewport.Children.Add(lineVisual);
                }
            }
        }

        

        public Intersection FindNearestIntersection(Ray ray, GeometryModel3D model)
        {
            Intersection nearestIntersection = null;
            double nearestDistance = double.MaxValue;

            if (model.Geometry == null)
            {
                return null;
            }

            var intersection = IntersectRayGeometry(ray.Origin, ray.Direction, model);
            if (intersection != null && intersection.Distance < nearestDistance)
            {
                nearestIntersection = intersection;
                nearestDistance = intersection.Distance;
                ChangeColorToLighter(model);
            }

            return nearestIntersection;
        }

        public Intersection IntersectRayGeometry(Point3D origin, WpfVector3D direction, GeometryModel3D model)
        {
            if (model.Geometry is MeshGeometry3D mesh)
            {
                for (int i = 0; i < mesh.TriangleIndices.Count; i += 3)
                {
                    Point3D p0 = mesh.Positions[mesh.TriangleIndices[i]];
                    Point3D p1 = mesh.Positions[mesh.TriangleIndices[i + 1]];
                    Point3D p2 = mesh.Positions[mesh.TriangleIndices[i + 2]];

                    if (RayTriangleIntersection(origin, direction, p0, p1, p2, out Point3D hitPoint))
                    {
                        WpfVector3D normal = WpfVector3D.CrossProduct(p1 - p0, p2 - p0);
                        normal.Normalize();
                        double distance = (hitPoint - origin).Length;
                        return new Intersection(distance, hitPoint, normal, model);
                    }
                }
            }
            return null;
        }

        private void ChangeColorToLighter(GeometryModel3D model)
        {
            if (model.Material is DiffuseMaterial diffuseMaterial)
            {
                var color = ((SolidColorBrush)diffuseMaterial.Brush).Color;
                var lighterColor = Color.FromArgb(
                    color.A,
                    (byte)Math.Min(255, color.R + 15),
                    (byte)Math.Min(255, color.G + 15),
                    (byte)Math.Min(255, color.B + 15));

                diffuseMaterial.Brush = new SolidColorBrush(lighterColor);
            }
        }

        public static bool RayTriangleIntersection(Point3D origin, WpfVector3D direction, Point3D v0, Point3D v1, Point3D v2, out Point3D hitPoint)
        {
            hitPoint = new Point3D();

            WpfVector3D e1 = v1 - v0;
            WpfVector3D e2 = v2 - v0;
            WpfVector3D h = WpfVector3D.CrossProduct(direction, e2);
            double a = WpfVector3D.DotProduct(e1, h);

            if (a > -0.00001 && a < 0.00001)
                return false;

            double f = 1.0 / a;
            WpfVector3D s = origin - v0;
            double u = f * WpfVector3D.DotProduct(s, h);

            if (u < 0.0 || u > 1.0)
                return false;

            WpfVector3D q = WpfVector3D.CrossProduct(s, e1);
            double v = f * WpfVector3D.DotProduct(direction, q);

            if (v < 0.0 || u + v > 1.0)
                return false;

            double t = f * WpfVector3D.DotProduct(e2, q);

            if (t > 0.00001)
            {
                hitPoint = origin + t * direction;
                return true;
            }

            return false;
        }

        private ModelVisual3D CreateModelVisual(MeshGeometry3D mesh, Color color)
        {
            var material = new DiffuseMaterial(new SolidColorBrush(color));
            var model = new GeometryModel3D(mesh, material);
            return new ModelVisual3D { Content = model };
        }


        public ModelVisual3D CreateLineVisual(Point3D start, Point3D end, double diameter, Color color)
        {
            var builder = new MeshBuilder();
            builder.AddCylinder(start, end, diameter, 16);
            var mesh = builder.ToMesh();
            return CreateModelVisual(mesh, color);
        }


        public ModelVisual3D CreateSphereVisual(Point3D center, double radius, Color color)
        {
            var builder = new MeshBuilder();
            builder.AddSphere(center, radius, 16, 16);
            var mesh = builder.ToMesh();
            return CreateModelVisual(mesh, color);
        }

        public class Ray
        {
            public Point3D Origin { get; set; }
            public WpfVector3D Direction { get; set; }

            public Ray(Point3D origin, WpfVector3D direction)
            {
                Origin = origin;
                Direction = direction;
                Direction.Normalize();
            }
        }

        public class Intersection
        {
            public double Distance { get; set; }
            public Point3D Point { get; set; }
            public WpfVector3D Normal { get; set; }
            public GeometryModel3D HitModel { get; set; }

            public Intersection(double distance, Point3D point, WpfVector3D normal, GeometryModel3D hitModel)
            {
                Distance = distance;
                Point = point;
                Normal = normal;
                HitModel = hitModel;
            }
        }

        protected void OnMouseDown(MouseButtonEventArgs e)
        {
            MouseDown?.Invoke(this, e);
        }

        protected void OnDeleteClicked()
        {
            DeleteClicked?.Invoke(this, EventArgs.Empty);
        }

        public void InvokeDeleteClicked()
        {
            OnDeleteClicked();
        }
    }
}
