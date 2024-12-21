using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace TestApp.Controls;

public class ManaPanel : StackPanel
{
    private static Dictionary<string, BitmapImage> _imageCache = new();

    public object Mana
    {
        get { return (string)GetValue(ManaProperty); }
        set { SetValue(ManaProperty, value); }
    }

    public static readonly DependencyProperty ManaProperty =
        DependencyProperty.Register(
            nameof(Mana), 
            typeof(object), 
            typeof(ManaPanel), 
            new PropertyMetadata(OnManaChanged));

    private static void OnManaChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ManaPanel panel)
        {
            panel.Children.Clear();

            var manaTypes = e.NewValue as IEnumerable<string>;

            if (manaTypes is null && panel.Mana is string text)
            {
                manaTypes = text?.Split(['{', '}', ' '], StringSplitOptions.RemoveEmptyEntries);
            }

            if (manaTypes != null)
            { 
                foreach (var child in panel.GetChildImages(manaTypes))
                {
                    panel.Children.Add(child);
                }
            }
        }
    }

    private IEnumerable<FrameworkElement> GetChildImages(IEnumerable<string> manaTypes)
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
                var uri = new Uri($"pack://application:,,,/Images/sym/_{fixedManaType}.png");
                bmp = new BitmapImage(uri);
                _imageCache[fixedManaType] = bmp;
            }

            yield return new Image()
            {
                Source = bmp,
                Margin = new Thickness(2, 0, 2, 0),
                MaxHeight = this.MaxHeight,
                Stretch = System.Windows.Media.Stretch.Uniform,
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        yield break;
    }
}