using MvCamCtrl.NET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static MvCamCtrl.NET.MyCamera;
using System.Runtime.InteropServices;
using System.Drawing;

namespace HK.NET
{
    public class HKGigeLineCamera : HKGigeCamera
    {
        public HKGigeLineCamera(string code) : base(code)
        {
            cbImage = new cbOutputExdelegate(ImageCallBack);

        }

        public HKGigeLineCamera(MyCamera.MV_CC_DEVICE_INFO deviceInfo, MyCamera.MV_GIGE_DEVICE_INFO gigaCamInfo) : base(deviceInfo, gigaCamInfo)
        {
            cbImage = new cbOutputExdelegate(ImageCallBack);

        }
        public override bool OpenDevice()
        {
            _myCamera.MV_CC_RegisterImageCallBackEx_NET(cbImage, IntPtr.Zero);
            return base.OpenDevice();
        }
        public override bool InitCamera()
        {
            // 检测一下线相机
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
            using Graphics graph = Graphics.FromImage(bitmap);
            imageBytes = null;
            //if (!GetPayloadSize(out var stParam)) return false;
            //UInt32 nPayloadSize = stParam.nCurValue;
            //IntPtr pBufForDriver = Marshal.AllocHGlobal((int)nPayloadSize);
            //IntPtr pBufForSaveImage = IntPtr.Zero;
            ////MV_FRAME_OUT_INFO_EX FrameInfo = new();
            //int nRet = 0;
            //int idx = 0;
            //while (FrameInfoQueue.Count > 0)
            //{
            //    (IntPtr pdata, MV_FRAME_OUT_INFO_EX FrameInfo) = FrameInfoQueue.Dequeue();
            //    Console.WriteLine("Get One Frame:" + "Width[" + Convert.ToString(FrameInfo.nWidth) + "] , Height[" + Convert.ToString(FrameInfo.nHeight)
            //               + "] , FrameNum[" + Convert.ToString(FrameInfo.nFrameNum) + "]");

            //    if (pBufForSaveImage == IntPtr.Zero)
            //    {
            //        pBufForSaveImage = Marshal.AllocHGlobal((int)(FrameInfo.nHeight * FrameInfo.nWidth * 3 + 2048));
            //    }

            //    MV_SAVE_IMAGE_PARAM_EX stSaveParam = new MV_SAVE_IMAGE_PARAM_EX();
            //    stSaveParam.enImageType = MyCamera.MV_SAVE_IAMGE_TYPE.MV_Image_Bmp;
            //    stSaveParam.enPixelType = FrameInfo.enPixelType;
            //    //stSaveParam.pData = pBufForDriver;
            //    stSaveParam.pData = pdata;
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
            //    File.WriteAllBytes($"{idx}.bmp", imageBytes);
            //    var image1 = Image.FromStream(new MemoryStream(imageBytes));
            //    //image1.Save();
            //    {
            //        //graph.DrawImage(image1, 0, idx * 2);
            //        graph.DrawImage(image1, 0, idx * 2, width: MaxWidth, height: 2);
            //        idx++;
            //        //graph.DrawImage(newImg, 0, sourceImg.Height, newImg.Width, newImg.Height);
            //    }
            //}


           
            ////while (true)
            ////{
            ////    nRet = _myCamera.MV_CC_GetOneFrameTimeout_NET(pBufForDriver, nPayloadSize, ref FrameInfo, 1000);
            ////    if (nRet != MV_OK) break;
                

            ////}

            //bitmap.Save("揽镜.bmp");

            return true;
        }
    }
}
