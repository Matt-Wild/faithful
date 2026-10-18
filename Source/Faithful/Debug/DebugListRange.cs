using System;

namespace Faithful
{
    internal static class DebugListRange
    {
        public static int GetVisibleCount(int _count, float _viewportHeight, float _rowHeight)
        {
            // Include an extra row above and below the viewport
            return Math.Min(_count, (int)Math.Ceiling(_viewportHeight / _rowHeight) + 2);
        }

        public static int GetFirstIndex(int _count, int _visibleCount, float _offset, float _rowHeight)
        {
            // Keep the visible range inside the full list, including while overscrolling
            int firstIndex = (int)Math.Floor(_offset / _rowHeight) - 1;
            return Math.Max(0, Math.Min(firstIndex, Math.Max(0, _count - _visibleCount)));
        }

        public static int GetRowIndex(int _slot, int _firstIndex, int _visibleCount)
        {
            // Reuse rows in a ring so scrolling one entry only changes one row
            if (_visibleCount == 0 || _slot >= _visibleCount) return -1;
            return _firstIndex + (_slot - _firstIndex % _visibleCount + _visibleCount) % _visibleCount;
        }

        public static float GetScrollOffset(int _index, int _count, float _offset, float _viewportHeight, float _rowHeight)
        {
            // Move only far enough to show the requested row
            float top = _index * _rowHeight;
            float bottom = top + _rowHeight;
            if (_viewportHeight < _rowHeight || top < _offset) _offset = top;
            else if (bottom > _offset + _viewportHeight) _offset = bottom - _viewportHeight;

            // Keep the offset inside the scrollable content
            return Math.Max(0.0f, Math.Min(_offset, _count * _rowHeight - _viewportHeight));
        }
    }
}
