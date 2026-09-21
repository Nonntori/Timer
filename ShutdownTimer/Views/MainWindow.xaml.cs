using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using ShutdownTimer.ViewModels;

namespace ShutdownTimer.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        
        var viewModel = App.Current.Services.GetService<MainViewModel>();
        if (viewModel == null)
        {
            viewModel = new MainViewModel(
                new Services.ShutdownService(),
                new Services.TimerService(),
                new Services.NotificationService(new Services.SettingsService()),
                new Services.SettingsService(),
                new Services.StartupService());
        }
        
        _viewModel = viewModel;
        DataContext = _viewModel;
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            // Double click - maximize/restore
            if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Maximized;
            }
            else
            {
                WindowState = WindowState.Normal;
            }
        }
        else
        {
            DragMove();
        }
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.IsRunning)
        {
            var result = MessageBox.Show(
                "Таймер продолжит работать в фоновом режиме.\n\nВы хотите отменить таймер и выйти?",
                "Таймер активен",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                _viewModel.CancelTimerCommand.Execute(null);
                Close();
            }
            else if (result == MessageBoxResult.No)
            {
                WindowState = WindowState.Minimized;
            }
            // Cancel - do nothing
        }
        else
        {
            Close();
        }
    }
}
