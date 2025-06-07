using GerGO.Utils;

namespace GerGO.Query
{
    class RowComparerAsc : IComparer<string>
    {
        string _type;
        int _indexOutX, _indexOutY;
        int _indexInX, _indexInY;
        public RowComparerAsc(string type, int indexOutX, int indexOutY, int indexInX, int indexInY)
        {
            _type = type;
            _indexOutX = indexOutX;
            _indexOutY = indexOutY;
            _indexInX = indexInX;
            _indexInY = indexInY;
        }

        public int Compare(string? x, string? y)
        {
            string val1 = x.Split('#')[_indexOutX].Split('^')[_indexInX];
            string val2 = y.Split('#')[_indexOutY].Split('^')[_indexInY];
            if (Validator.IsLess(val1, val2, _type)) return -1;
            return 1;
        }
    }
}
