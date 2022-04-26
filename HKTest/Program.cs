// See https://aka.ms/new-console-template for more information
using HK.NET;

var list = SciHKCore.GetDeviceInfoListFull();
for (int i = 0; i < list.Count; i++)
{
    Console.WriteLine(list[i].nMajorVer);
    
}
var cc = HKGigeCamera.CreateCameras(new List<string> { "00D08414152", "J58494175" });
//HKGigeCamera HKGigeCamera = new HKGigeCamera("00D08414152");
for (int i = 0; i < cc.Count; i++)
{
    cc[i].InitCamera();
}
for (int i = 0; i < cc.Count; i++)
{
    cc[i].GetImages();
}
Console.WriteLine("连接成功");
//Console.Read();