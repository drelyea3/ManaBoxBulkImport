using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace TestApp.Controls
{
    public class ManaCostPanel : StackPanel
    {
        private static Dictionary<string, BitmapImage> _imageCache = new();

        public string ManaCostText
        {
            get { return (string)GetValue(ManaCostTextProperty); }
            set { SetValue(ManaCostTextProperty, value); }
        }

        public static readonly DependencyProperty ManaCostTextProperty =
            DependencyProperty.Register(
                nameof(ManaCostText), 
                typeof(string), 
                typeof(ManaCostPanel), 
                new PropertyMetadata(OnManaCostTextChanged));
        private static void OnManaCostTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ManaCostPanel panel)
            {
                panel.Children.Clear();

                foreach (var child in panel.GetChildImages(e.NewValue.ToString()))
                {
                    panel.Children.Add(child);
                }
            }
        }

        private IEnumerable<FrameworkElement> GetChildImages(string? manaCost)
        {
            var manaTypes = manaCost?.Split(['{', '}', ' '], StringSplitOptions.RemoveEmptyEntries);
            
            if (manaTypes != null)
            {
                foreach (var manaType in manaTypes)
                {
                    if (manaType == "//")
                    {
                        yield return new TextBlock() { Text = "/" };
                        continue;
                    }

                    var fixedManaType = manaType.Replace("/", "");
                    if (!_imageCache.TryGetValue(fixedManaType, out var bmp))
                    {
                        var uri = new Uri($"pack://application:,,,/Images/sym/{fixedManaType}.png");
                        bmp = new BitmapImage(uri);
                        _imageCache[fixedManaType] = bmp;
                    }

                    yield return new Image() 
                    { 
                        Source = bmp,
                        Margin = new Thickness(2,0, 2, 0),
                        MaxHeight = this.MaxHeight, 
                        Stretch=System.Windows.Media.Stretch.Uniform,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                }
            }

            yield break;
        }
    }
}
