using System;
using System.Threading;
using System.Threading.Tasks;
using RvnMiner.Core;
using RvnMiner.Models;
using RvnMiner.Utils;

namespace RvnMiner
{
    class Program
    {
        private static MiningManager _miningManager;
        private static CancellationTokenSource _cancellationTokenSource;
        private static Logger _logger;

        static async Task Main(string[] args)
        {
            Console.Title = "RVN Miner";
            Console.WriteLine("=================================");
            Console.WriteLine("   RVN Майнер с разделением хешрейта");
            Console.WriteLine("=================================");
            Console.WriteLine();

            try
            {
                // Загружаем конфигурацию
                var config = Models.AppConfig.Load();
                _logger = Logger.GetInstance(
                    config.Logging.LogFilePath,
                    ParseLogLevel(config.Logging.LogLevel),
                    config.Logging.ConsoleOutput,
                    config.Logging.MaxLogSizeMB
                );

                _logger.Info("RVN Майнер запущен");

                // Проверяем права администратора для Windows Defender
                if (config.Security.AddToDefenderExclusions && !DefenderManager.IsAdministrator())
                {
                    _logger.Warning("Для добавления в исключения Windows Defender требуются права администратора");
                }

                // Создаем менеджер майнинга
                _miningManager = new MiningManager(config);

                // Подписываемся на события
                _miningManager.HashrateUpdated += OnHashrateUpdated;
                _miningManager.MiningStatusChanged += OnMiningStatusChanged;
                _miningManager.StatusMessage += OnStatusMessage;

                // Обрабатываем аргументы командной строки
                if (args.Length > 0)
                {
                    await ProcessCommandLineArgs(args);
                }
                else
                {
                    // Интерактивный режим
                    await RunInteractiveMode();
                }
            }
            catch (Exception ex)
            {
                _logger?.Error("Критическая ошибка в основном потоке", ex);
                Console.WriteLine($"Критическая ошибка: {ex.Message}");
                Console.WriteLine("Нажмите любую клавишу для выхода...");
                Console.ReadKey();
            }
        }

        private static async Task ProcessCommandLineArgs(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "--start":
                    case "-s":
                        await StartMining();
                        break;

                    case "--stop":
                    case "-t":
                        await StopMining();
                        break;

                    case "--status":
                    case "-u":
                        ShowStatus();
                        break;

                    case "--config":
                    case "-c":
                        ShowConfiguration();
                        break;

                    case "--help":
                    case "-h":
                        ShowHelp();
                        return;

                    default:
                        Console.WriteLine($"Неизвестный аргумент: {args[i]}");
                        ShowHelp();
                        return;
                }
            }
        }

        private static async Task RunInteractiveMode()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            Console.WriteLine("Доступные команды:");
            Console.WriteLine("1. start  - Запустить майнинг");
            Console.WriteLine("2. stop   - Остановить майнинг");
            Console.WriteLine("3. status - Показать статус");
            Console.WriteLine("4. config - Показать конфигурацию");
            Console.WriteLine("5. help   - Показать справку");
            Console.WriteLine("6. exit   - Выйти");
            Console.WriteLine();

            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                Console.Write("Команда: ");
                string command = Console.ReadLine()?.ToLower().Trim();

                switch (command)
                {
                    case "start":
                    case "1":
                        await StartMining();
                        break;

                    case "stop":
                    case "2":
                        await StopMining();
                        break;

                    case "status":
                    case "3":
                        ShowStatus();
                        break;

                    case "config":
                    case "4":
                        ShowConfiguration();
                        break;

                    case "help":
                    case "5":
                        ShowHelp();
                        break;

                    case "exit":
                    case "6":
                        await Shutdown();
                        return;

                    default:
                        Console.WriteLine("Неизвестная команда. Введите 'help' для справки.");
                        break;
                }

                await Task.Delay(100);
            }
        }

        private static async Task StartMining()
        {
            if (_miningManager.IsMining)
            {
                Console.WriteLine("Майнинг уже запущен");
                return;
            }

            try
            {
                await _miningManager.StartMiningAsync();
                Console.WriteLine("Майнинг запущен успешно");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при запуске майнинга: {ex.Message}");
                _logger?.Error("Ошибка при запуске майнинга", ex);
            }
        }

        private static async Task StopMining()
        {
            if (!_miningManager.IsMining)
            {
                Console.WriteLine("Майнинг не запущен");
                return;
            }

            try
            {
                await _miningManager.StopMiningAsync();
                Console.WriteLine("Майнинг остановлен");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при остановке майнинга: {ex.Message}");
                _logger?.Error("Ошибка при остановке майнинга", ex);
            }
        }

        private static void ShowStatus()
        {
            Console.WriteLine("=== СТАТУС МАЙНЕРА ===");

            if (_miningManager.IsMining)
            {
                Console.WriteLine("Статус: Майнинг активен");

                var hashrateInfo = GetCurrentHashrateInfo();
                if (hashrateInfo != null)
                {
                    Console.WriteLine($"Общий хешрейт: {hashrateInfo.TotalHashrate:F2} H/s");
                    Console.WriteLine($"Хешрейт основного адреса: {hashrateInfo.PrimaryHashrate:F2} H/s");
                    Console.WriteLine($"Хешрейт вторичного адреса: {hashrateInfo.SecondaryHashrate:F2} H/s");
                    Console.WriteLine($"Найдено шар: {hashrateInfo.SharesFound}");
                }
            }
            else
            {
                Console.WriteLine("Статус: Майнинг остановлен");
            }

            if (_miningManager.IsPaused)
            {
                Console.WriteLine("Причина паузы: Полноэкранное приложение активно");
            }

            Console.WriteLine();
        }

        private static void ShowConfiguration()
        {
            var config = Models.AppConfig.Load();

            Console.WriteLine("=== КОНФИГУРАЦИЯ ===");
            Console.WriteLine($"Основной адрес: {config.Mining.PrimaryAddress}");
            Console.WriteLine($"Вторичный адрес: {config.Mining.SecondaryAddress}");
            Console.WriteLine($"Распределение: {config.Mining.PrimaryPercentage}% / {config.Mining.SecondaryPercentage}%");
            Console.WriteLine($"Пул: {config.Mining.PoolUrl}");
            Console.WriteLine($"Автопауза при полноэкранном режиме: {config.Performance.AutoPauseOnFullscreen}");
            Console.WriteLine($"Добавление в исключения Defender: {config.Security.AddToDefenderExclusions}");
            Console.WriteLine();
        }

        private static void ShowHelp()
        {
            Console.WriteLine("=== СПРАВКА ===");
            Console.WriteLine("Команды:");
            Console.WriteLine("  start, -s, --start     - Запустить майнинг");
            Console.WriteLine("  stop, -t, --stop       - Остановить майнинг");
            Console.WriteLine("  status, -u, --status   - Показать статус");
            Console.WriteLine("  config, -c, --config   - Показать конфигурацию");
            Console.WriteLine("  help, -h, --help       - Показать эту справку");
            Console.WriteLine("  exit                   - Выйти из программы");
            Console.WriteLine();
            Console.WriteLine("Примеры использования:");
            Console.WriteLine("  rvn-miner.exe --start");
            Console.WriteLine("  rvn-miner.exe --status");
            Console.WriteLine();
        }

        private static async Task Shutdown()
        {
            Console.WriteLine("Завершение работы...");
            _cancellationTokenSource?.Cancel();

            if (_miningManager?.IsMining == true)
            {
                await _miningManager.StopMiningAsync();
            }

            _logger?.Info("RVN Майнер остановлен");
        }

        private static void OnHashrateUpdated(HashrateInfo info)
        {
            // Обновляем заголовок консоли с текущим хешрейтом
            Console.Title = $"RVN Miner - {info.TotalHashrate:F1} H/s";
        }

        private static void OnMiningStatusChanged(bool isActive)
        {
            Console.WriteLine($"Статус майнинга изменен: {(isActive ? "Активен" : "Остановлен")}");
        }

        private static void OnStatusMessage(string message)
        {
            Console.WriteLine($"[Статус] {message}");
        }

        private static HashrateInfo GetCurrentHashrateInfo()
        {
            // Здесь будет получение текущей информации о хешрейте
            // Пока возвращаем null
            return null;
        }

        private static LogLevel ParseLogLevel(string level)
        {
            return level?.ToLower() switch
            {
                "debug" => LogLevel.Debug,
                "info" => LogLevel.Info,
                "warning" => LogLevel.Warning,
                "error" => LogLevel.Error,
                "critical" => LogLevel.Critical,
                _ => LogLevel.Info
            };
        }
    }
}