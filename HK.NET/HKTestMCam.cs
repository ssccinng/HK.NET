using MvCamCtrl.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using MvCamCtrl.NET;
using static MvCamCtrl.NET.MyCamera;
using System.Diagnostics;

namespace HK.NET
{
    internal class HKTestMCam : HKGigeCamera
    {
        public HKTestMCam(string code) : base(code)
        {
            cbImage = new cbOutputExdelegate(ImageCallBack);

        }

        public HKTestMCam(MyCamera.MV_CC_DEVICE_INFO deviceInfo, MyCamera.MV_GIGE_DEVICE_INFO gigaCamInfo) : base(deviceInfo, gigaCamInfo)
        {
            cbImage = new cbOutputExdelegate(ImageCallBack);

        }
        public static List<HKTestMCam> CreateCameras(List<string> code)
        {
            List<HKTestMCam> hKGigeCameras = new();
            var cams = SciHKCore.GetDeviceInfoListFull()
                .Where(s => s.nTLayerType == MV_GIGE_DEVICE)
                .Select(s => new { Cam = s, GigeCam = SciHKCore.GetGigeDeviveInfo(s) });
            foreach (var cam in cams)
            {
                hKGigeCameras.Add(new HKTestMCam(cam.Cam, cam.GigeCam.Value));
            }
            return hKGigeCameras;
        }

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
            if (!SetTriggerMode(MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_OFF))
            {
                Debug.WriteLine("设置触发模式失败");
                DestroyDevice();

                return false;
            }
            if (!StartGrabbing())
            {
                Debug.WriteLine("相机开流失败");
                DestroyDevice();
                return false;
            }

            return true;

        }
        public override bool OpenDevice()
        {
            _myCamera.MV_CC_RegisterImageCallBackEx_NET(cbImage, IntPtr.Zero);
            return base.OpenDevice();
        }
    }
}
