using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MvCamCtrl.NET;
using static MvCamCtrl.NET.MyCamera;


namespace HK.NET
{
    //public enum AcquisitionMode
    //{
    //    SingleFrame,
    //    MultiFrame,
    //    Continuous,
    //}
    internal class AcquisitionControl
    {
        /// <summary>
        /// 采集模式
        /// </summary>
        public MV_CAM_ACQUISITION_MODE? AcquisitionMode { get; set; }
        /// <summary>
        /// 一次触发采集帧数
        /// </summary>
        public int? AcquisitionBurstFrameCount { get; set; }
        /// <summary>
        /// 行频设置
        /// </summary>
        public int? AcquisitionLineRate { get; set; }
        /// <summary>
        /// 行频使能设置
        /// </summary>
        public bool? AcquisitionLineRateEnable {get;set;}
            


    }
}
