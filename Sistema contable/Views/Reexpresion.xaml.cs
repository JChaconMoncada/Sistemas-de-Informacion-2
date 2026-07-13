using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using Sistema_contable.ViewModels;
using SistemaContableZulay.UI.Domain;
using Sistema_contable.Models;

namespace Sistema_contable.Views
{
    public partial class Reexpresion : Page
    {
        public Reexpresion()
        {
            InitializeComponent();
            DataContext = new ReexpresionViewModel();
        }

        private void GridPartidas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is DependencyObject depObj)
            {
                // Dejar que el ToggleButton maneje su propio click para expandir
                var btn = FindAncestor<ToggleButton>(depObj);
                if (btn != null) return;

                // Si hizo click en los detalles de la fila (ej. botones de historial), no hacer toggle de la fila
                var details = FindAncestor<DataGridDetailsPresenter>(depObj);
                if (details != null) return;
                
                var row = FindAncestor<DataGridRow>(depObj);
                if (row != null && row.Item is PartidaReexpresion partida)
                {
                    partida.Aplicar = !partida.Aplicar;
                    
                    if (DataContext is ReexpresionViewModel vm)
                    {
                        vm.PartidaSeleccionada = partida;
                    }
                    
                    // Prevenir que el DataGrid haga su selección nativa y expanda detalles automáticamente
                    e.Handled = true; 
                }
            }
        }

        private void BtnExpandir_Click(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton btn)
            {
                var row = FindAncestor<DataGridRow>(btn);
                if (row != null)
                {
                    row.DetailsVisibility = btn.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        private static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            do
            {
                if (current is T)
                {
                    return (T)current;
                }
                current = System.Windows.Media.VisualTreeHelper.GetParent(current);
            }
            while (current != null);
            return null;
        }
    }
}
