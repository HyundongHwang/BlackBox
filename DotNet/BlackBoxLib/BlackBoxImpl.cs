using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BlackBoxLib
{
    public class BlackBoxImpl : IBlackBox
    {
        public void CaptureScreen()
        {
            BlackBox.CaptureScreen();
        }

        public void d(string msg)
        {
            BlackBox.d(msg);
        }

        public void e(string msg)
        {
            BlackBox.e(msg);
        }

        public void Flush()
        {
            BlackBox.Flush();
        }

        public void i(string msg)
        {
            BlackBox.i(msg);
        }

        public void Init(string azureStorageConnectionString)
        {
            BlackBox.Init(azureStorageConnectionString);
        }

        public void session(string sessionStr)
        {
            BlackBox.session(sessionStr);
        }

        public void v(string msg)
        {
            BlackBox.v(msg);
        }

        public void w(string msg)
        {
            BlackBox.w(msg);
        }
    }
}
