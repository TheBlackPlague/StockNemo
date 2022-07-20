using System;

namespace Terminal.Interactive;

public class Option
{

    public readonly string Text;
    public readonly Action OnClick;

    public Option(string text, Action onClick)
    {
        Text = text;
        OnClick = onClick;
    }

}