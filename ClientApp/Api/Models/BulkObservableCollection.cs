using System.Collections.Specialized;
using System.Windows.Data;

namespace MediaCast.Models;

public class BulkObservableCollection<T> : ObservableCollection<T>
{
    private bool _suppressCollectionChangedNotification;

    public override event NotifyCollectionChangedEventHandler CollectionChanged;

    public BulkObservableCollection() : this(Enumerable.Empty<T>())
    {
    }

    public BulkObservableCollection(IEnumerable<T> collection) : base(collection)
    {
    }

    public void RemoveRange(IEnumerable<T> collection)
    {
        var _collection = collection ?? throw new ArgumentNullException(nameof(collection));

        try
        {
            _suppressCollectionChangedNotification = true;

            foreach (var item in _collection)
            {
                this.Remove(item);
            }
        }
        finally
        {
            _suppressCollectionChangedNotification = false;
        }

        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, _collection));
    }

    public void AddRange(IEnumerable<T> collection)
    {
        var _collection = collection ?? throw new ArgumentNullException(nameof(collection));

        try
        {
            _suppressCollectionChangedNotification = true;

            foreach (var item in _collection)
            {
                this.Add(item);
            }
        }
        finally
        {
            _suppressCollectionChangedNotification = false;
        }

        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, _collection));
    }

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        if (!_suppressCollectionChangedNotification)
        {
            base.OnCollectionChanged(e);

            var handlers = CollectionChanged;

            if (handlers != null)
            {
                foreach (NotifyCollectionChangedEventHandler handler in handlers.GetInvocationList())
                {
                    if (handler.Target is CollectionView)
                    {
                        ((CollectionView)handler.Target).Refresh();
                    }
                    else
                    {
                        handler(this, e);
                    }
                }
            }
        }
    }
}