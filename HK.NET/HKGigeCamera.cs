using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using MvCamCtrl.NET;
using static MvCamCtrl.NET.MyCamera;

namespace HK.NET
{
    public class HKGigeCamera : IDisposable
    {
        public uint MaxWidth { get; protected set; }
        public uint MaxHeight { get; protected set; }
        public uint FrameRate { get; protected set; }
        protected readonly string _code;

        public bool IsActive { get; protected set; }


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
                CreateDevice();
            }
        }
        public static byte[] StructToBytes(object structObj)
        {
            //得到结构体的大小
            int size = Marshal.SizeOf(structObj);
            //创建byte数组
            byte[] bytes = new byte[540];
            //分配结构体大小的内存空间
            IntPtr structPtr = Marshal.AllocHGlobal(size);
            //将结构体拷到分配好的内存空间
            Marshal.StructureToPtr(structObj, structPtr, false);
            //从内存空间拷到byte数组
            Marshal.Copy(structPtr, bytes, 0, size);
            //释放内存空间
            Marshal.FreeHGlobal(structPtr);
            //返回byte数组
            return bytes;
        }
        public HKGigeCamera(MV_CC_DEVICE_INFO deviceInfo, MV_GIGE_DEVICE_INFO gigaCamInfo)
        {
            _code = gigaCamInfo.chSerialNumber;
            _deviceInfo = deviceInfo;
            _gigaCamInfo = gigaCamInfo;
            //_myCamera.
            //_gigaCamInfo.nCurrentIp = IpConvert("192.168.1.155");
            CreateDevice();
        }
        ~HKGigeCamera()
        {
            DestroyDevice();
        }
        /// <summary>
        /// 重置相机
        /// </summary>
        /// <returns></returns>
        public bool ResetCamera()
        {

            if (_myCamera.MV_CC_SetCommandValue_NET("DeviceReset") != MV_OK)
            {
                Debug.WriteLine("重启设备失败");
                return false;
            }
            return true;
        }

        public virtual bool InitCamera()
        {
            //if (!CreateDevice())
            //{
            //    Debug.WriteLine("连接相机失败");
            //    DestroyDevice();

            //    return false;
            //}
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

        //public bool SetImageFormat(ImageFormatControl imageFormatControl)
        //{

        //    return true;
        //}

        public bool TryGetFps(out float fps)
        {
            MVCC_FLOATVALUE val = new();
            if (_myCamera.MV_CC_GetFrameRate_NET(ref val) != MV_OK)
            {
                fps = 0.0f;
                return false;
            }
            fps = val.fCurValue;
            return true;
        }
        /// <summary>
        /// 检查连接
        /// </summary>
        /// <returns></returns>
        public bool CheckConnect()
        {
            MVCC_INTVALUE val = new();
            var nRet = _myCamera.MV_CC_GetWidth_NET(ref val);
            //Debug.WriteLine($"nRet = {nRet}, val = {val.nCurValue}");
            return nRet == MV_OK;
        }
        /// <summary>
        /// 创建驱动
        /// </summary>
        /// <returns></returns>
        public bool CreateDevice()
        {
            var nRet = _myCamera.MV_CC_CreateDevice_NET(ref _deviceInfo);
            if (nRet != MV_OK)
            {
                Debug.WriteLine("Create device failed:{0:x8}", nRet);
                return false;
            }
            return true;
        }
        /// <summary>
        /// 开启驱动
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        /// 获取最适合的包大小
        /// </summary>
        /// <param name="nPacketSize"></param>
        /// <returns></returns>
        protected bool GetOptimalPacketSize(out int nPacketSize)
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
        public bool SetTriggerMode(MV_CAM_TRIGGER_MODE triggerMode)
        {
            //if (MV_OK != _myCamera.MV_CC_SetEnumValue_NET("TriggerMode", value ? (uint)1 : 0))
            if (MV_OK != _myCamera.MV_CC_SetTriggerMode_NET(triggerMode == MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_ON ? (uint)1 : 0))
            {
                Debug.WriteLine("Set TriggerMode failed!");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 设置触发模式
        /// </summary>
        /// <param name="triggerSource"></param>
        /// <returns></returns>
        public bool SetTriggerSource(MV_CAM_TRIGGER_SOURCE triggerSource)
        {
            if (_myCamera.MV_CC_SetTriggerSource_NET((uint)triggerSource) != MV_OK)
            {
                return false;
            }
            return true;

        }

        /// <summary>
        /// 开流
        /// </summary>
        /// <returns></returns>
        public bool StartGrabbing()
        {
            var nRet = _myCamera.MV_CC_StartGrabbing_NET();
            //_myCamera.MV_CC_SetHeartBeatTimeout_NET
            if (MV_OK != nRet)
            {
                Console.WriteLine("Start grabbing failed:{0:x8}", nRet);
                return false;
            }
            return true;
        }
        /// <summary>
        /// 关流
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        /// 关闭相机驱动
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        /// 销毁相机驱动
        /// </summary>
        /// <returns></returns>
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
            //if (MV_OK != _myCamera.MV_CC_SetIntValue_NET("GevHeartbeatTimeout", value))
            if (MV_OK != _myCamera.MV_CC_SetHeartBeatTimeout_NET(value))
            {
                Debug.WriteLine("Set HeartbeatTimeout failed!");
                return false;
            }
            _myCamera.MV_CC_GetIntValue_NET("GevHeartbeatTimeout", ref linliu);
            Console.WriteLine(linliu.nCurValue);
            return true;

        }
        /// <summary>
        /// 获取最佳适应包大小
        /// </summary>
        /// <param name="stParam"></param>
        /// <returns></returns>
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
        /// 设置曝光时间
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetExposureTime(uint value)
        {
            var nRet = _myCamera.MV_CC_SetExposureTime_NET(value);
            if (MV_OK != nRet)
            {
                Debug.WriteLine("Set ExposureTime failed!: {0}", nRet);
                return false;
            }
            return true;
        }
        public bool SetIp(string ip)
        {
            var nRet = _myCamera.MV_GIGE_SetIpConfig_NET(MV_IP_CFG_STATIC);
            //nRet = _myCamera.MV_CC_SetBoolValue_NET("GevCurrentIPConfigurationPersistentIP", true);
            nRet = _myCamera.MV_GIGE_ForceIp_NET(IpConvert(ip));
            if (nRet != MV_OK)
            {
                Debug.WriteLine("ip设置失败: {0}", nRet);
                return false;
            }
            //_deviceInfo.SpecialInfo
            _gigaCamInfo.nCurrentIp = IpConvert(ip);
            _deviceInfo.SpecialInfo.stGigEInfo = StructToBytes(_gigaCamInfo);

            DestroyDevice();
            CreateDevice();
            return true;
        }
        public bool SetIp(string ip, string mask, string gateway)
        {
            var nRet = _myCamera.MV_GIGE_SetIpConfig_NET(MV_IP_CFG_STATIC);
            //nRet = _myCamera.MV_CC_SetBoolValue_NET("GevCurrentIPConfigurationPersistentIP", true);
            nRet = _myCamera.MV_GIGE_ForceIpEx_NET(IpConvert(ip), IpConvert(mask), IpConvert(gateway));
            _gigaCamInfo.nCurrentIp = IpConvert(ip);
            _gigaCamInfo.nCurrentSubNetMask = IpConvert(mask);
            _gigaCamInfo.nDefultGateWay = IpConvert(gateway);

            if (nRet != MV_OK)
            {
                Debug.WriteLine("ip设置失败: {0}", nRet);
                return false;
            }
            return true;
            //if (Mask != null)
            //    if (_myCamera.MV_CC_SetIntValue_NET("GevPersistentSubnetMask", IpConvert(Mask)) != MV_OK)
            //    {
            //        Debug.WriteLine("子网掩码设置失败");
            //        return false;
            //    }
            //if (Gateway != null)
            //    if (_myCamera.MV_CC_SetIntValue_NET("GevPersistentDefaultGateway", IpConvert(Gateway)) != MV_OK)
            //    {
            //        Debug.WriteLine("网关设置失败");
            //        return false;
            //    }
            //return true;
            //IPEndPoint.Parse(ip).Address.Address

        }

        public bool SetAcquisitionMode(MV_CAM_ACQUISITION_MODE mode)
        {
            var nRet = _myCamera.MV_CC_SetEnumValue_NET("AcquisitionMode", (uint)mode);
            if (nRet != MV_OK)
            {
                Debug.WriteLine("设置采集模式失败: {0}", nRet);
                return false;
            }
            return true;
        }
        /// <summary>
        /// 一次触发采集的帧数
        /// </summary>
        /// <param name="value"></param>
        public bool SetAcquisitionBurstFrameCount(uint value)
        {
            var nRet = _myCamera.MV_CC_SetIntValue_NET("AcquisitionBurstFrameCount", value);
            if (nRet != MV_OK)
            {
                Debug.WriteLine("设置倍频失败: {0}", nRet);
            }
            return true;
        }
        /// <summary>
        /// 行频设置
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetAcquisitionLineRate(uint value)
        {
            var nRet = _myCamera.MV_CC_SetIntValue_NET("AcquisitionLineRate", value);
            if (nRet != MV_OK)
            {
                Debug.WriteLine("设置倍频失败: {0}", nRet);
            }
            return true;
        }
        /// <summary>
        /// 行频使能
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetAcquisitionLineRateEnable(bool value)
        {
            var nRet = _myCamera.MV_CC_SetBoolValue_NET("AcquisitionLineRateEnable", value);
            if (nRet != MV_OK)
            {
                Debug.WriteLine("设置行频使能失败: {0}", nRet);
            }
            return true;
        }
        /// <summary>
        /// 触发上升沿、下降沿、高电平、低电平等
        /// </summary>
        /// <param name="triggerActivation"></param>
        /// <returns></returns>
        public bool SetTriggerActivation(TriggerActivation triggerActivation)
        {
            var nRet = _myCamera.MV_CC_SetEnumValue_NET("TriggerActivation", (uint)triggerActivation);
            if (nRet != MV_OK)
            {
                Debug.WriteLine("设置行频使能失败: {0}", nRet);
            }
            return true;
        }
        /// <summary>
        /// 设置触发延时
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetTriggerDelay(float value)
        {
            var nRet = _myCamera.MV_CC_SetFloatValue_NET("TriggerDelay", value);
            if (nRet != MV_OK)
            {
                Debug.WriteLine("设置倍频失败: {0}", nRet);
            }
            return true;
        }

        public bool SetImageWidth(uint value)
        {
            var nRet = _myCamera.MV_CC_SetIntValue_NET("Width", value);
            if (nRet != MV_OK)
            {
                Debug.WriteLine("设置图像宽度失败: {0}", nRet);
            }
            return true;
        }
        /// <summary>
        /// 设置图像宽度
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetImageHeight(uint value)
        {
            var nRet = _myCamera.MV_CC_SetIntValue_NET("Height", value);
            if (nRet != MV_OK)
            {
                Debug.WriteLine("设置图像高度失败: {0}", nRet);
            }
            return true;
        }
        public bool SetOffsetX(uint value)
        {
            var nRet = _myCamera.MV_CC_SetIntValue_NET("OffsetX", value);
            if (nRet != MV_OK)
            {
                Debug.WriteLine("设置图像OffsetX失败: {0}", nRet);
            }
            return true;
        }
        public bool SetOffsetY(uint value)
        {
            var nRet = _myCamera.MV_CC_SetIntValue_NET("OffsetY", value);
            if (nRet != MV_OK)
            {
                Debug.WriteLine("设置图像OffsetY失败: {0}", nRet);
            }
            return true;
        }

        public virtual bool GetImage(out byte[] imageBytes)
        {
            imageBytes = new byte[0];
            return true;
        }
        /// <summary>
        /// 设置图片模式
        /// </summary>
        /// <returns></returns>
        public bool SetImageFormat(ImageFormatControl imageFormatControl)
        {
            bool flag = true;
            if (imageFormatControl.Width != null)
            {
                flag &= SetImageWidth(imageFormatControl.Width.Value);
            }
            if (imageFormatControl.Height != null)
            {
                flag &= SetImageHeight(imageFormatControl.Height.Value);
            }
            if (imageFormatControl.OffsetX != null)
            {
                flag &= SetOffsetX(imageFormatControl.OffsetX.Value);
            }
            if (imageFormatControl.OffsetY != null)
            {
                flag &= SetOffsetY(imageFormatControl.OffsetY.Value);
            }
            return flag;
        }
        public bool SetAcquisition(AcquisitionControl acquisitionControl)
        {
            bool flag = true;
            if (acquisitionControl.AcquisitionMode != null)
            {
                flag &= SetAcquisitionMode(acquisitionControl.AcquisitionMode.Value);
            }
            if (acquisitionControl.AcquisitionBurstFrameCount != null)
            {
                flag &= SetAcquisitionBurstFrameCount(acquisitionControl.AcquisitionBurstFrameCount.Value);
            }
            if (acquisitionControl.AcquisitionLineRateEnable != null)
            {
                flag &= SetAcquisitionLineRateEnable(acquisitionControl.AcquisitionLineRateEnable.Value);
            }
            if (acquisitionControl.AcquisitionLineRate != null)
            {
                flag &= SetAcquisitionLineRate(acquisitionControl.AcquisitionLineRate.Value);
            }
            if (acquisitionControl.TriggerMode != null)
            {
                flag &= SetTriggerMode(acquisitionControl.TriggerMode.Value);
            }
            if (acquisitionControl.TriggerSource != null)
            {
                flag &= SetTriggerSource(acquisitionControl.TriggerSource.Value);
            }
            if (acquisitionControl.TriggerActivation != null)
            {
                flag &= SetTriggerActivation(acquisitionControl.TriggerActivation.Value);
            }
            if (acquisitionControl.TriggerDelay != null)
            {
                flag &= SetTriggerDelay(acquisitionControl.TriggerDelay.Value);
            }
            if (acquisitionControl.ExposureTime != null)
            {
                flag &= SetExposureTime(acquisitionControl.ExposureTime.Value);
            }
            return flag;
        }
        //public bool SetTriggerSelector()
        /// <summary>
        /// IP转换工具
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        protected uint IpConvert(string ip)
        {
            uint[] ip4 = ip.Split('.').Select(uint.Parse).ToArray();
            uint res = 0;
            for (int i = 0; i < ip4.Length; i++)
            {
                res <<= 8;
                res |= ip4[i];
            }
            return res;
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

        public void Dispose()
        {
            DestroyDevice();
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
        /// <summary>
        /// 初始化相机
        /// </summary>
        /// <returns></returns>
        public override bool InitCamera()
        {


            //if (!CreateDevice())
            //{
            //    Debug.WriteLine("连接相机失败");
            //    DestroyDevice();

            //    return false;
            //}
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

            // 触发模式设置
            if (!SetTriggerMode(MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_ON))
            {
                Debug.WriteLine("设置触发模式失败");
                DestroyDevice();

                return false;
            }

            if (!SetTriggerSource(MV_CAM_TRIGGER_SOURCE.MV_TRIGGER_SOURCE_SOFTWARE))
            {
                Debug.WriteLine("设置触发源失败");
                DestroyDevice();

                return false;
            }
            if (!SetExposureTime(5000))
            {
                Debug.WriteLine("设置曝光时间失败");
                DestroyDevice();

                return false;
            }
            return true;

        }
        /// <summary>
        /// 拍照
        /// </summary>
        /// <param name="imageBytes"></param>
        /// <returns></returns>
        public bool GetImage(out byte[] imageBytes)
        {
            imageBytes = null;
            if (!GetPayloadSize(out var stParam)) return false;
            UInt32 nPayloadSize = stParam.nCurValue;
            IntPtr pBufForDriver = Marshal.AllocHGlobal((int)nPayloadSize);
            IntPtr pBufForSaveImage = IntPtr.Zero;
            MV_FRAME_OUT_INFO_EX FrameInfo = new();
            _myCamera.MV_CC_TriggerSoftwareExecute_NET();
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
                    return false;
                }

                // ch:将图像数据保存到本地文件 | en:Save image data to local file
                imageBytes = new byte[stSaveParam.nImageLen];
                Marshal.Copy(pBufForSaveImage, imageBytes, 0, (int)stSaveParam.nImageLen);
                //try
                //{
                //    path = $"{_code}frame.bmp";
                //    FileStream pFile = new FileStream(path, FileMode.Create);
                //    pFile.Write(data, 0, data.Length);
                //    pFile.Close();
                //}
                //catch
                //{

                //}
            }
            else
            {
                Console.WriteLine("No data:{0:x8}", nRet);
                return false;
            }
            return true;

        }
    }


    //public class HKGigeTriggerCamera :
}

