using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace CourseWork2
{
    public class CameraControl
    {
        private PerspectiveCamera mainCamera;
        private Viewport3D viewport3d;
        private double moveSpeed = 1;
        private double rotateSpeed = 1;

        public CameraControl(Viewport3D viewport)
        {
            viewport3d = viewport;
            InitializeCamera();
            InitializeLighting();
        }

        private void InitializeCamera()
        {
            mainCamera = new PerspectiveCamera(new Point3D(0, 0, 10), new Vector3D(0, 0, -1), new Vector3D(0, 1, 0), 45);
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
            directionalLight.Direction = new Vector3D(-1, -1, -1);
            ModelVisual3D directionalLightModel = new ModelVisual3D();
            directionalLightModel.Content = directionalLight;
            viewport3d.Children.Add(directionalLightModel);
        }

        public void MoveCamera(Vector3D direction)
        {
            mainCamera.Position += direction;
        }

        public void RotateCamera(double angle, Vector3D axis)
        {
            axis.Normalize();

            Point3D cameraPosition = mainCamera.Position;
            Vector3D cameraLookDirection = mainCamera.LookDirection;
            Vector3D cameraUpDirection = mainCamera.UpDirection;

            RotateTransform3D rotation = new RotateTransform3D(new AxisAngleRotation3D(axis, angle), cameraPosition);
            cameraLookDirection = rotation.Transform(cameraLookDirection);
            cameraUpDirection = rotation.Transform(cameraUpDirection);

            mainCamera.LookDirection = cameraLookDirection;
            mainCamera.UpDirection = cameraUpDirection;

            mainCamera.Position = cameraPosition + cameraLookDirection;
        }

        public PerspectiveCamera GetCamera()
        {
            return mainCamera;
        }

        public void HandleKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.W:
                    MoveCamera(moveSpeed * mainCamera.LookDirection);
                    break;
                case Key.S:
                    MoveCamera(-moveSpeed * mainCamera.LookDirection);
                    break;
                case Key.A:
                    MoveCamera(-Vector3D.CrossProduct(mainCamera.LookDirection, mainCamera.UpDirection) * moveSpeed);
                    break;
                case Key.D:
                    MoveCamera(Vector3D.CrossProduct(mainCamera.LookDirection, mainCamera.UpDirection) * moveSpeed);
                    break;
                case Key.Left:
                    RotateCamera(rotateSpeed, mainCamera.UpDirection);
                    break;
                case Key.Up:
                    RotateCamera(-rotateSpeed, Vector3D.CrossProduct(mainCamera.UpDirection, mainCamera.LookDirection));
                    break;
                case Key.Down:
                    RotateCamera(rotateSpeed, Vector3D.CrossProduct(mainCamera.UpDirection, mainCamera.LookDirection));
                    break;
                case Key.Right:
                    RotateCamera(-rotateSpeed, mainCamera.UpDirection);
                    break;
                case Key.Space:
                    MoveCamera(new Vector3D(0, moveSpeed, 0));
                    break;
                case Key.LeftShift:
                    MoveCamera(new Vector3D(0, -moveSpeed, 0));
                    break;
            }
        }
    }
}
