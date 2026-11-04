using System.Drawing.Drawing2D;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Shipwreck.ViewModelUtils.Controls;

namespace Shipwreck.Aipri.CustomEditor;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel(this);
    }

    private void FrameworkWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        var vm = (MainWindowViewModel)DataContext;
        if (!vm.ConfirmClose())
        {
            e.Cancel = true;
        }
    }

    private void DataGrid_KeyDown(object sender, KeyEventArgs e)
    {
        if (!e.Handled
            && ((Key.D0 <= e.Key && e.Key <= Key.D9) || (Key.NumPad0 <= e.Key && e.Key <= Key.NumPad9))
            && sender is DataGrid dg
            && dg.CurrentCell is DataGridCellInfo ci
            && ci.Column is DataGridComboBoxColumn cbc
            && cbc.ItemsSource is int[] ary)
        {
            var c = e.Key <= Key.D9 ? e.Key - Key.D0 : (e.Key - Key.NumPad0);
            if (Array.IndexOf(ary, c) is var i && i > 0)
            {
                dg.BeginEdit();
                var cb = (ComboBox)cbc.GetCellContent(ci.Item);
                cb.SelectedIndex = i;
                dg.CommitEdit();
                e.Handled = true;
            }
        }
    }
}
