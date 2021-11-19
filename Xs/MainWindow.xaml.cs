using System.Windows;

namespace Xs;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public sealed partial class MainWindow : Window
{
    internal static MainWindow? _singleton;

    /// <summary>
    /// Creates a new instance of <see cref="MainWindow"/>.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
        _singleton = this;
        Activated += (_, _) => XsManager.InitMre();
    }

    internal void CreateClient(GimmeDaBlood<XsClient> gdb)
    {
        BrowserWindow w = new()
        {
            Owner = this
        };
        w.Show();
        XsClient client = new(w);
        gdb.Result = client;
    }
}
