using System.Runtime.CompilerServices;

namespace MediaCast.Api;

internal abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    protected virtual void OnPropertyChanged(params string[] propertyNames) => propertyNames?.ForEach(OnPropertyChanged);

    protected virtual void SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (!EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;

            OnPropertyChanged(propertyName);
        }
    }
}

