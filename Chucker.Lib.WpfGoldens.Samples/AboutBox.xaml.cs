using System;
using System.Windows;

namespace Chucker.Lib.WpfGoldens.Samples;

public partial class AboutBox
{
    public AboutBox()
    {
        InitializeComponent();

        BuildTimestamp.Text = $"Built {DateTime.Now}";
    }
}

