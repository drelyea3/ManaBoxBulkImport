
namespace TestApp.ViewModels;

[Flags]
public enum ColorFlags
{
    None = 0,
    Colorless = 1 << 0,
    Red = 1 << 1,
    Green = 1 << 2,
    Blue = 1 << 3,
    White = 1 << 4,
    Black = 1 << 5,
}


public class ColorFilter : ViewModelBase
{
    private ColorFlags _colorFlags = ColorFlags.None;
    public ColorFlags Colors => _colorFlags;

    public bool IsColorless
    {
        get => Colors.HasFlag(ColorFlags.Colorless);
        set
        {
            if (SetFlag(ColorFlags.Colorless, value))
            {
                OnPropertyChanged();
            }
        }
    }

    public bool IsRed
    {
        get => Colors.HasFlag(ColorFlags.Red);
        set
        {
            if (SetFlag(ColorFlags.Red, value))
            {
                OnPropertyChanged();
            }
        }
    }

    public bool IsGreen
    {
        get => Colors.HasFlag(ColorFlags.Green);
        set
        {
            if (SetFlag(ColorFlags.Green, value))
            {
                OnPropertyChanged();
            }
        }
    }

    public bool IsBlue
    {
        get => Colors.HasFlag(ColorFlags.Blue);
        set
        {
            if (SetFlag(ColorFlags.Blue, value))
            {
                OnPropertyChanged();
            }
        }
    }

    public bool IsWhite
    {
        get => Colors.HasFlag(ColorFlags.White);
        set
        {
            if (SetFlag(ColorFlags.White, value))
            {
                OnPropertyChanged();
            }
        }
    }

    public bool IsBlack
    {
        get => Colors.HasFlag(ColorFlags.Black);
        set
        {
            if (SetFlag(ColorFlags.Black, value))
            {
                OnPropertyChanged();
            }
        }
    }

    public bool All(ColorFlags colors)
    {
        if (Colors == ColorFlags.None)
        {
            return true;
        }

        return (colors & Colors) == colors;
    }

    public bool Any(ColorFlags colors)
    {
        if (Colors == ColorFlags.None)
        {
            return true;
        }

        return (colors & Colors) != 0;
    }

    public bool None(ColorFlags colors)
    {
        if (Colors == ColorFlags.None)
        {
            return true;
        }

        return (colors & Colors) == 0;
    }

    public bool Only(ColorFlags colors)
    {
        if (Colors == ColorFlags.None)
        {
            return true;
        }

        return Any(colors) && None(colors);
    }

    private bool SetFlag(ColorFlags colorFlags, bool value)
    {
        var oldFlags = _colorFlags;

        if (value)
        {
            _colorFlags |= colorFlags;
        }
        else
        {
            _colorFlags &= ~colorFlags;
        }

        return oldFlags != _colorFlags;
    }
}
