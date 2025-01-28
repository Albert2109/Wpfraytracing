using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Controls;
using System.Linq;

namespace Figures
{
    public class Nav
    {
        public Transform3DGroup CreateTransformGroup()
        {
            Transform3DGroup transformGroup = new Transform3DGroup();
            transformGroup.Children.Add(new RotateTransform3D(new QuaternionRotation3D(new Quaternion(0, 0, 0, 1))));
            transformGroup.Children.Add(new TranslateTransform3D());
            transformGroup.Children.Add(new ScaleTransform3D(1, 1, 1));
            return transformGroup;
        }

        public Point3D GetHitPosition(Viewport3D viewport3d, Point mousePos)
        {
            Point3D hitPosition = new Point3D();
            PointHitTestParameters hitTestParameters = new PointHitTestParameters(mousePos);
            VisualTreeHelper.HitTest(viewport3d, null, result =>
            {
                RayMeshGeometry3DHitTestResult rayResult = result as RayMeshGeometry3DHitTestResult;
                if (rayResult != null)
                {
                    hitPosition = rayResult.PointHit;
                    return HitTestResultBehavior.Stop;
                }
                return HitTestResultBehavior.Continue;
            }, hitTestParameters);

            return hitPosition;
        }

        public void RotateModel(Transform3DGroup transformGroup, double dx, double dy)
        {
            double rotationSpeed = 0.2;

            Quaternion deltaX = new Quaternion(new Vector3D(0, 1, 0), dx * rotationSpeed);
            Quaternion deltaY = new Quaternion(new Vector3D(1, 0, 0), dy * rotationSpeed);

            RotateTransform3D rotateTransform = transformGroup.Children.OfType<RotateTransform3D>().FirstOrDefault();
            if (rotateTransform != null)
            {
                var quaternionRotation = rotateTransform.Rotation as QuaternionRotation3D;
                quaternionRotation.Quaternion = deltaX * quaternionRotation.Quaternion * deltaY;
            }
        }

        public void ScaleModel(Transform3DGroup transformGroup, double sx, double sy, double sz)
        {
            double scaleSpeed = 0.005;
            ScaleTransform3D scaleTransform = transformGroup.Children.OfType<ScaleTransform3D>().FirstOrDefault();
            if (scaleTransform != null)
            {
                scaleTransform.ScaleX += sx * scaleSpeed;
                scaleTransform.ScaleY += sy * scaleSpeed;
                scaleTransform.ScaleZ += sz * scaleSpeed;
            }
        }

        public void TranslateModel(Transform3DGroup transformGroup, double deltaX, double deltaY, double deltaZ)
        {
            Vector3D translationVector = new Vector3D(deltaX, deltaY, deltaZ);
            RotateTransform3D rotateTransform = transformGroup.Children.OfType<RotateTransform3D>().FirstOrDefault();
            TranslateTransform3D translateTransform = transformGroup.Children.OfType<TranslateTransform3D>().FirstOrDefault();

            if (rotateTransform != null && translateTransform != null)
            {
                Matrix3D rotationMatrix = new Matrix3D();
                rotationMatrix.Rotate(((QuaternionRotation3D)rotateTransform.Rotation).Quaternion);

                Vector3D transformedVector = rotationMatrix.Transform(translationVector);

                translateTransform.OffsetX += transformedVector.X;
                translateTransform.OffsetY += transformedVector.Y;
                translateTransform.OffsetZ += transformedVector.Z;
            }
        }

        public Vector3D GetTranslation(Transform3DGroup transformGroup)
        {
            TranslateTransform3D translateTransform = transformGroup.Children.OfType<TranslateTransform3D>().FirstOrDefault();
            return new Vector3D(translateTransform.OffsetX, translateTransform.OffsetY, translateTransform.OffsetZ);
        }

        public Quaternion GetRotation(Transform3DGroup transformGroup)
        {
            RotateTransform3D rotateTransform = transformGroup.Children.OfType<RotateTransform3D>().FirstOrDefault();
            return ((QuaternionRotation3D)rotateTransform.Rotation).Quaternion;
        }

        public Vector3D GetScale(Transform3DGroup transformGroup)
        {
            ScaleTransform3D scaleTransform = transformGroup.Children.OfType<ScaleTransform3D>().FirstOrDefault();
            return new Vector3D(scaleTransform.ScaleX, scaleTransform.ScaleY, scaleTransform.ScaleZ);
        }
    }
}
