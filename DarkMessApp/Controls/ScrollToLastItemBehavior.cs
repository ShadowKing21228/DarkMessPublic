using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

namespace DarkMessApp.Controls;

public class ScrollToLastItemBehavior : Behavior<CollectionView>
{
    private INotifyCollectionChanged _notifyCollection;
    private CollectionView _collectionView;

    protected override void OnAttachedTo(CollectionView bindable)
    {
        base.OnAttachedTo(bindable);
        _collectionView = bindable;
        bindable.PropertyChanged += OnCollectionViewPropertyChanged;
    }

    protected override void OnDetachingFrom(CollectionView bindable)
    {
        base.OnDetachingFrom(bindable);
        bindable.PropertyChanged -= OnCollectionViewPropertyChanged;
        UnsubscribeFromCollection();
    }

    private void OnCollectionViewPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "ItemsSource")
        {
            UnsubscribeFromCollection();
            
            if (_collectionView.ItemsSource is INotifyCollectionChanged newCollection)
            {
                _notifyCollection = newCollection;
                _notifyCollection.CollectionChanged += OnCollectionChanged;
            }
        }
    }

    private void UnsubscribeFromCollection()
    {
        if (_notifyCollection != null)
        {
            _notifyCollection.CollectionChanged -= OnCollectionChanged;
            _notifyCollection = null;
        }
    }

    private async void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add && _collectionView.ItemsSource is IList items)
        {
            await Task.Delay(100); // Даем время на рендеринг
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                _collectionView.ScrollTo(items[items.Count - 1], animate: true, position: ScrollToPosition.End);
            });
        }
    }
}