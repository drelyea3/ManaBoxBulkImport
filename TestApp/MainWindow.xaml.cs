using Microsoft.Extensions.DependencyInjection;
using Scryfall;
using Scryfall.Models;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
            //Debug.WriteLine(cardDefinition.Name + " " + cardDefinition.ManaCost);
            //GetManaCost(cardDefinition.ManaCost, out int colorLess, out int red, out int blue, out int green, out int black, out int white, out int total);
            //e.Accepted = green + colorLess == total && green > 0;

            e.Accepted = true;
        }
    }

    private void GetManaCost(string manaCost, out int colorLess, out int red, out int blue, out int green, out int black, out int white, out int total)
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