using System.Collections.ObjectModel;

namespace Shipwreck.Aipri;

public abstract class DataItemCollection<T> : Collection<T>
    where T : DataItem
{
    private readonly AipriDataSet? _DataSet;

    private protected DataItemCollection(AipriDataSet? dataSet)
    {
        _DataSet = dataSet;
    }

    public void Set(IEnumerable<T>? items)
    {
        if (items == this)
        {
            return;
        }

        Clear();

        if (items != null)
        {
            foreach (var e in items)
            {
                Add(e);
            }
        }
    }

    protected override void ClearItems()
    {
        foreach (var e in this)
        {
            OnRemoving(e);
        }
        base.ClearItems();
    }

    protected override void RemoveItem(int index)
    {
        var e = this[index];

        OnRemoving(e);

        base.RemoveItem(index);
    }

    protected override void InsertItem(int index, T item)
    {
        OnAdding(item);

        base.InsertItem(index, item);
    }

    protected override void SetItem(int index, T item)
    {
        var e = this[index];

        if (e == item)
        {
            return;
        }

        OnRemoving(e);
        OnAdding(item);

        base.SetItem(index, item);
    }

    protected virtual void OnAdding(T item)
    {
        if (item.DataSet != null)
        {
            throw new InvalidOperationException();
        }

        item.DataSet = _DataSet;
    }

    protected virtual void OnRemoving(T e)
    {
        e.DataSet = null;
    }
}