using System;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelVerticalTab;

public class WindowMessageHandler : NativeWindow, IDisposable
{
    public event EventHandler? RefreshRequired;
    
    // Message that seems to be sent on renaming and other changes
    private static int AirSpaceNotificationMessage { get; } = (int)Helper.RegisterWindowMessage("AirSpace::Notification");

    public WindowMessageHandler(Excel.Application application)
    {
        var target = FindTarget(new IntPtr(application.Hwnd));
        if (target != IntPtr.Zero)
        {
            AssignHandle(target);
        }
    }

    private static IntPtr FindTarget(IntPtr hwnd)
    {
        var desk = Helper.FindWindowEx(hwnd, IntPtr.Zero, "XLDESK", null);
        return desk == IntPtr.Zero ? IntPtr.Zero : Helper.FindWindowEx(desk, IntPtr.Zero, "EXCEL7", null);
    }

    protected override void WndProc(ref Message m)
    {
        base.WndProc(ref m);

        if (m.Msg == AirSpaceNotificationMessage)
        {
            RefreshRequired?.Invoke(this, EventArgs.Empty);
        }
    }

    #region IDisposable Support
    private bool _disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            ReleaseHandle();
            _disposedValue = true;
        }
    }

    ~WindowMessageHandler()
    {
        if (Handle != IntPtr.Zero)
        {
            Dispose(false);
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    #endregion
}
