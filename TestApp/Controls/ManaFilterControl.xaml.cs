using System.Windows;
using System.Windows.Controls;
using TestApp.ViewModels;

namespace TestApp.Controls;

/// <summary>
/// Interaction logic for ManaFilterControl.xaml
/// </summary>
public partial class ManaFilterControl : UserControl
{
    public ColorFilter Colors
    {
        get { return (ColorFilter)GetValue(ColorsProperty); }
        set { SetValue(ColorsProperty, value); }
    }

    public static readonly DependencyProperty ColorsProperty =
        DependencyProperty.Register("Colors", typeof(ColorFilter), typeof(ManaFilterControl));

    public ManaFilterControl()
    {
        InitializeComponent();
    }
}