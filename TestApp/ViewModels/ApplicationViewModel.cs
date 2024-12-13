using Microsoft.Extensions.DependencyInjection;
using Scryfall;
using Scryfall.Models;
using System.Windows.Data;

namespace TestApp.ViewModels;

public class ApplicationViewModel : ViewModelBase
{
    private CardSet? _selectedSetDefinition;
    private IEnumerable<CardDefinition>? _cardDefinitions;

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
