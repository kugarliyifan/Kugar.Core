namespace Kugar.Core.ByteDataConverter
{
    public enum FloatSwap
    {
        /// <summary>
        ///     排列顺序为内存顺序的 2143
        /// </summary>
        CNormal,

        /// <summary>
        ///     排列顺序为内存顺序的全反,即 4321
        /// </summary>
        CSwap,

        /// <summary>
        ///     即内存顺序不做处理
        /// </summary>
        C3210
    }

    public enum DoubleSwap
    {
        C76543210,
        CNormal

    }

    public enum Int32Swap
    {
        C0123,
        C3210,
        C1032
    }

    public enum UInt16Swap
    {
        HightBefore,
        LowBeforce
    }

    public enum DateTimeType
    {
        Long,
        Short
    }
}
