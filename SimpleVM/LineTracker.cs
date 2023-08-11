using System;
using SimpleVM.Collections;

namespace SimpleVM;

public class LineTracker : IDisposable
{
    private UnmanagedList<Data> _datas = new UnmanagedList<Data>();

    public UnmanagedList<Data> Datas => _datas;

    public struct Data
    {
        public uint Offset;
        public LineNumber LineNumber;
    }

    private int BSearch(int key)
    {
        var m = 0;
        var n = _datas.Count - 1;
        while (m <= n)
        {
            var k = (n + m) >> 1;
            var cmp = key - _datas[k].Offset;
            if (cmp > 0)
            {
                m = k + 1;
            }
            else if (cmp < 0)
            {
                n = k - 1;
            }
            else
            {
                return k;
            }
        }

        return -m - 1;
    }

    public LineNumber GetLineNumber(int key)
    {
        return _datas[LineIndex(key)].LineNumber;
    }

    private int LineIndex(int key)
    {
        if (key <= _datas[0].Offset)
        {
            return 0;
        }

        if (key >= _datas[_datas.Count - 1].Offset)
        {
            return _datas.Count - 1;
        }

        var index = BSearch(key);

        if (index >= 0)
            return index;

        return (-index - 1);
    }


    public void AddLine(uint offset, LineNumber line)
    {
        if (_datas.Count > 0)
        {
            ref var data = ref _datas[_datas.Count - 1];
            if (data.LineNumber == line)
            {
                data.Offset++;
                return;
            }

            if (data.LineNumber > line)
                throw new Exception("Line numbers need to be incremental");
        }

        _datas.Add(new Data()
        {
            LineNumber = line,
            Offset = offset
        });
    }

    public void Dispose()
    {
        _datas?.Dispose();
    }
}