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
    public static class SciHKCore
    {
        /// <summary>
        /// 获取相机列表
        /// </summary>
        /// <returns></returns>
        public static MV_CC_DEVICE_INFO_LIST GetDeviceInfoList()
        {
            MV_CC_DEVICE_INFO_LIST stDevList = new();
            var nRet = MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE | MyCamera.MV_USB_DEVICE, ref stDevList);
            stDevList.pDeviceInfo = stDevList.pDeviceInfo.TakeWhile(s => s != IntPtr.Zero).ToArray();
            if (MyCamera.MV_OK != nRet)
            {
                Debug.WriteLine("Enum device failed:{0:x8}", nRet);
            }
            return stDevList;
        }

        public static MV_GIGE_DEVICE_INFO? GetGigeDeviveInfo(IntPtr camPtr)
        {
            var stDevInfo = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(camPtr, typeof(MV_CC_DEVICE_INFO));
            return GetGigeDeviveInfo(stDevInfo);
        }


        public static MV_GIGE_DEVICE_INFO? GetGigeDeviveInfo(MV_CC_DEVICE_INFO stDevInfo)
        {

            if (MyCamera.MV_GIGE_DEVICE == stDevInfo.nTLayerType)
            {
                MV_GIGE_DEVICE_INFO stGigEDeviceInfo = (MV_GIGE_DEVICE_INFO)MyCamera.ByteToStruct(stDevInfo.SpecialInfo.stGigEInfo, typeof(MyCamera.MV_GIGE_DEVICE_INFO));
                uint nIp1 = ((stGigEDeviceInfo.nCurrentIp & 0xff000000) >> 24);
                uint nIp2 = ((stGigEDeviceInfo.nCurrentIp & 0x00ff0000) >> 16);
                uint nIp3 = ((stGigEDeviceInfo.nCurrentIp & 0x0000ff00) >> 8);
                uint nIp4 = (stGigEDeviceInfo.nCurrentIp & 0x000000ff);
                Console.WriteLine("\n" + ": [GigE] User Define Name : " + stGigEDeviceInfo.chUserDefinedName);
                Console.WriteLine(stGigEDeviceInfo.chSerialNumber);
                Console.WriteLine("device IP :" + nIp1 + "." + nIp2 + "." + nIp3 + "." + nIp4);
                return stGigEDeviceInfo;
            }
            return null;
        }
        public static MV_CC_DEVICE_INFO GetDeviveInfo(IntPtr camPtr)
        {
            var stDevInfo = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(camPtr, typeof(MV_CC_DEVICE_INFO));
            if (MyCamera.MV_GIGE_DEVICE == stDevInfo.nTLayerType)
            {
                MyCamera.MV_GIGE_DEVICE_INFO stGigEDeviceInfo = (MyCamera.MV_GIGE_DEVICE_INFO)MyCamera.ByteToStruct(stDevInfo.SpecialInfo.stGigEInfo, typeof(MyCamera.MV_GIGE_DEVICE_INFO));
                uint nIp1 = ((stGigEDeviceInfo.nCurrentIp & 0xff000000) >> 24);
                uint nIp2 = ((stGigEDeviceInfo.nCurrentIp & 0x00ff0000) >> 16);
                uint nIp3 = ((stGigEDeviceInfo.nCurrentIp & 0x0000ff00) >> 8);
                uint nIp4 = (stGigEDeviceInfo.nCurrentIp & 0x000000ff);
                Console.WriteLine("\n" + ": [GigE] User Define Name : " + stGigEDeviceInfo.chUserDefinedName);
                Console.WriteLine("device IP :" + nIp1 + "." + nIp2 + "." + nIp3 + "." + nIp4);
                Console.WriteLine(stGigEDeviceInfo.chSerialNumber);

            }
            else if (MyCamera.MV_USB_DEVICE == stDevInfo.nTLayerType)
            {
                MyCamera.MV_USB3_DEVICE_INFO stUsb3DeviceInfo = (MyCamera.MV_USB3_DEVICE_INFO)MyCamera.ByteToStruct(stDevInfo.SpecialInfo.stUsb3VInfo, typeof(MyCamera.MV_USB3_DEVICE_INFO));
                Console.WriteLine("\n" + ": [U3V] User Define Name : " + stUsb3DeviceInfo.chUserDefinedName);
                Console.WriteLine("\n Serial Number : " + stUsb3DeviceInfo.chSerialNumber);
                Console.WriteLine("\n Device Number : " + stUsb3DeviceInfo.nDeviceNumber);
            }
            return stDevInfo;
        }

        public static List<MV_CC_DEVICE_INFO> GetDeviceInfoListFull()
        {
            List<MV_CC_DEVICE_INFO> res = new();

            MV_CC_DEVICE_INFO_LIST stDevList = GetDeviceInfoList();
            foreach (var stDev in stDevList.pDeviceInfo)
            {
                res.Add(GetDeviveInfo(stDev));
            }

            return res;
        }


        //public static List<MV_CC_DEVICE_INFO> GetGigeDeviceInfoListFull()
        //{
        //    //List<MV_CC_DEVICE_INFO> res = new();

        //    //MV_CC_DEVICE_INFO_LIST stDevList = GetDeviceInfoList();
        //    //foreach (var stDev in stDevList.pDeviceInfo)
        //    //{
        //    //    res.Add(GetDeviveInfo(stDev));
        //    //}

        //    return res;
        //}

    }
}
