namespace Shipwreck.Aipri.CustomEditor;

internal sealed class CustomTextColumn : DataGridTextColumn
{
    public Binding? IsChangedBinding { get; set; }
    protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
    {
        var b = IsChangedBinding;

        if (b != null)
        {
            var bs1 = DataGridOwner.FindResource("MahApps.Styles.TextBlock.DataGrid") as Style;

            if (bs1 != null)
            {
                ElementStyle = CustomComboBoxColumn.AddTrigger(b, ElementStyle, bs1);
            }
        }

        return base.GenerateElement(cell, dataItem);
    }

    protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
    {
        var b = IsChangedBinding;

        if (b != null)
        {
            var bs1 = DataGridOwner.FindResource("MahApps.Styles.TextBox.DataGrid.Editing") as Style;

            if (bs1 != null)
            {
                EditingElementStyle = CustomComboBoxColumn.AddTrigger(b, EditingElementStyle, bs1);
            }
        }

        return base.GenerateEditingElement(cell, dataItem);
    }

}
