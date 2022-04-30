using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GxIAPINET;
namespace DaHeng.NET
{
    public class DaHengCamera : IDisposable
    {
        public IGXFactory test => IGXFactory.GetInstance();
        public DaHengCamera()
        {
            IGXFactory.GetInstance().Init();
            
        }
        public void Dispose()
        {
            
        }
    }
}
