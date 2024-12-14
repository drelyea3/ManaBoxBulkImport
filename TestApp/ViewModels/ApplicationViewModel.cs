using Microsoft.Extensions.DependencyInjection;
using Scryfall;
using Scryfall.Models;
using System.Windows.Data;

namespace TestApp.ViewModels;

public class ApplicationViewModel : ViewModelBase
{
    private CardSet? _selectedSetDefinition;
    private IEnumerable<CardDefinition>? _cardDefinitions;
    private ColorFilter _colorFilter;

    public ColorFilter ColorFilter
    {
        get => _colorFilter;
        set => SetField(ref _colorFilter, value);
    }
    public CollectionViewSource SetDefinitions { get; } = new();
    public IEnumerable<CardDefinition>? CardDefinitions { get => _cardDefinitions; set => SetField(ref _cardDefinitions , value); }

    public CardSet? SelectedSetDefinition
    {
        get => _selectedSetDefinition;
        set 
        { 
            if (SetField(ref _selectedSetDefinition, value))
            {
                UpdateCardSet();
            }
        }
    }

    public ApplicationViewModel()
    {
        _colorFilter = new ColorFilter();       
    }
    private async void UpdateCardSet()
    {
        CardDefinitions = [];

        if (SelectedSetDefinition != null)
        {
            var client = Services.Provider.GetRequiredService<ScryfallClient>();
            CardDefinitions = await client.GetCards(SelectedSetDefinition);
        }
    }
}
