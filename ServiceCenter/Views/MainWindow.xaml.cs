using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ServiceCenter.Views
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnWindowLoaded;
            SizeChanged += OnWindowSizeChanged;
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            SyncNavigationStateWithWindowWidth();
        }

        private void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            SyncNavigationStateWithWindowWidth();
        }

        private void SyncNavigationStateWithWindowWidth()
        {
            if (DataContext is ViewModels.MainViewModel viewModel)
                viewModel.UpdateWindowWidth(ActualWidth);
        }
    }
}
