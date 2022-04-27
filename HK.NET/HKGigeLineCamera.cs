using MvCamCtrl.NET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvCamCtrl.NET;
using static MvCamCtrl.NET.MyCamera;
using System.Runtime.InteropServices;
using System.Drawing;

namespace HK.NET
{
    public class HKGigeLineCamera : HKGigeCamera
    {
        public HKGigeLineCamera(string code) : base(code)
        {
        }

        public HKGigeLineCamera(MyCamera.MV_CC_DEVICE_INFO deviceInfo, MyCamera.MV_GIGE_DEVICE_INFO gigaCamInfo) : base(deviceInfo, gigaCamInfo)
        {
        }

        public override bool InitCamera()
        {
            // 检测一下线相机
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
            if (!SetImageFormat(new ImageFormatControl
            {
                 Height = 8,
                 Width = MaxWidth,
                 
            }))
            {
                return false;
            }
            if (!SetAcquisition(new AcquisitionControl
            {
                AcquisitionLineRate = 1650,
                TriggerMode = MyCamera.MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_ON,
                TriggerSource = MyCamera.MV_CAM_TRIGGER_SOURCE.MV_TRIGGER_SOURCE_LINE0,
                ExposureTime = 1200,
                AcquisitionLineRateEnable = false
            }))
            {
                Debug.WriteLine("获取最大临流失败");
                return false;
            }
            return true;
        }

        public override bool GetImage(out byte[] imageBytes)
        {
            Bitmap bitmap = new Bitmap((int)MaxWidth, 1800);
            imageBytes = null;
            if (!GetPayloadSize(out var stParam)) return false;
            UInt32 nPayloadSize = stParam.nCurValue;
            IntPtr pBufForDriver = Marshal.AllocHGlobal((int)nPayloadSize);
            IntPtr pBufForSaveImage = IntPtr.Zero;
            MV_FRAME_OUT_INFO_EX FrameInfo = new();
            int nRet = 0;
            while (_myCamera.MV_CC_GetOneFrameTimeout_NET(pBufForDriver, nPayloadSize, ref FrameInfo, 100) == MV_OK)
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
                //Image.FromStream(new MemoryStream(imageBytes));

                Marshal.Copy(pBufForSaveImage, imageBytes, 0, (int)stSaveParam.nImageLen);
            }
            //var nRet = 
            //var nRet = _myCamera.mvccgetfra(pBufForDriver, nPayloadSize, ref FrameInfo, 1000);
            // ch:获取一帧图像 | en:Get one image
            //if (MyCamera.MV_OK == nRet)
            //{
            //    Console.WriteLine("Get One Frame:" + "Width[" + Convert.ToString(FrameInfo.nWidth) + "] , Height[" + Convert.ToString(FrameInfo.nHeight)
            //                    + "] , FrameNum[" + Convert.ToString(FrameInfo.nFrameNum) + "]");

            //    if (pBufForSaveImage == IntPtr.Zero)
            //    {
            //        pBufForSaveImage = Marshal.AllocHGlobal((int)(FrameInfo.nHeight * FrameInfo.nWidth * 3 + 2048));
            //    }

            //    MyCamera.MV_SAVE_IMAGE_PARAM_EX stSaveParam = new MyCamera.MV_SAVE_IMAGE_PARAM_EX();
            //    stSaveParam.enImageType = MyCamera.MV_SAVE_IAMGE_TYPE.MV_Image_Bmp;
            //    stSaveParam.enPixelType = FrameInfo.enPixelType;
            //    stSaveParam.pData = pBufForDriver;
            //    stSaveParam.nDataLen = FrameInfo.nFrameLen;
            //    stSaveParam.nHeight = FrameInfo.nHeight;
            //    stSaveParam.nWidth = FrameInfo.nWidth;
            //    stSaveParam.pImageBuffer = pBufForSaveImage;
            //    stSaveParam.nBufferSize = (uint)(FrameInfo.nHeight * FrameInfo.nWidth * 3 + 2048);
            //    stSaveParam.nJpgQuality = 80;
            //    nRet = _myCamera.MV_CC_SaveImageEx_NET(ref stSaveParam);
            //    if (MyCamera.MV_OK != nRet)
            //    {
            //        Console.WriteLine("Save Image failed:{0:x8}", nRet);
            //        return false;
            //    }

            //    // ch:将图像数据保存到本地文件 | en:Save image data to local file
            //    imageBytes = new byte[stSaveParam.nImageLen];
            //    Marshal.Copy(pBufForSaveImage, imageBytes, 0, (int)stSaveParam.nImageLen);
            //    //try
            //    //{
            //    //    path = $"{_code}frame.bmp";
            //    //    FileStream pFile = new FileStream(path, FileMode.Create);
            //    //    pFile.Write(data, 0, data.Length);
            //    //    pFile.Close();
            //    //}
            //    //catch
            //    //{

            //    //}
            //}
            //else
            //{
            //    Console.WriteLine("No data:{0:x8}", nRet);
            //    return false;
            //}
            return true;
        }
    }
}
