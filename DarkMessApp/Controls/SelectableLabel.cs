using System.Text.RegularExpressions;

namespace DarkMessApp.Controls;

public class SelectableLabel : Label
{
    public static readonly BindableProperty DetectLinksProperty =
        BindableProperty.Create(
            nameof(DetectLinks), 
            typeof(bool), 
            typeof(SelectableLabel), 
            false,
            propertyChanged: OnDetectLinksChanged);

    public static readonly BindableProperty LinkColorProperty =
        BindableProperty.Create(
            nameof(LinkColor), 
            typeof(Color), 
            typeof(SelectableLabel), 
            Colors.Blue,
            propertyChanged: OnLinkColorChanged);

    public bool DetectLinks
    {
        get => (bool)GetValue(DetectLinksProperty);
        set => SetValue(DetectLinksProperty, value);
    }

    public Color LinkColor
    {
        get => (Color)GetValue(LinkColorProperty);
        set => SetValue(LinkColorProperty, value);
    }

    public event EventHandler<string>? LinkClicked;

    public SelectableLabel()
    {
        // Инициализация при создании
        UpdateFormattedText();
    }

    private static void OnDetectLinksChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SelectableLabel label && label.DetectLinks)
        {
            label.UpdateFormattedText();
        }
    }

    private static void OnLinkColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SelectableLabel label && label.DetectLinks)
        {
            label.UpdateFormattedText();
        }
    }

    protected override void OnPropertyChanged(string propertyName = null)
    {
        base.OnPropertyChanged(propertyName);

        if (propertyName == TextProperty.PropertyName && DetectLinks)
        {
            UpdateFormattedText();
        }
    }

    private void UpdateFormattedText()
    {
        if (!DetectLinks || string.IsNullOrEmpty(Text)) {
            FormattedText = null;
            return;
        }

        var formattedString = new FormattedString();
        var parts = Regex.Split(Text, @"(https?://[^\s]+)");

        foreach (var part in parts)
        {
            if (Uri.TryCreate(part, UriKind.Absolute, out var uri) && 
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                var span = new Span 
                { 
                    Text = part,
                    TextColor = LinkColor,
                    TextDecorations = TextDecorations.Underline
                };

                span.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new Command(() => LinkClicked?.Invoke(this, part))
                });

                formattedString.Spans.Add(span);
            }
            else
            {
                formattedString.Spans.Add(new Span { Text = part });
            }
        }

        FormattedText = formattedString;
    }
}