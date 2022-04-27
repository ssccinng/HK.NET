using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MvCamCtrl.NET;
using static MvCamCtrl.NET.MyCamera;


namespace HK.NET
{
    public enum TriggerActivation
    {
        RisingEdge,

        FallingEdge,

        LevelHigh,

        LevelLow,

        //SingleFrame,
        //MultiFrame,
        //Continuous,     SingleFrame,
        //MultiFrame,
        //Continuous,
    }
    public class AcquisitionControl
    {
        /// <summary>
        /// 采集模式
        /// </summary>
        public MV_CAM_ACQUISITION_MODE? AcquisitionMode { get; set; }
        /// <summary>
        /// 一次触发采集帧数
        /// </summary>
        public uint? AcquisitionBurstFrameCount { get; set; }
        /// <summary>
        /// 行频设置
        /// </summary>
        public uint? AcquisitionLineRate { get; set; }
        /// <summary>
        /// 行频使能设置
        /// </summary>
        public bool? AcquisitionLineRateEnable {get;set;}
            
        /// <summary>
        /// 触发模式
        /// </summary>
        public MV_CAM_TRIGGER_MODE? TriggerMode { get; set; }
        /// <summary>
        /// 触发源
        /// </summary>
        public MV_CAM_TRIGGER_SOURCE? TriggerSource { get; set; }
        /// <summary>
        /// 触发方式
        /// </summary>
        public TriggerActivation? TriggerActivation { get; set; }
        /// <summary>
        /// 曝光时间
        /// </summary>
        public uint? ExposureTime { get; set; }
        public float? TriggerDelay { get; set; }
    }
}
