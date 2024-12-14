using Microsoft.Extensions.DependencyInjection;
using Scryfall;
using Scryfall.Models;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TestApp.ViewModels;

namespace TestApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private ApplicationViewModel _viewModel;
    private ScryfallClient _scryfallClient;
    private Popup? _popup;

    public MainWindow()
    {
        InitializeComponent();

        CommandManager.RegisterClassCommandBinding(typeof(MainWindow), new CommandBinding(Commands.ViewImage, OnExecutedViewImage));

        _scryfallClient = Services.Provider.GetRequiredService<ScryfallClient>();
        _viewModel = Services.Provider.GetRequiredService<ApplicationViewModel>();

        DataContext = _viewModel;
        _viewModel.ColorFilter.PropertyChanged += OnColorFilterChanged;
        _viewModel.SetDefinitions.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));

        Task.Run(async () =>
        {
            var sets = await _scryfallClient.GetSetsAsync();
            _ = Dispatcher.BeginInvoke(() =>
            {
                _viewModel.SetDefinitions.Source = sets?.Values.Where(set => set.ReleasedAt.Year >= 2020);
            });
        });

        PreviewKeyDown += MainWindow_PreviewKeyDown;
    }

    private void OnColorFilterChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (Resources["CardDefinitionsViewSource"] is CollectionViewSource cvs)
        {
            cvs.View.Refresh();
        }
    }

    private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (_popup != null)
        {
            _popup.IsOpen = false;
            e.Handled = true;
        }
    }

    private void OnExecutedViewImage(object sender, ExecutedRoutedEventArgs e)
    {
        if (e.Parameter is CardDefinition cardDefinition)
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, () =>
            {
                var popup = new Popup() { StaysOpen = false, PlacementTarget = this, Placement = PlacementMode.Center, AllowsTransparency = true };
                _popup = popup;
                popup.Closed += Popup_Closed;
                var container = new StackPanel() { Orientation = Orientation.Horizontal };
                popup.Child = container;

                foreach (var def in _viewModel.CardDefinitions.Where(cd => cd.OracleId == cardDefinition.OracleId))
                {
                    var image = new Image() { Source = new BitmapImage(def.ImageUris.Normal), Stretch = Stretch.Uniform, Margin = new Thickness(4, 0, 4, 0) };
                    container.Children.Add(image);
                }

                popup.IsOpen = true;
            });
        }
    }

    private void Popup_Closed(object? sender, EventArgs e)
    {
        _popup = null;
    }

    private void CardDefinitionsFilter(object sender, System.Windows.Data.FilterEventArgs e)
    {
        if (e.Item is CardDefinition cardDefinition)
        {
            //GetManaCost(cardDefinition.ManaCost, out int colorLess, out int red, out int blue, out int green, out int white, out int black, out int total);
            var flags = GetColorFlags(cardDefinition.ManaCost);
            //if (flags == ColorFlags.None)
            //{
            //    e.Accepted = true;// _viewModel.ColorFilter.Colors == ColorFlags.None;
            //}
            //else
            {
                //Debug.WriteLine($"Comparing {flags} to filter {_viewModel.ColorFilter.Colors} Any {_viewModel.ColorFilter.Any(flags)} All {_viewModel.ColorFilter.All(flags)} None {_viewModel.ColorFilter.None(flags)}");
                //Debug.WriteLine($"Cost {cardDefinition.ManaCost} Colors {string.Join(", ", cardDefinition.Colors)} Identity {string.Join(", ", cardDefinition.ColorIdentity)} Produced {string.Join(", ", cardDefinition.ProducedMana)}");
                e.Accepted = _viewModel.ColorFilter.All(flags);
            }
        }
    }

    private ColorFlags GetColorFlags(string manaCost)
    {
        ColorFlags colorFlags = ColorFlags.None;

        foreach (var c in manaCost.ToUpper())
        {
            switch (c)
            {
                case 'R':
                    colorFlags |= ColorFlags.Red;
                    break;
                case 'G':
                    colorFlags |= ColorFlags.Green;
                    break;
                case 'U':
                    colorFlags |= ColorFlags.Blue;
                    break;
                case 'W':
                    colorFlags |= ColorFlags.White;
                    break;
                case 'B':
                    colorFlags |= ColorFlags.Black;
                    break;
                case 'X':
                    colorFlags |= ColorFlags.Colorless;
                    break;
                default:
                    if (char.IsDigit(c))
                    {
                        colorFlags |= ColorFlags.Colorless;
                    }
                    break;
            }
        }

        return colorFlags;
    }

    private void GetManaCost(string manaCost, out int colorLess, out int red, out int blue, out int green, out int white, out int black, out int total)
    {
        var parts = manaCost?.Split([',', '{', '}'], StringSplitOptions.RemoveEmptyEntries) ?? [];

        colorLess = red = blue = green = black = white = total = 0;

        foreach (var part in parts)
        {
            switch (part)
            {
                case "R":
                    ++red;
                    break;
                case "G":
                    ++green;
                    break;
                case "U":
                    ++blue;
                    break;
                case "B":
                    ++black;
                    break;
                case "W":
                    ++white;
                    break;
                default:
                    bool success = int.TryParse(part, out var n);
                    if (success)
                    {
                        colorLess = n;
                    }
                    break;
            }
        }

        total = colorLess + red + blue + green + black + white;
    }
}