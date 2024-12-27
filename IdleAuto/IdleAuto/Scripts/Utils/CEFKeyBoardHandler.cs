using CefSharp;
using System.Windows.Forms;
using System;

public class CEFKeyBoardHandler : IKeyboardHandler
{
    public bool OnKeyEvent(IWebBrowser browserControl, IBrowser browser, KeyType type, int windowsKeyCode, int nativeKeyCode, CefEventFlags modifiers, bool isSystemKey)
    {
        if (type == KeyType.KeyUp && Enum.IsDefined(typeof(Keys), windowsKeyCode))
        {
            var key = (Keys)windowsKeyCode;
            switch (key)
            {
                case Keys.F12:
                    browser.ShowDevTools();
                    break;
                case Keys.F5:
                    if (modifiers == CefEventFlags.ControlDown)
                    {
                        //MessageBox.Show("ctrl+f5");
                        browser.Reload(true); //强制忽略缓存
                    }
                    else
                    {
                        //MessageBox.Show("f5");
                        browser.Reload();
                    }
                    break;
            }
        }
        return false;
    }
    public bool OnPreKeyEvent(IWebBrowser browserControl, IBrowser browser, KeyType type, int windowsKeyCode, int nativeKeyCode, CefEventFlags modifiers, bool isSystemKey, ref bool isKeyboardShortcut)
    {
        return false;
    }
}