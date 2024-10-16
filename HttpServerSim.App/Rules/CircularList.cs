namespace HttpServerSim.App.Rules;

public class CircularList<T>
{
    private readonly List<T> _list;
    private int _index = 0;
    private readonly object _locker = new();

    public CircularList(List<T> list)
    {
        if (list.Count == 0)
        {
            throw new Exception("List cannot be empty");
        }

        _list = [.. list];
    }

    public T Next()
    {
        lock (_locker)
        {
            var next = _list[_index];
            _index++;
            if (_index == _list.Count)
            {
                _index = 0;
            }

            return next;
        }
    }

    public void Reset()
    {
        lock (_locker)
        {
            _index = 0;
        }
    }
}
