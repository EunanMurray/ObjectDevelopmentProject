using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Project1
{
    public partial class ImageViewer : Window
    {
        private List<ImageSource> _images;
        private int _currentIndex;

        public ImageViewer(List<ImageSource> images)
        {
            InitializeComponent();
            _images = images;
            _currentIndex = 0;
            UpdateImageDisplay();
        }

        private void UpdateImageDisplay()
        {
            // Check to make sure that there is at least one image and index is in bounds
            if (_images != null && _images.Count > 0 && _currentIndex >= 0 && _currentIndex < _images.Count)
            {
                FullSizeImage.Source = _images[_currentIndex];
            }
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentIndex > 0)
            {
                _currentIndex--;
                UpdateImageDisplay();
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentIndex < _images.Count - 1)
            {
                _currentIndex++;
                UpdateImageDisplay();
            }
        }
    }
}
