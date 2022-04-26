using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using MvCamCtrl.NET;
using static MvCamCtrl.NET.MyCamera;

namespace HK.NET
{
    public class HKGigeCamera
    {
        public uint MaxWidth { get; protected set; }
        public uint MaxHeight { get; protected set; }
        private readonly string _code;


        protected MyCamera _myCamera = new();

        protected MV_CC_DEVICE_INFO _deviceInfo;
        protected MV_GIGE_DEVICE_INFO _gigaCamInfo;
        public MV_GIGE_DEVICE_INFO GigaCamInfo => _gigaCamInfo;


        public static List<HKGigeCamera> CreateCameras(List<string> code)
        {
            List<HKGigeCamera> hKGigeCameras = new();
            var cams = SciHKCore.GetDeviceInfoListFull()
                .Where(s => s.nTLayerType == MV_GIGE_DEVICE)
                .Select(s => new { Cam = s, GigeCam = SciHKCore.GetGigeDeviveInfo(s) });
            foreach (var cam in cams)
            {
                hKGigeCameras.Add(new HKGigeCamera(cam.Cam, cam.GigeCam.Value));
            }
            return hKGigeCameras;
        }

        ///// <summary>
        ///// 初始化为第一个
        ///// </summary>
        //public HKGigeCamera()
        //{

        //}
        
        public HKGigeCamera(string code)
        {
            _code = code;
            var cam = SciHKCore.GetDeviceInfoListFull()
                .Where(s => s.nTLayerType == MV_GIGE_DEVICE)
                .Select(s => new { Cam = s, GigeCam = SciHKCore.GetGigeDeviveInfo(s) })
            
                .FirstOrDefault(s => s.GigeCam.HasValue && s.GigeCam.Value.chSerialNumber == code);
            if (cam == null)
            {
                Debug.WriteLine("未找到该序列号");
            }
            else
            {
                _deviceInfo = cam.Cam;
                _gigaCamInfo = cam.GigeCam.Value;
            }
        }

        public HKGigeCamera(MV_CC_DEVICE_INFO deviceInfo, MV_GIGE_DEVICE_INFO gigaCamInfo)
        {
            _code = gigaCamInfo.chSerialNumber;
            _deviceInfo = deviceInfo;
            _gigaCamInfo = gigaCamInfo;
        }

        public virtual bool InitCamera()
        {
            if (!ConnectCamera())
            {
                Debug.WriteLine("连接相机失败");
                DestroyDevice();

                return false;
            }
            if (!OpenDevice())
            {
                Debug.WriteLine("打开相机驱动失败");
                DestroyDevice();

                return false;
            }
            if (!GetOptimalPacketSize(out var size))
            {
                Debug.WriteLine("探测网络最佳包大小失败 size = {0}", size);
                DestroyDevice();

                return false;
            }
            if (!SetHeartTimeOut(5000))
            {
                Debug.WriteLine("设置心跳超时失败");
                DestroyDevice();

                return false;
            }
            if (!GetWidthHeight())
            {
                Debug.WriteLine("获取最大长宽失败");
                return false;
            }
            //触发模式设置
            //if (!SetTriggerMode(false))
            //{
            //    Debug.WriteLine("设置触发模式失败");
            //    DestroyDevice();

            //    return false;
            //}
            if (!StartGrabbing())
            {
                Debug.WriteLine("相机开流失败");
                DestroyDevice();
                return false;
            }

            return true;
           
        }
        /// <summary>
        /// 设置图片模式
        /// </summary>
        /// <returns></returns>
        public bool SetImageFormat()
        {

            return true;
        }
        
        public bool ConnectCamera()
        {
            var nRet = _myCamera.MV_CC_CreateDevice_NET(ref _deviceInfo);
            if (nRet != MV_OK)
            {
                Debug.WriteLine("Create device failed:{0:x8}", nRet);
                return false;
            }
            return true;
        }
        public bool OpenDevice()
        {
            var nRet = _myCamera.MV_CC_OpenDevice_NET();
            if (nRet != MV_OK)
            {
                Debug.WriteLine("Create device failed:{0:x8}", nRet);
                return false;
            }
            return true;
        }
        public bool GetWidthHeight()
        {
            MVCC_INTVALUE widthM = new MVCC_INTVALUE();
            MVCC_INTVALUE heightM = new MVCC_INTVALUE();
            var nRet = _myCamera.MV_CC_GetIntValue_NET("WidthMax", ref widthM);
            if (nRet != MV_OK)
            {
                Debug.WriteLine("Warning: 获取最大宽度失败 {0:x8}", nRet);

                return false;
            }
            MaxWidth = widthM.nCurValue;
            nRet = _myCamera.MV_CC_GetIntValue_NET("HeightMax", ref widthM);
            if (nRet != MV_OK)
            {
                Debug.WriteLine("Warning: 获取最大高度失败 {0:x8}", nRet);
                return false;
            }
            MaxHeight = heightM.nCurValue;
            return true;
        }
        private bool GetOptimalPacketSize(out int nPacketSize)
        {
            nPacketSize = _myCamera.MV_CC_GetOptimalPacketSize_NET();
            if (nPacketSize > 0)
            {
                var nRet = _myCamera.MV_CC_SetIntValue_NET("GevSCPSPacketSize", (uint)nPacketSize);
                if (nRet != MyCamera.MV_OK)
                {
                    Debug.WriteLine("Warning: Set Packet Size failed {0:x8}", nRet);
                    return false;
                }
                return true;
            }
            else
            {
                Debug.WriteLine("Warning: Get Packet Size failed {0:x8}", nPacketSize);
                return false;
            }
        }
        /// <summary>
        /// 设置触发模式
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetTriggerMode(bool value)
        {
            if (MV_OK != _myCamera.MV_CC_SetEnumValue_NET("TriggerMode", value ? (uint)1 : 0))
            {
                Debug.WriteLine("Set TriggerMode failed!");
                return false;
            }
            return true;
        }
        public bool SetTriggerSource(MV_CAM_TRIGGER_SOURCE triggerSource)
        {
            if (_myCamera.MV_CC_SetTriggerMode_NET((uint)triggerSource) != MV_OK)
            {
                return false;
            }
            return true;
            
        }


        public bool StartGrabbing()
        {
            var nRet = _myCamera.MV_CC_StartGrabbing_NET();
            if (MV_OK != nRet)
            {
                Console.WriteLine("Start grabbing failed:{0:x8}", nRet);
                return false ;
            }
            return true;
        }

        public bool StopGrabbing()
        {
            var nRet = _myCamera.MV_CC_StopGrabbing_NET();
            if (MV_OK != nRet)
            {
                Console.WriteLine("Stop grabbing failed{0:x8}", nRet);
                return false;
            }
            return true;
        }

        public bool CloseDevice()
        {
            var nRet = _myCamera.MV_CC_CloseDevice_NET();
            if (MV_OK != nRet)
            {
                Console.WriteLine("Close device failed{0:x8}", nRet);
                return false;
            }
            return true;
        }

        public bool DestroyDevice()
        {
            var nRet = _myCamera.MV_CC_DestroyDevice_NET();
            if (MyCamera.MV_OK != nRet)
            {
                Console.WriteLine("Destroy device failed:{0:x8}", nRet);
                return false;
            }
            return true;
        }
        /// <summary>
        /// 设置心跳超时时间
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetHeartTimeOut(uint value)
        {
            MVCC_INTVALUE linliu = new();

            
            //if (MV_OK != _myCamera.MV_CC_SetBoolValue_NET("DeviceLinkHeartbeatMode", true))
            //{
            //    Debug.WriteLine("Set HeartbeatTimeout failed!");
            //    return false;
            //}
            if (MV_OK != _myCamera.MV_CC_SetIntValue_NET("GevHeartbeatTimeout", value))
            {
                Debug.WriteLine("Set HeartbeatTimeout failed!");
                return false;
            }
            _myCamera.MV_CC_GetIntValue_NET("GevHeartbeatTimeout", ref linliu);
            Console.WriteLine(linliu.nCurValue);
            return true;

        }

       protected bool GetPayloadSize(out MVCC_INTVALUE stParam)
        {
            stParam = new();
            var nRet = _myCamera.MV_CC_GetIntValue_NET("PayloadSize", ref stParam);
            if (MyCamera.MV_OK != nRet)
            {
                Console.WriteLine("Get PayloadSize failed:{0:x8}", nRet);
                return false;
            }
            return true;
        }
        /// <summary>
        /// test
        /// </summary>
        public bool GetImages()
        {
            if (!GetPayloadSize(out var stParam)) return false;
            UInt32 nPayloadSize = stParam.nCurValue;
            IntPtr pBufForDriver = Marshal.AllocHGlobal((int)nPayloadSize);
            IntPtr pBufForSaveImage = IntPtr.Zero;
            MV_FRAME_OUT_INFO_EX FrameInfo = new();
            for (int i = 0; i < 10; i++)
            {
                var nRet = _myCamera.MV_CC_GetOneFrameTimeout_NET(pBufForDriver, nPayloadSize, ref FrameInfo, 1000);
                // ch:获取一帧图像 | en:Get one image
                if (MyCamera.MV_OK == nRet)
                {
                    Console.WriteLine("Get One Frame:" + "Width[" + Convert.ToString(FrameInfo.nWidth) + "] , Height[" + Convert.ToString(FrameInfo.nHeight)
                                    + "] , FrameNum[" + Convert.ToString(FrameInfo.nFrameNum) + "]");

                    if (pBufForSaveImage == IntPtr.Zero)
                    {
                        pBufForSaveImage = Marshal.AllocHGlobal((int)(FrameInfo.nHeight * FrameInfo.nWidth * 3 + 2048));
                    }

                    MyCamera.MV_SAVE_IMAGE_PARAM_EX stSaveParam = new MyCamera.MV_SAVE_IMAGE_PARAM_EX();
                    stSaveParam.enImageType = MyCamera.MV_SAVE_IAMGE_TYPE.MV_Image_Bmp;
                    stSaveParam.enPixelType = FrameInfo.enPixelType;
                    stSaveParam.pData = pBufForDriver;
                    stSaveParam.nDataLen = FrameInfo.nFrameLen;
                    stSaveParam.nHeight = FrameInfo.nHeight;
                    stSaveParam.nWidth = FrameInfo.nWidth;
                    stSaveParam.pImageBuffer = pBufForSaveImage;
                    stSaveParam.nBufferSize = (uint)(FrameInfo.nHeight * FrameInfo.nWidth * 3 + 2048);
                    stSaveParam.nJpgQuality = 80;
                    nRet = _myCamera.MV_CC_SaveImageEx_NET(ref stSaveParam);
                    if (MyCamera.MV_OK != nRet)
                    {
                        Console.WriteLine("Save Image failed:{0:x8}", nRet);
                        continue;
                    }

                    // ch:将图像数据保存到本地文件 | en:Save image data to local file
                    byte[] data = new byte[stSaveParam.nImageLen];
                    Marshal.Copy(pBufForSaveImage, data, 0, (int)stSaveParam.nImageLen);
                    try
                    {
                        FileStream pFile = new FileStream($"{_code}frame{i}.bmp", FileMode.Create);
                        pFile.Write(data, 0, data.Length);
                        pFile.Close();
                    }
                    catch
                    {

                    }
                    continue;
                }
                else
                {
                    Console.WriteLine("No data:{0:x8}", nRet);
                }
            }
            return true;
        }
    }

    public class HKGigeTriggerCamera : HKGigeCamera
    {
        public static List<HKGigeTriggerCamera> CreateCameras(List<string> code)
        {
            List<HKGigeTriggerCamera> hKGigeCameras = new();
            var cams = SciHKCore.GetDeviceInfoListFull()
                .Where(s => s.nTLayerType == MV_GIGE_DEVICE)
                .Select(s => new { Cam = s, GigeCam = SciHKCore.GetGigeDeviveInfo(s) });
            foreach (var cam in cams)
            {
                hKGigeCameras.Add(new HKGigeTriggerCamera(cam.Cam, cam.GigeCam.Value));
            }
            return hKGigeCameras;
        }
        public HKGigeTriggerCamera(string code) : base(code)
        {
        }

        public HKGigeTriggerCamera(MV_CC_DEVICE_INFO deviceInfo, MV_GIGE_DEVICE_INFO gigaCamInfo) : base(deviceInfo, gigaCamInfo)
        {
        }

        public override bool InitCamera()
        {

            if (base.InitCamera())
            {
                return false;
            }
            // 触发模式设置
            if (!SetTriggerMode(true))
            {
                Debug.WriteLine("设置触发模式失败");
                DestroyDevice();

                return false;
            }
            return true;

        }

        public bool GetImage()
        {
            if (!GetPayloadSize(out var stParam)) return false;
            UInt32 nPayloadSize = stParam.nCurValue;
            IntPtr pBufForDriver = Marshal.AllocHGlobal((int)nPayloadSize);
            IntPtr pBufForSaveImage = IntPtr.Zero;
            MV_FRAME_OUT_INFO_EX FrameInfo = new();
            
            return true;

        }
    }


    //public class HKGigeTriggerCamera :
}

