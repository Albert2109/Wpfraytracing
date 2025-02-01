using Figures;
using Figures.Materials;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using FM = Figures.Materials;

namespace RayGen
{
    public partial class MainWindow : Window
    {
        private PerspectiveCamera mainCamera;
        private ModelVisual3D selectedModel;
        private System.Windows.Point previousMousePos;
        private Nav modelTransformer;
        private bool _isMouseDown;
        private Transform3DGroup _transformGroup;
        private RayTracerLight rayTracer;
        private ImageBrush selectedTexture;
        private System.Windows.Media.Color lightColor = System.Windows.Media.Colors.White;
        private System.Windows.Media.Media3D.Vector3D lightDirection = new System.Windows.Media.Media3D.Vector3D(-1, -1, -1);

        private List<FigureInfo> figuresInfo = new List<FigureInfo>();
       
        public MainWindow()
        {
            InitializeComponent();
            InitializeCamera();
            InitializeLighting();
            InitializeContextMenu();
            modelTransformer = new Nav();
            viewport3d.MouseDown += Viewport3D_MouseDown;
            viewport3d.MouseMove += Viewport3D_MouseMove;
            viewport3d.MouseUp += Viewport3D_MouseUp;
            KeyDown += Window_KeyDown;
            shapesListBox.SelectionChanged += ShapesListBox_SelectionChanged;
        }

        private void InitializeCamera()
        {
            mainCamera = new PerspectiveCamera(new Point3D(0, 0, 10), new System.Windows.Media.Media3D.Vector3D(0, 0, -1), new System.Windows.Media.Media3D.Vector3D(0, 1, 0), 45);
            viewport3d.Camera = mainCamera;
        }

        private void InitializeLighting()
        {
            AmbientLight ambientLight = new AmbientLight(System.Windows.Media.Colors.Gray);
            ModelVisual3D ambientLightModel = new ModelVisual3D();
            ambientLightModel.Content = ambientLight;
            viewport3d.Children.Add(ambientLightModel);

            DirectionalLight directionalLight = new DirectionalLight();
            directionalLight.Color = System.Windows.Media.Colors.White;
            directionalLight.Direction = new System.Windows.Media.Media3D.Vector3D(-1, -1, -1);
            ModelVisual3D directionalLightModel = new ModelVisual3D();
            directionalLightModel.Content = directionalLight;
            viewport3d.Children.Add(directionalLightModel);
        }

        private void InitializeContextMenu()
        {
            ContextMenu viewportContextMenu = new ContextMenu();
            MenuItem deleteMenuItem = new MenuItem();
            deleteMenuItem.Header = "Delete";
            deleteMenuItem.Click += DeleteMenuItem_Click;
            viewportContextMenu.Items.Add(deleteMenuItem);
            viewport3d.ContextMenu = viewportContextMenu;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            double moveSpeed = 1;
            double rotateSpeed = 1;

            var keyActions = new Dictionary<Key, Action>
            {
                { Key.W, () => MoveCamera(moveSpeed * mainCamera.LookDirection) },
                { Key.S, () => MoveCamera(-moveSpeed * mainCamera.LookDirection) },
                { Key.A, () => MoveCamera(-System.Windows.Media.Media3D.Vector3D.CrossProduct(mainCamera.LookDirection, mainCamera.UpDirection) * moveSpeed) },
                { Key.D, () => MoveCamera(System.Windows.Media.Media3D.Vector3D.CrossProduct(mainCamera.LookDirection, mainCamera.UpDirection) * moveSpeed) },
                { Key.Left, () => RotateCamera(rotateSpeed, mainCamera.UpDirection) },
                { Key.Up, () => RotateCamera(-rotateSpeed, System.Windows.Media.Media3D.Vector3D.CrossProduct(mainCamera.UpDirection, mainCamera.LookDirection)) },
                { Key.Down, () => RotateCamera(rotateSpeed, System.Windows.Media.Media3D.Vector3D.CrossProduct(mainCamera.UpDirection, mainCamera.LookDirection)) },
                { Key.Right, () => RotateCamera(-rotateSpeed, mainCamera.UpDirection) },
                { Key.Space, () => MoveCamera(new System.Windows.Media.Media3D.Vector3D(0, moveSpeed, 0)) },
                { Key.LeftShift, () => MoveCamera(new System.Windows.Media.Media3D.Vector3D(0, -moveSpeed, 0)) }
            };

            if (keyActions.ContainsKey(e.Key))
            {
                keyActions[e.Key].Invoke();
            }
        }

        private void MoveCamera(System.Windows.Media.Media3D.Vector3D direction)
        {
            mainCamera.Position += direction;
        }

        private void RotateCamera(double angle, System.Windows.Media.Media3D.Vector3D axis)
        {
            Point3D cameraPosition = mainCamera.Position;
            System.Windows.Media.Media3D.Vector3D cameraLookDirection = mainCamera.LookDirection;
            System.Windows.Media.Media3D.Vector3D cameraUpDirection = mainCamera.UpDirection;

            RotateTransform3D rotation = new RotateTransform3D(new AxisAngleRotation3D(axis, angle), cameraPosition);
            cameraLookDirection = rotation.Transform(cameraLookDirection);
            cameraUpDirection = rotation.Transform(cameraUpDirection);

            mainCamera.LookDirection = cameraLookDirection;
            mainCamera.UpDirection = cameraUpDirection;

            mainCamera.Position = cameraPosition + cameraLookDirection;
        }

        private void AddShape(string shapeName, ModelVisual3D modelVisual, System.Windows.Media.Color color, MyMaterial material, object figureObject)
        {
            viewport3d.Children.Add(modelVisual);

            ListBoxItem listItem = new ListBoxItem();
            listItem.Content = shapeName;
            listItem.Tag = modelVisual;
            shapesListBox.Items.Add(listItem);

            figuresInfo.Add(new FigureInfo
            {
                Name = shapeName,
                FigureType = shapeName,
                Position = new Point3D(0, 0, 0),
                Rotation = new System.Windows.Media.Media3D.Vector3D(0, 0, 0),
                Scale = new System.Windows.Media.Media3D.Vector3D(1, 1, 1),
                Color = color,
                ModelVisual = modelVisual,
                CustomMaterial = material,
                FigureObject = figureObject
            });

            debugTextBox.AppendText($"Added shape: {shapeName}\n");
            debugTextBox.AppendText($"Total shapes in viewport: {viewport3d.Children.Count}\n");
            debugTextBox.ScrollToEnd();
        }

        private void DrawPlane()
        {
            var color = colorPicker.SelectedColor ?? System.Windows.Media.Colors.DarkGreen;
            var material = GetSelectedMaterial(color);

            Figures.Plane plane = new Figures.Plane(
                new Point3D(5, 5, 5),
                new Point3D(8,5, 5),
                new Point3D(5, 8, 5),
                new Point3D(8, 8, 5),
                material
            );

            ModelVisual3D modelVisual = new ModelVisual3D();
            modelVisual.Content = plane.Model;

            AddShape("Plane", modelVisual, color, material, plane);
           
        }

        private void AddPlane(object sender, RoutedEventArgs e)
        {
            DrawPlane();
            UpdateFigureTransforms();
            
        }

       

        private List<Cube> cubes = new List<Cube>();
        private void AddCube(object sender, RoutedEventArgs e)
        {
            var color = colorPicker.SelectedColor ?? System.Windows.Media.Colors.Green;
            var material = GetSelectedMaterial(color);

            Cube cube = new Cube(new Point3D(7, -5, 0), 5, material);
            var modelVisual = new ModelVisual3D { Content = cube.Model };
            AddShape("Cube", modelVisual, color, material, cube);

            LogDebugMessage("Cube created and added to viewport.");
            LogDebugMessage($"Current children count in viewport: {viewport3d.Children.Count}");
            LogDebugMessage($"Added cube with color: {color}");

            cubes.Add(cube);
            AnalyzeViewportObjects();

            LogDebugMessage($"Found {cubes.Count} cube(s) in viewport.");
        }

        private void LogDebugMessage(string message)
        {
            debugTextBox.AppendText(message + "\n");
        }

        private void AnalyzeViewportObjects()
        {
            foreach (var item in viewport3d.Children)
            {
                if (item is ModelVisual3D { Content: Model3DGroup modelGroup })
                {
                    foreach (var child in modelGroup.Children)
                    {
                        if (child is GeometryModel3D { Geometry: MeshGeometry3D mesh })
                        {
                            string objectType = mesh.Positions.Count == 8 ? "Cube" : "non-Cube object";
                            LogDebugMessage($"Found a {objectType} in the viewport.");
                        }
                    }
                }
            }
        }

        private void AddCircle(object sender, RoutedEventArgs e)
        {
            var material = new FM.MyDiffuseMaterial(System.Windows.Media.Colors.DarkCyan);
            Circle circle = new Circle(new Point3D(10, 7, 0), 5, 60);
            circle.Draw(material);

            ModelVisual3D modelVisual = new ModelVisual3D();
            modelVisual.Content = circle.Model;

            AddShape("Circle", modelVisual, System.Windows.Media.Colors.DarkCyan, material, circle);
            
        }

        private void AddCilinder(object sender, RoutedEventArgs e)
        {
            var material = new FM.MyDiffuseMaterial(System.Windows.Media.Colors.Red);
            Cylinder cylinder = new Cylinder(new Point3D(12, 3, 0), 4, 8, 60);
            cylinder.Draw(material);
            ModelVisual3D modelVisual = new ModelVisual3D();
            modelVisual.Content = cylinder.Model;

            AddShape("Cilinder", modelVisual, System.Windows.Media.Colors.Red, material, cylinder);
          
        }

        private void AddCone(object sender, RoutedEventArgs e)
        {
            var material = new FM.MyDiffuseMaterial(System.Windows.Media.Colors.Green);
            Cone cone = new Cone(new Point3D(0, 0, -6), 5, 60, 10);
            cone.Draw(material);
            ModelVisual3D modelVisual = new ModelVisual3D();
            modelVisual.Content = cone.Model;

            AddShape("Cone", modelVisual, System.Windows.Media.Colors.Green, material, cone);
           
        }

        private void AddSphere(object sender, RoutedEventArgs e)
        {
            var color = colorPicker.SelectedColor ?? System.Windows.Media.Colors.DarkBlue;
            var material = GetSelectedMaterial(color);

            Sphere sphere = new Sphere(new Point3D(0, 0, 0), 4, 60, material);
            sphere.Draw(material);
            ModelVisual3D modelVisual3D = new ModelVisual3D();
            modelVisual3D.Content = sphere.Model;

            AddShape("Sphere", modelVisual3D, color, material, sphere);
            debugTextBox.AppendText($"Sphere added with material: {material.GetType().Name}\n");
            
        }

        private void AddTorus(object sender, RoutedEventArgs e)
        {
            try
            {

            
            var material = new FM.MyDiffuseMaterial(System.Windows.Media.Colors.Yellow);
            Torus torus = new Torus(new Point3D(10, -8, 6), 1, 4, 60, 180);
            torus.Draw(material);
            ModelVisual3D modelVisual3D = new ModelVisual3D();
            modelVisual3D.Content = torus.Model;

            AddShape("Torus", modelVisual3D, System.Windows.Media.Colors.Yellow, material, torus);
               
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating torus: {ex.Message}");
            }
        }

        private void AddLight(object sender, RoutedEventArgs e)
        {
            try
            {

           
               
                Point3D lightPosition = new Point3D(10, 10, 10);
                System.Windows.Media.Color lightColor = System.Windows.Media.Colors.Red;
               
                rayTracer = new RayTracerLight(lightPosition, 140, 15.0, viewport3d)
                {
                     DebugTextBox = debugTextBox, 
                    Position = lightPosition,
                    Color = lightColor
                };
                UpdateLight();
               
                ModelVisual3D lightVisual = rayTracer.CreateSphereVisual(lightPosition, 0.2, System.Windows.Media.Colors.Red);
                viewport3d.Children.Add(lightVisual);

               
            }
            catch (Exception ex)
            {
                debugTextBox.AppendText($"Error: {ex.Message}\n");
            }
        }

        private void UpdateLight()
        {
            
            UpdateFigureTransforms();

            
            var figures = GetFiguresLists();

           
            var modelVisuals = viewport3d.Children.OfType<ModelVisual3D>().ToList();

           
            foreach (var figure in figures)
            {
                if (figure is Figures.Plane plane)
                {
                    
                    var (p0, p1, p2, p3) = plane.GetTransformedPositions();
                    Debug.WriteLine($"Plane Transformed Positions: P0: {p0}, P1: {p1}, P2: {p2}, P3: {p3}");
                    plane.UpdatePositions(p0, p1, p2, p3);
                } 
            }
            rayTracer.ShowRayTracingResults(modelVisuals, figures.OfType<Figures.Figures>().ToList());
              
        }




        private void UpdateFigureTransforms()
        {
            foreach (var figureInfo in figuresInfo)
            {
                var modelVisual = figureInfo.ModelVisual;
                if (modelVisual.Transform is Transform3DGroup transformGroup)
                {
                    var translation = modelTransformer.GetTranslation(transformGroup);
                    var rotation = modelTransformer.GetRotation(transformGroup);
                    var scale = modelTransformer.GetScale(transformGroup);

                    figureInfo.Position = new Point3D(translation.X, translation.Y, translation.Z);
                    figureInfo.Rotation = new System.Windows.Media.Media3D.Vector3D(rotation.Axis.X, rotation.Axis.Y, rotation.Axis.Z);
                    figureInfo.Scale = scale;
                    if (figureInfo.FigureObject is Plane plane)
                    {
                        plane.ApplyTransform(transformGroup);
                        var (transformedP0, transformedP1, transformedP2, transformedP3) = plane.GetTransformedPositions();
                        plane.UpdatePositions(transformedP0, transformedP1, transformedP2, transformedP3);
                    }
                    else if (figureInfo.FigureObject is Circle circle)
                    {
                        circle.ApplyTransform(transformGroup);
                        var transformedCenter = transformGroup.Transform(circle.Center);
                        var transformedRadiusPoint = transformGroup.Transform(new Point3D(circle.Center.X + circle.Radius, circle.Center.Y, circle.Center.Z));
                        double transformedRadius = (transformedRadiusPoint - transformedCenter).Length;
                        circle.UpdatePositions(transformedCenter, transformedRadius);
                    }
                    else if (figureInfo.FigureObject is Torus torus)
                    {
                        torus.ApplyTransform(transformGroup);
                        var transformedCenter = transformGroup.Transform(torus.Center);
                        var transformedRadiusPoint = transformGroup.Transform(new Point3D(torus.Center.X + torus.Radius, torus.Center.Y, torus.Center.Z));
                        double transformedRadius = (transformedRadiusPoint - transformedCenter).Length;
                        var transformedRadius2Point = transformGroup.Transform(new Point3D(torus.Center.X + torus.Radius2, torus.Center.Y, torus.Center.Z));
                        double transformedRadius2 = (transformedRadius2Point - transformedCenter).Length;
                        torus.UpdatePositions(transformedCenter, transformedRadius, transformedRadius2);
                    }
                    else if (figureInfo.FigureObject is Sphere sphere)
                    {
                        sphere.ApplyTransform(transformGroup);
                        var transformedCenter = transformGroup.Transform(sphere.Center);
                        var transformedRadiusPoint = transformGroup.Transform(new Point3D(sphere.Center.X + sphere.Radius, sphere.Center.Y, sphere.Center.Z));
                        double transformedRadius = (transformedRadiusPoint - transformedCenter).Length;
                        sphere.UpdatePositions(transformedCenter, transformedRadius);
                    }


                    debugTextBox.AppendText($"Updated {figureInfo.Name} - Position: {figureInfo.Position}, Rotation: {figureInfo.Rotation}, Scale: {figureInfo.Scale}\n");
                }
            }
        }





        

            private List<Figures.Figures> GetFiguresLists()
            {
                List<Figures.Figures> figures = new List<Figures.Figures>();

                foreach (var figureInfo in figuresInfo)
                {
                    var modelVisual = figureInfo.ModelVisual;

                    if (modelVisual.Transform is Transform3DGroup transformGroup)
                    {
                        var translation = modelTransformer.GetTranslation(transformGroup);
                        var rotation = modelTransformer.GetRotation(transformGroup);
                        var scale = modelTransformer.GetScale(transformGroup);

                        figureInfo.Position = new Point3D(translation.X, translation.Y, translation.Z);
                        figureInfo.Rotation = new System.Windows.Media.Media3D.Vector3D(rotation.Axis.X, rotation.Axis.Y, rotation.Axis.Z);
                        figureInfo.Scale = scale;

                        debugTextBox.AppendText($"Updated {figureInfo.Name} - Position: {figureInfo.Position}, Rotation: {figureInfo.Rotation}, Scale: {figureInfo.Scale}\n");
                    }

                   
                }

                return figures;
            }

        



        private void InitializeTransformGroup()
        {
            selectedModel.Transform = modelTransformer.CreateTransformGroup();
        }

        private void Viewport3D_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                previousMousePos = e.GetPosition(viewport3d);

                System.Windows.Point mousePos = e.GetPosition(viewport3d);
                HitTestResult hitTestResult = VisualTreeHelper.HitTest(viewport3d, mousePos);

                if (hitTestResult != null && hitTestResult.VisualHit is ModelVisual3D)
                {
                    selectedModel = hitTestResult.VisualHit as ModelVisual3D;

                    if (selectedModel.Transform is Transform3DGroup existingTransformGroup)
                    {
                        _transformGroup = existingTransformGroup;
                    }
                    else
                    {
                        InitializeTransformGroup();
                    }

                    previousMousePos = e.GetPosition(viewport3d);
                    UpdateTransformFields();
                    SelectShapeInListBox(selectedModel);
                }
            }
            catch (Exception ex)
            {
                debugTextBox.AppendText($"Error on mouse down: {ex.Message}\n");
            }
        }

        private void Viewport3D_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.LeftButton == MouseButtonState.Pressed && selectedModel != null)
                {
                    System.Windows.Point currentMousePos = e.GetPosition(viewport3d);

                    if (Keyboard.IsKeyDown(Key.M))
                    {
                        ApplyTranslation(currentMousePos);
                    }

                    if (Keyboard.IsKeyDown(Key.R))
                    {
                        ApplyRotation(currentMousePos);
                    }

                    if (Keyboard.IsKeyDown(Key.E))
                    {
                        ApplyScaling(currentMousePos);
                    }

                    UpdateTransformFields();
                }
            }
            catch (Exception ex)
            {
                debugTextBox.AppendText($"Error on mouse move: {ex.Message}\n");
            }
        }

        private void ApplyTranslation(System.Windows.Point currentMousePos)
        {
            Point3D currentHitPosition = modelTransformer.GetHitPosition(viewport3d, currentMousePos);
            Point3D previousHitPosition = modelTransformer.GetHitPosition(viewport3d, previousMousePos);
            double deltaX = currentHitPosition.X - previousHitPosition.X;
            double deltaY = currentHitPosition.Y - previousHitPosition.Y;
            double deltaZ = currentHitPosition.Z - previousHitPosition.Z;

            modelTransformer.TranslateModel((Transform3DGroup)selectedModel.Transform, deltaX, deltaY, deltaZ);
            previousMousePos = currentMousePos;
        }

        private void ApplyRotation(System.Windows.Point currentMousePos)
        {
            double dx = currentMousePos.X - previousMousePos.X;
            double dy = currentMousePos.Y - previousMousePos.Y;

            modelTransformer.RotateModel((Transform3DGroup)selectedModel.Transform, dx, dy);
            previousMousePos = currentMousePos;
        }

        private void ApplyScaling(System.Windows.Point currentMousePos)
        {
            double scaleDeltaX = currentMousePos.X - previousMousePos.X;
            double scaleDeltaY = currentMousePos.Y - previousMousePos.Y;
            double scaleDeltaZ = currentMousePos.X - previousMousePos.X;

            modelTransformer.ScaleModel((Transform3DGroup)selectedModel.Transform, scaleDeltaX, scaleDeltaY, scaleDeltaZ);
            previousMousePos = currentMousePos;
        }


        private void Viewport3D_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _isMouseDown = false;
            Mouse.Capture(null);
        }

        private void Model_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                selectedModel = sender as ModelVisual3D;

                if (selectedModel != null)
                {
                    InitializeContextMenu();
                    UpdateTransformFields();
                    SelectShapeInListBox(selectedModel);
                }
            }
            catch (Exception ex)
            {
                debugTextBox.AppendText($"Error on preview mouse left button down: {ex.Message}\n");
            }
        }

        private void ApplyTransform_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ApplyTransform();
            }
            catch (Exception ex)
            {
                debugTextBox.AppendText($"Error on apply transform: {ex.Message}\n");
            }
        }

        private void ApplyTransform()
        {
            if (selectedModel == null) return;
            ApplyTranslation();
           ApplyRotation();
            ApplyScale();

            UpdateTransformFields();
        }
        private void ApplyTranslation()
        {
            if (double.TryParse(PosX.Text, out double posX) &&
               double.TryParse(PosY.Text, out double posY) &&
               double.TryParse(PosZ.Text, out double posZ))
            {
                System.Windows.Media.Media3D.Vector3D currentTranslation = modelTransformer.GetTranslation(_transformGroup);
                modelTransformer.TranslateModel((Transform3DGroup)selectedModel.Transform, posX - currentTranslation.X,
                                                posY - currentTranslation.Y,
                                                posZ - currentTranslation.Z);
            }
        }
        private void ApplyRotation()
        {
            if (double.TryParse(RotX.Text, out double rotX) &&
                double.TryParse(RotY.Text, out double rotY) &&
                double.TryParse(RotZ.Text, out double rotZ))
            {
                Quaternion currentRotation = modelTransformer.GetRotation(_transformGroup);
                modelTransformer.RotateModel((Transform3DGroup)selectedModel.Transform, rotX - currentRotation.Axis.X,
                                             rotY - currentRotation.Axis.Y);
            }
        }
        private void ApplyScale()
        {
            if (double.TryParse(ScaleX.Text, out double scaleX) &&
                double.TryParse(ScaleY.Text, out double scaleY) &&
                double.TryParse(ScaleZ.Text, out double scaleZ))
            {
                System.Windows.Media.Media3D.Vector3D currentScale = modelTransformer.GetScale(_transformGroup);
                modelTransformer.ScaleModel((Transform3DGroup)selectedModel.Transform, scaleX / currentScale.X - 1,
                                            scaleY / currentScale.Y - 1,
                                            scaleZ / currentScale.Z - 1);
            }
        }
        private void UpdateTranslationFields()
        {
            var translation = modelTransformer.GetTranslation(_transformGroup);
            PosX.Text = translation.X.ToString();
            PosY.Text = translation.Y.ToString();
            PosZ.Text = translation.Z.ToString();
        }
        private void UpdateRotationFields()
        {
            Quaternion rotation = modelTransformer.GetRotation(_transformGroup);
            RotX.Text = rotation.Axis.X.ToString();
            RotY.Text = rotation.Axis.Y.ToString();
            RotZ.Text = rotation.Axis.Z.ToString();
        }
        private void UpdateScaleFields()
        {
            System.Windows.Media.Media3D.Vector3D scale = modelTransformer.GetScale(_transformGroup);
            ScaleX.Text = scale.X.ToString();
            ScaleY.Text = scale.Y.ToString();
            ScaleZ.Text = scale.Z.ToString();
        }
        private void UpdateFigureInfo()
        {
            var figureInfo = figuresInfo.FirstOrDefault(f => f.ModelVisual == selectedModel);
            if (figureInfo == null) return;

            var translation = modelTransformer.GetTranslation(_transformGroup);
            var rotation = modelTransformer.GetRotation(_transformGroup);
            var scale = modelTransformer.GetScale(_transformGroup);

            figureInfo.Position = new Point3D(translation.X, translation.Y, translation.Z);
            figureInfo.Rotation = new System.Windows.Media.Media3D.Vector3D(rotation.Axis.X, rotation.Axis.Y, rotation.Axis.Z);
            figureInfo.Scale = new System.Windows.Media.Media3D.Vector3D(scale.X, scale.Y, scale.Z);

            PosX.Text = figureInfo.Position.X.ToString();
            PosY.Text = figureInfo.Position.Y.ToString();
            PosZ.Text = figureInfo.Position.Z.ToString();

            RotX.Text = figureInfo.Rotation.X.ToString();
            RotY.Text = figureInfo.Rotation.Y.ToString();
            RotZ.Text = figureInfo.Rotation.Z.ToString();

            ScaleX.Text = figureInfo.Scale.X.ToString();
            ScaleY.Text = figureInfo.Scale.Y.ToString();
            ScaleZ.Text = figureInfo.Scale.Z.ToString();

            UpdateFigureMaterial(figureInfo);
        }
        private void UpdateFigureMaterial(FigureInfo figureInfo)
        {
            colorPicker.SelectedColor = figureInfo.Color;

            if (figureInfo.CustomMaterial is MyDiffuseMaterial diffuseMaterial)
            {
                diffuseMaterial.Color = figureInfo.Color;
            }
            else if (figureInfo.CustomMaterial is MySpecularMaterial specularMaterial)
            {
                specularMaterial.Color = figureInfo.Color;
            }

            if (selectedModel.Content is GeometryModel3D geometryModel)
            {
                geometryModel.Material = figureInfo.CustomMaterial.GetMaterial();
            }
        }
        private void UpdateTransformFields()
        {
            if (selectedModel == null || !(selectedModel.Transform is Transform3DGroup transformGroup)) return;


            _transformGroup = transformGroup;

            UpdateTranslationFields();
            UpdateRotationFields();
            UpdateScaleFields();
            UpdateFigureInfo();
        }


        private void ShapesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (shapesListBox.SelectedItem != null)
            {
                ListBoxItem selectedItem = shapesListBox.SelectedItem as ListBoxItem;
                selectedModel = selectedItem?.Tag as ModelVisual3D;

                ShapeTypeTextBox.Text = selectedItem?.Content.ToString() ?? string.Empty;
                UpdateTransformFields();
            }
        }
           
        private void SelectShapeInListBox(ModelVisual3D model)
        {
            foreach (ListBoxItem item in shapesListBox.Items)
            {
                if (item.Tag == model)
                {
                    shapesListBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (selectedModel != null)
                {
                   
                    bool removedFromViewport = viewport3d.Children.Remove(selectedModel);
                    debugTextBox.AppendText($"Removed from viewport: {removedFromViewport}\n");

                    ListBoxItem itemToRemove = null;
                    foreach (ListBoxItem item in shapesListBox.Items)
                    {
                        if (item.Tag == selectedModel)
                        {
                            itemToRemove = item;
                            break;
                        }
                    }
                    if (itemToRemove != null)
                    {
                        shapesListBox.Items.Remove(itemToRemove);
                        debugTextBox.AppendText($"Removed from shapes list box: {itemToRemove.Content}\n");
                    }
                    else
                    {
                        debugTextBox.AppendText("Item to remove not found in shapes list box.\n");
                    }

                    
                    var figureInfoToRemove = figuresInfo.FirstOrDefault(f => f.ModelVisual == selectedModel);
                    if (figureInfoToRemove != null)
                    {
                        figuresInfo.Remove(figureInfoToRemove);
                        debugTextBox.AppendText($"Removed from figures info: {figureInfoToRemove.Name}\n");
                    }
                    else
                    {
                        debugTextBox.AppendText("Figure info to remove not found.\n");
                    }

                    selectedModel = null;

                    ClearTransformFields();
                }
            }
            catch (Exception ex)
            {
                debugTextBox.AppendText($"Error on delete: {ex.Message}\n");
            }
        }

        private void ClearTransformFields()
        {
            ShapeTypeTextBox.Text = string.Empty;
            PosX.Text = PosY.Text = PosZ.Text = string.Empty;
            RotX.Text = RotY.Text = RotZ.Text = string.Empty;
            ScaleX.Text = ScaleY.Text = ScaleZ.Text = string.Empty;
            colorPicker.SelectedColor = System.Windows.Media.Colors.White;
            shapesListBox.SelectedItem = null;
        }

        private MyMaterial GetSelectedMaterial(System.Windows.Media.Color color)
        {
            var selectedMaterialType = ((ComboBoxItem)materialTypeComboBox.SelectedItem)?.Content.ToString();
            MyMaterial material;
            double defaultShininess = 50;
            double defaultSpecularIntensity = 0.5;
            double defaultReflectionIntensity = 0.5;

            double shininess = double.TryParse(shininessTextBox.Text, out double shininessValue) ? shininessValue : defaultShininess;
            double specularIntensity = double.TryParse(specularIntensityTextBox.Text, out double specularValue) ? specularValue : defaultSpecularIntensity;
            double reflectionIntensity = double.TryParse(reflectionIntensityTextBox.Text, out double reflectionValue) ? reflectionValue : defaultReflectionIntensity;

            switch (selectedMaterialType)
            {
                case "Specular":
                    material = new MySpecularMaterial(color, shininess, specularIntensity, reflectionIntensity);
                    break;
                case "Diffuse":
                default:
                    material = new MyDiffuseMaterial(color);
                    break;
            }

            return material;
        }

        private void ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e)
        {
            try
            {
                if (selectedModel != null && e.NewValue.HasValue)
                {
                    System.Windows.Media.Color newColor = e.NewValue.Value;
                    var figureInfo = figuresInfo.FirstOrDefault(f => f.ModelVisual == selectedModel);
                    if (figureInfo != null)
                    {
                        figureInfo.Color = newColor;

                        if (figureInfo.CustomMaterial is MyDiffuseMaterial diffuseMaterial)
                        {
                            diffuseMaterial.Color = newColor;
                        }
                        else if (figureInfo.CustomMaterial is MySpecularMaterial specularMaterial)
                        {
                            specularMaterial.Color = newColor;
                        }

                        var geometryModel = selectedModel.Content as GeometryModel3D;
                        geometryModel.Material = figureInfo.CustomMaterial.GetMaterial();

                        debugTextBox.AppendText($"Color changed to: {newColor}\n");
                    }
                }
            }
            catch (Exception ex)
            {
                debugTextBox.AppendText($"Error on color change: {ex.Message}\n");
            }
        }

        private void MaterialTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateMaterialForSelectedModel();
        }

        private void ShininessTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateMaterialForSelectedModel();
        }

        private void SpecularIntensityTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateMaterialForSelectedModel();
        }

        private void ReflectionIntensityTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateMaterialForSelectedModel();
        }

        private void UpdateMaterialForSelectedModel()
        {
            if (selectedModel != null)
            {
                var color = colorPicker.SelectedColor ?? System.Windows.Media.Colors.White;
                var material = GetSelectedMaterial(color);

                var figureInfo = figuresInfo.FirstOrDefault(f => f.ModelVisual == selectedModel);
                if (figureInfo != null)
                {
                    figureInfo.CustomMaterial = material;
                    figureInfo.Color = color;

                    
                    var geometryModel = selectedModel.Content as GeometryModel3D;
                    if (geometryModel != null)
                    {
                        geometryModel.Material = figureInfo.CustomMaterial.GetMaterial();

                        if (figureInfo.FigureObject is Cube cube)
                        {
                            cube.ChangeColor(color); 
                        }

                        debugTextBox.AppendText($"Material updated to: {material.GetType().Name}\n");
                    }
                    else
                    {
                        debugTextBox.AppendText("Geometry model is null\n");
                    }
                }
                else
                {
                    debugTextBox.AppendText("Figure info not found for selected model\n");
                }
            }
        }


    }
}
