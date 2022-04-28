using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static MvCamCtrl.NET.MyCamera;
namespace HK.NET
{
    public enum InputSourceType
    {
        Line0,
        Line1,
        Line2,
        Line3,
    }
    public enum SignalAlignmentType
    {
        RisingEdge,
        FallingEdge
    }
    public class FrequencyConverterControl
    {
        /// <summary>
        /// 分频器输入源
        /// </summary>
        public InputSourceType? InputSource { get; set; }
        /// <summary>
        /// 分频器信号方向
        /// </summary>
        public SignalAlignmentType? SignalAlignment { get; set; }
        /// <summary>
        /// 前置分频器调节
        /// </summary>
        public uint? PreDivider { get; set; }
        /// <summary>
        /// 倍频器调节
        /// </summary>
        public uint? Multiplier { get; set; }
        /// <summary>
        /// 后置分频器调节
        /// </summary>
        public uint? PostDivider { get; set; }
    }
}
