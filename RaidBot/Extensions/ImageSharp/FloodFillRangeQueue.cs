namespace T.Extensions.ImageSharp
{
    using System;

    /// <summary>A queue of FloodFillRanges.</summary>
    public class FloodFillRangeQueue
	{
        private FloodFillRange[] _array;
        private int _size;
        private int _head;

        /// <summary>
        /// Returns the number of items currently in the queue.
        /// </summary>
        public int Count => _size;

		public FloodFillRangeQueue() 
            : this(10000)
		{
		}

		public FloodFillRangeQueue(int initialSize)
		{
			_array = new FloodFillRange[initialSize];
			_head = 0;
			_size = 0;
		}

		/// <summary>Gets the <see cref="FloodFillRange"/> at the beginning of the queue.</summary>
		public FloodFillRange First
		{
			get { return _array[_head]; }
		}

		/// <summary>Adds a <see cref="FloodFillRange"/> to the end of the queue.</summary>
		public void Enqueue(ref FloodFillRange r)
		{
			if (_size + _head == _array.Length)
			{
				FloodFillRange[] newArray = new FloodFillRange[2 * _array.Length];
				Array.Copy(_array, _head, newArray, 0, _size);
				_array = newArray;
				_head = 0;
			}
			_array[_head + (_size++)] = r;
		}

		/// <summary>Removes and returns the <see cref="FloodFillRange"/> at the beginning of the queue.</summary>
		public FloodFillRange Dequeue()
		{
			FloodFillRange range = new FloodFillRange();
			if (_size > 0)
			{
				range = _array[_head];
				_array[_head] = new FloodFillRange();
				_head++;//advance head position
				_size--;//update size to exclude dequeued item
			}
			return range;
		}
	}
}
