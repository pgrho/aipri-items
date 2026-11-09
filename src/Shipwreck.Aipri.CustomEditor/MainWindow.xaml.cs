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

    private void PasteCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        if (!e.Handled
            && sender is DataGrid dg
            && dg.CurrentCell is var cc
            && cc.Item != null
            && cc.Column != null
            && Clipboard.ContainsText())
        {
            var t = Clipboard.GetText()
                    .Split(Environment.NewLine)
                    .Select(l => l.Split('\t'))
                    .Reverse()
                    .SkipWhile(e => e.All(c => string.IsNullOrEmpty(c)))
                    .Reverse()
                    .ToArray();

            var fr = dg.ItemContainerGenerator.IndexFromContainer(dg.ItemContainerGenerator.ContainerFromItem(cc.Item));
            var fc = dg.Columns.IndexOf(cc.Column);
            if (t.Length > 0 && fr >= 0 && fc >= 0)
            {
                var mc = t.Max(e => e.Length);

                for (var ci = 0; ci < mc; ci++)
                {
                    if (fc + ci >= dg.Columns.Count)
                    {
                        continue;
                    }

                    var col = dg.Columns[fc + ci];

                    if (col.IsReadOnly)
                    {
                        continue;
                    }

                    Action<object, string> setter = null;
                    if (col is DataGridTextColumn tc)
                    {
                        setter = (item, v) => ((TextBox)tc.GetCellContent(item)).Text = v;
                    }
                    else if (col is DataGridCheckBoxColumn cbc)
                    {
                        setter = (item, v) =>
                        {
                            if (bool.TryParse(v, out var b))
                            {
                                ((CheckBox)cbc.GetCellContent(item)).IsChecked = b;
                            }
                        };
                    }
                    else if (col is DataGridComboBoxColumn cmc)
                    {
                        setter = (item, v) =>
                        {
                            var combo = (ComboBox)cmc.GetCellContent(item);
                            if (combo.IsEditable)
                            {
                                combo.Text = v;
                            }
                        };
                    }

                    if (setter != null)
                    {
                        for (var ri = 0; ri < t.Length; ri++)
                        {
                            if (dg.ItemContainerGenerator.ContainerFromIndex(fr + ri) is DataGridRow row)
                            {
                                if (col.GetCellContent(row.DataContext).Parent is DataGridCell cell)
                                {
                                    dg.CurrentCell = new DataGridCellInfo(cell);
                                    dg.BeginEdit();
                                    setter(row.DataContext, t[ri].ElementAtOrDefault(ci) ?? string.Empty);
                                    dg.EndEdit();
                                }

                            }

                        }
                    }
                }

                e.Handled = true;
            }
        }
    }
}
