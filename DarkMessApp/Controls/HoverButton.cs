namespace DarkMessApp.Controls;

public class HoverButton : Button
{
    public static readonly BindableProperty HoverColorProperty =
        BindableProperty.Create(
            nameof(HoverColor),
            typeof(Color),
            typeof(HoverButton),
            null);

    public Color HoverColor
    {
        get => (Color)GetValue(HoverColorProperty);
        set => SetValue(HoverColorProperty, value);
    }

    private Color _defaultColor;
    private bool _isHovered;

    public HoverButton()
    {
        Loaded += OnLoaded;
        HandlerChanged += OnHandlerChanged;
    }

    private void OnLoaded(object sender, EventArgs e)
    {
        // Сохраняем исходный цвет только при первой загрузке
        _defaultColor ??= BackgroundColor;
    }

    private void OnHandlerChanged(object sender, EventArgs e)
    {
#if WINDOWS
        if (Handler?.PlatformView is Microsoft.UI.Xaml.Controls.Button nativeButton)
        {
            // Удаляем старые подписки
            nativeButton.PointerEntered -= OnPointerEntered;
            nativeButton.PointerExited -= OnPointerExited;
            
            // Добавляем новые
            nativeButton.PointerEntered += OnPointerEntered;
            nativeButton.PointerExited += OnPointerExited;
        }
#endif
    }

#if WINDOWS
    private void OnPointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (_isHovered) return;
        _isHovered = true;
        
        Dispatcher.Dispatch(() => 
        {
            if (HoverColor != null)
            {
                BackgroundColor = HoverColor;
            }
        });
    }

    private void OnPointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (!_isHovered) return;
        _isHovered = false;
        
        Dispatcher.Dispatch(() => 
        {
            BackgroundColor = _defaultColor;
        });
    }
#endif

    protected override void OnParentChanged()
    {
        base.OnParentChanged();
        if (Parent != null) return;
#if WINDOWS
            if (Handler?.PlatformView is Microsoft.UI.Xaml.Controls.Button nativeButton)
            {
                nativeButton.PointerEntered -= OnPointerEntered;
                nativeButton.PointerExited -= OnPointerExited;
            }
#endif
        Loaded -= OnLoaded;
        HandlerChanged -= OnHandlerChanged;
    }
}