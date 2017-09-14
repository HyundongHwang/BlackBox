using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackBoxLib
{
    public interface IBlackBox
    {
        void Init(string azureStorageConnectionString);

        void CaptureScreen();

        void Flush();

        void e(string msg);

        void w(string msg);

        void i(string msg);

        void d(string msg);

        void v(string msg);

        void session(string sessionStr);
    }
}
