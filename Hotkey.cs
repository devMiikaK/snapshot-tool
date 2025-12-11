using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Snapshot_tool
{
    public class Hotkey : IDisposable
    {
        [DllImport("user32.dll")] //windows api
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private IntPtr _windowHandle;
        private int _currentId;
        private const int HOTKEY_ID = 9000;

        public Hotkey(IntPtr windowHandle)
        {
            _windowHandle = windowHandle;
        }

        public void Register(Keys key)
        {
            UnregisterHotKey(_windowHandle, HOTKEY_ID); //remove old hotkey before setting new one
            RegisterHotKey(_windowHandle, HOTKEY_ID, 0, (int)key);
            _currentId = (int)key;
        }

        public void Unregister()
        {
            UnregisterHotKey(_windowHandle, HOTKEY_ID);
        }

        public void Dispose()
        {
            Unregister();
        }

        //helper to check if hotkey was pressed
        public bool IsHotkeyMessage(Message m)
        {
            return m.Msg == 0x0312 && m.WParam.ToInt32() == HOTKEY_ID;
        }
    }
}