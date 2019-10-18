using System;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;
//_____________________________________________________________________________________________________________________________________________________________
//https://stackoverflow.com/questions/10296018/get-system-windows-forms-iwin32window-from-wpf-window/10296513#10296513
//https://stackoverflow.com/questions/2972513/printdialog-in-multithreaded-wpf-window-thrown-targetinvocationexception
namespace IseAddons {
    //_____________________________________________________________________________________________________________________________________________________________
    public class WindowWrapper : System.Windows.Forms.IWin32Window {
        private IntPtr _hwnd;
        //_____________________________________________________________________________________________________________________________________________________________
        public WindowWrapper(IntPtr handle) { _hwnd = handle; }
        //_____________________________________________________________________________________________________________________________________________________________
        public WindowWrapper(Window window) { _hwnd = new WindowInteropHelper(window).Handle; }
        //_____________________________________________________________________________________________________________________________________________________________
        public IntPtr Handle { get { return _hwnd; } }
        //_____________________________________________________________________________________________________________________________________________________________
        public static IntPtr GetSafeHandle() {
            PropertyInfo CriticalHandleProperty;
            var windowInteropType = typeof(WindowInteropHelper);
            CriticalHandleProperty = windowInteropType.GetProperty("CriticalHandle", BindingFlags.Instance | BindingFlags.NonPublic);

            var safeHandle = IntPtr.Zero;

            if (Application.Current != null) {
                Application.Current.Dispatcher.Invoke(new Action(() => {
                                            if (Application.Current.MainWindow != null) {
                                                var windowInteropHelper = new WindowInteropHelper(Application.Current.MainWindow);
                                                safeHandle = (IntPtr)CriticalHandleProperty.GetValue(windowInteropHelper, null);
                                                }
                                        }));
                }
            return safeHandle;
            }
        }
    }
