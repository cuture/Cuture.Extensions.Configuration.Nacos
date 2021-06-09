namespace Nacos.Internal
{
    /// <summary>
    /// 计数器(线程不安全)
    /// </summary>
    public sealed class Scaler
    {
        private readonly int _increaseValue;
        private readonly int _initValue;
        private readonly int? _maxValue;

        /// <summary>
        /// 当前值
        /// </summary>
        public int Value { get; private set; }

        /// <summary>
        /// <inheritdoc cref="Scaler"/>
        /// </summary>
        /// <param name="initValue">初始值，每次增加值</param>
        /// <param name="increaseValue">每次增加值</param>
        /// <param name="maxValue">最大值</param>
        public Scaler(int initValue, int increaseValue, int? maxValue = null)
        {
            _initValue = initValue;
            Value = initValue;

            _increaseValue = increaseValue;
            _maxValue = maxValue;
        }

        /// <summary>
        /// 增加计数
        /// </summary>
        public void Add()
        {
            if (_maxValue.HasValue)
            {
                if (Value >= _maxValue.Value)
                {
                    return;
                }

                var newValue = Value + _increaseValue;
                if (newValue > _maxValue.Value)
                {
                    newValue = _maxValue.Value;
                }
                Value = newValue;
            }
            else
            {
                Value += _increaseValue;
            }
        }

        /// <summary>
        /// 重置计数
        /// </summary>
        public void Reset() => Value = _initValue;
    }
}