namespace TestProject
{
    // The code is used for rx exercise not the part of the project.
    // The code website is 
    // http://stackoverflow.com/questions/5166716/linq-to-calculate-a-moving-average-of-a-sortedlistdatetime-double
    class MovingAverage
    {
        private readonly int _length;
        private readonly double[] _circularBuffer;
        private int _circIndex = -1;
        private bool _filled = false;
        private double _total = 0.0;
        private double _average = double.NaN;
        public MovingAverage(int length)
        {
            _length = length;
            _circularBuffer = new double[length];
        }
        public MovingAverage Push(double value)
        {
            _circIndex++;
            _circIndex %= _length;
            double lostValue = _circularBuffer[_circIndex];
            _total -= lostValue;
            _total += value;
            _circularBuffer[_circIndex] = value;
            if(!_filled && _circIndex != _length - 1)
                return this;
            else
                _filled = true;
            _average = _total / _length;
            return this;
        }
        public int Length
        { get { return _length; } }
        public double Current
        { get { return _average; } }
    }
}
