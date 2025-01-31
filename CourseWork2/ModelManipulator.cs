using System;
using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace CourseWork2
{
    public class ModelController
    {
        private ModelVisual3D selectedModel;
        private Transform3DGroup _transformGroup;
        private readonly TextBox debugTextBox;

        public ModelController(TextBox debugTextBox)
        {
            this.debugTextBox = debugTextBox;
        }

        public void SelectModel(ModelVisual3D model)
        {
            selectedModel = model;

            if (selectedModel.Transform is Transform3DGroup existingTransformGroup)
            {
                _transformGroup = existingTransformGroup;
            }
            else
            {
                InitializeTransformGroup();
            }
        }

        private void InitializeTransformGroup()
        {
            _transformGroup = new Transform3DGroup();
            selectedModel.Transform = _transformGroup;
        }

        public void ApplyTranslation(double deltaX, double deltaY, double deltaZ)
        {
            try
            {
                if (selectedModel == null) return;

                var translation = new TranslateTransform3D(deltaX, deltaY, deltaZ);
                _transformGroup.Children.Add(translation);
            }
            catch (Exception ex)
            {
                debugTextBox.AppendText($"Error in translation: {ex.Message}\n");
            }
        }

        public void ApplyRotation(double angleX, double angleY)
        {
            try
            {
                if (selectedModel == null) return;

                var rotationX = new AxisAngleRotation3D(new Vector3D(1, 0, 0), angleX);
                var rotationY = new AxisAngleRotation3D(new Vector3D(0, 1, 0), angleY);

                _transformGroup.Children.Add(new RotateTransform3D(rotationX));
                _transformGroup.Children.Add(new RotateTransform3D(rotationY));
            }
            catch (Exception ex)
            {
                debugTextBox.AppendText($"Error in rotation: {ex.Message}\n");
            }
        }

        public void ApplyScaling(double scaleX, double scaleY, double scaleZ)
        {
            try
            {
                if (selectedModel == null) return;

                var scaling = new ScaleTransform3D(scaleX, scaleY, scaleZ);
                _transformGroup.Children.Add(scaling);
            }
            catch (Exception ex)
            {
                debugTextBox.AppendText($"Error in scaling: {ex.Message}\n");
            }
        }
    }
}
