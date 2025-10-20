using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace RvnMiner.Utils
{
    public class FullscreenDetector
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("psapi.dll")]
        private static extern uint GetProcessImageFileName(IntPtr hProcess, System.Text.StringBuilder lpImageName, uint nSize);

        private const int SM_CXSCREEN = 0;
        private const int SM_CYSCREEN = 1;
        private const uint PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;

        private struct RECT
        {
            public int Left, Top, Right, Bottom;
        }

        public class FullscreenAppInfo
        {
            public bool IsFullscreen { get; set; }
            public string ProcessName { get; set; } = "";
            public string WindowTitle { get; set; } = "";
        }

        public static FullscreenAppInfo IsFullscreenForeground()
        {
            try
            {
                IntPtr hwnd = GetForegroundWindow();
                if (hwnd == IntPtr.Zero) return new FullscreenAppInfo { IsFullscreen = false };

                if (!IsWindowVisible(hwnd)) return new FullscreenAppInfo { IsFullscreen = false };

                GetWindowRect(hwnd, out RECT r);
                int screenW = GetSystemMetrics(SM_CXSCREEN);
                int screenH = GetSystemMetrics(SM_CYSCREEN);

                // Проверяем, занимает ли окно весь экран с небольшой погрешностью
                bool isFullscreen = (r.Left <= 0 && r.Top <= 0 &&
                                   (r.Right >= screenW - 20) &&
                                   (r.Bottom >= screenH - 20));

                if (isFullscreen)
                {
                    string processName = GetProcessName(hwnd);
                    return new FullscreenAppInfo
                    {
                        IsFullscreen = true,
                        ProcessName = processName,
                        WindowTitle = GetWindowTitle(hwnd)
                    };
                }

                return new FullscreenAppInfo { IsFullscreen = false };
            }
            catch (Exception)
            {
                return new FullscreenAppInfo { IsFullscreen = false };
            }
        }

        private static string GetProcessName(IntPtr hwnd)
        {
            try
            {
                GetWindowThreadProcessId(hwnd, out uint processId);
                IntPtr process = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, false, processId);

                if (process == IntPtr.Zero) return "unknown";

                var fileName = new System.Text.StringBuilder(1024);
                GetProcessImageFileName(process, fileName, (uint)fileName.Capacity);

                CloseHandle(process);

                string fullPath = fileName.ToString();
                return System.IO.Path.GetFileNameWithoutExtension(fullPath);
            }
            catch
            {
                return "unknown";
            }
        }

        private static string GetWindowTitle(IntPtr hwnd)
        {
            try
            {
                var title = new System.Text.StringBuilder(256);
                int length = 0;

                // Простая реализация получения заголовка окна
                return "Fullscreen Application";
            }
            catch
            {
                return "Fullscreen Application";
            }
        }

        public static async Task MonitorFullscreenAsync(Action<bool, string> onFullscreenChanged, CancellationToken cancellationToken)
        {
            string lastProcess = "";
            bool lastFullscreen = false;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var info = IsFullscreenForeground();

                    if (info.IsFullscreen != lastFullscreen || info.ProcessName != lastProcess)
                    {
                        lastFullscreen = info.IsFullscreen;
                        lastProcess = info.ProcessName;
                        onFullscreenChanged?.Invoke(info.IsFullscreen, info.ProcessName);
                    }

                    await Task.Delay(1000, cancellationToken); // Проверка каждую секунду
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка в мониторинге полноэкранного режима: {ex.Message}");
                    await Task.Delay(5000, cancellationToken);
                }
            }
        }
    }
}