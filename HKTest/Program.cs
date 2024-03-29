﻿// See https://aka.ms/new-console-template for more information
using HK.NET;
using System.Drawing;
using System.Runtime.InteropServices;
using static HKTest.Class1;
var aaa = SciHKCore.GetDeviceInfoListFull();
// var c = new HKGigeTriggerCamera("00C19547937");
var c = new HKGigeTriggerCamera("00K07937647");
c.SetIp("192.168.1.88");
c.InitCamera();
c.SetADCGainEnable(true);
c.SetGain(0);
c.SetExposureTime(100000);
for (int i = 0; i < 100; i++)
{
    if (!c.CheckConnect())
    {

        Console.WriteLine("断连");
        c.DestroyDevice();
        //c.CloseDevice();
        c = new HKGigeTriggerCamera("00K07937647");
        c.InitCamera();
        c.SetExposureTime(10000);
    }
    if (c.GetImage(out var img))
    {
        File.WriteAllBytes($"测试1{i}.bmp", img);

    }
    Console.ReadKey();
}
return;
//var list = SciHKCore.GetDeviceInfoListFull();
//for (int i = 0; i < list.Count; i++)
//{
//    Console.WriteLine(list[i].nMajorVer);
    
//}

//var cc1 = HKTestMCam.CreateCameras(new List<string> { "00D08414152", "J58494175" });
//for (int i = 0; i < cc1.Count; i++)
//{
//    Console.WriteLine(cc1[i].InitCamera());
//}
////cc1[0].GetImages();
//Console.ReadKey();
//return;

////var vv = new HKGigeTriggerCamera("00J39700886");
//var vv = new HKGigeLineCamera("00J39700886");
//vv.InitCamera();
//vv.StartGrabbing();

//Console.ReadKey();
//vv.GetImage(out var ba);
////File.WriteAllBytes($"linliu.bmp", ba);


//return;
var cc = HKGigeTriggerCamera.CreateCameras(new List<string> { "00D08414152", "J58494175" });
//HKGigeCamera HKGigeCamera = new HKGigeCamera("00D08414152");
//cc[0].ConnectCamera();
cc[0].SetIp("192.168.1.159");
cc[1].SetIp("192.168.1.156");
for (int i = 0; i < cc.Count; i++)
{
    Console.WriteLine(cc[i].InitCamera());
}
for (int i = 0; i < cc.Count; i++)
{
    for (int j = 0; j < 10; j++)
    {
        //cc[i].SetExposureTime((uint)(1000 * (j + 1)));
        Console.WriteLine("连接状态: {0}", cc[i].CheckConnect());
        var aa = cc[i].GetImage(out byte[] img);

        Console.WriteLine($"拍照: {aa}"); 
        if (aa)
        File.WriteAllBytes($"linliu{j}+{i}.bmp", img);
        await Task.Delay(100);
        //display(path);
        //Console.ReadKey();
    }
    
}

static void display(string path)
{
    Point location = new Point(10, 10);
    Size imageSize = new Size(20, 10); // desired image size in characters
    using (Graphics g = Graphics.FromHwnd(GetConsoleWindow()))
    {
        using (Image image = Image.FromFile(path))
        {
            Size fontSize = GetConsoleFontSize();

            // translating the character positions to pixels
            Rectangle imageRect = new Rectangle(
                location.X * fontSize.Width,
                location.Y * fontSize.Height,
                imageSize.Width * fontSize.Width,
                imageSize.Height * fontSize.Height);
            g.DrawImage(image, imageRect);
        }
    }
}

 static Size GetConsoleFontSize()
{
    // getting the console out buffer handle
    IntPtr outHandle = CreateFile("CONOUT$", GENERIC_READ | GENERIC_WRITE,
        FILE_SHARE_READ | FILE_SHARE_WRITE,
        IntPtr.Zero,
        OPEN_EXISTING,
        0,
        IntPtr.Zero);
    int errorCode = Marshal.GetLastWin32Error();
    if (outHandle.ToInt32() == INVALID_HANDLE_VALUE)
    {
        throw new IOException("Unable to open CONOUT$", errorCode);
    }

    ConsoleFontInfo cfi = new ConsoleFontInfo();
    if (!GetCurrentConsoleFont(outHandle, false, cfi))
    {
        throw new InvalidOperationException("Unable to get font information.");
    }

    return new Size(cfi.dwFontSize.X, cfi.dwFontSize.Y);
}
