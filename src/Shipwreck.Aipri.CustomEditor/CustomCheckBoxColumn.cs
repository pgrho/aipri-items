namespace Shipwreck.Aipri.CustomEditor;

internal sealed class CustomCheckBoxColumn : DataGridCheckBoxColumn
{
    public Binding? IsChangedBinding { get; set; }

    protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
    {
        var b = IsChangedBinding;

        if (b != null)
        {
            var bs1 = DataGridOwner.FindResource("MahApps.Styles.CheckBox.DataGrid") as Style;

            if (bs1 != null)
            {
                ElementStyle = AddTrigger(b, ElementStyle, bs1);
            }
        }

        return base.GenerateElement(cell, dataItem);
    }

    protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
    {
        var b = IsChangedBinding;

        if (b != null)
        {
            var bs1 = DataGridOwner.FindResource("MahApps.Styles.CheckBox.DataGrid") as Style;

            if (bs1 != null)
            {
                EditingElementStyle = AddTrigger(b, EditingElementStyle, bs1);
            }
        }

        return base.GenerateEditingElement(cell, dataItem);
    }
    internal static Style AddTrigger(Binding binding, Style elementStyle, Style baseStyle)
    {
        if (elementStyle == baseStyle)
        {
            elementStyle = new Style(baseStyle.TargetType, baseStyle);
        }

        if (!elementStyle.Triggers.OfType<DataTrigger>().Any(e => e.Binding == binding && e.Value is bool v && v))
        {
            if (elementStyle.IsSealed)
            {
                elementStyle = new Style(elementStyle.TargetType, elementStyle);
            }

            var dt = new DataTrigger() { Binding = binding, Value = true };
            dt.Setters.Add(new Setter(Control.BackgroundProperty, Brushes.Red));
            elementStyle.Triggers.Add(dt);
        }
        return elementStyle;
    }
}