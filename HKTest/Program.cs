// See https://aka.ms/new-console-template for more information
using HK.NET;
using System.Drawing;
using System.Runtime.InteropServices;
using static HKTest.Class1;

var list = SciHKCore.GetDeviceInfoListFull();
for (int i = 0; i < list.Count; i++)
{
    Console.WriteLine(list[i].nMajorVer);
    
}

var vv = new HKGigeLineCamera("00J39700886");

vv.InitCamera();

Console.ReadKey();



return;
var cc = HKGigeTriggerCamera.CreateCameras(new List<string> { "00D08414152", "J58494175" });
//HKGigeCamera HKGigeCamera = new HKGigeCamera("00D08414152");
//cc[0].ConnectCamera();
cc[0].SetIp("192.168.1.184");
for (int i = 0; i < cc.Count; i++)
{
    Console.WriteLine(cc[i].InitCamera());
}
for (int i = 0; i < cc.Count; i++)
{
    for (int j = 0; j < 100; j++)
    {
        //cc[i].SetExposureTime((uint)(1000 * (j + 1)));
        Console.WriteLine($"拍照: {cc[i].GetImage(out byte[] img)}"); ;
        cc[i].CheckConnect();
        File.WriteAllBytes($"linliu{j}+{i}.bmp", img);
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
