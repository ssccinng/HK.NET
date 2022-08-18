namespace HK.NET
{
    public class AnalogControl
    {
        /// <summary>
        /// 增益值
        /// </summary>
        public float? Gain { get; set; }
        /// <summary>
        /// 自动增益类型
        /// </summary>
        public GainAutoType? GainAuto { get; set; }
        /// <summary>
        /// 自动增益下限
        /// </summary>
        public float? AutoGainLowerLimit { get; set; }
        /// <summary>
        /// 自动增益上限
        /// </summary>
        public float? AutoGainUpperLimit { get; set; }
        /// <summary>
        /// ADC增益使能
        /// </summary>
        public bool? ADCGainEnable { get; set; }
    }

    public enum GainAutoType
    {
        Off,
        Once,
        Continuous 
    }
}