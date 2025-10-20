using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RvnMiner.Models;
using RvnMiner.Utils;

namespace RvnMiner.Core
{
    public class MiningManager
    {
        private readonly AppConfig _config;
        private readonly Logger _logger;
        private readonly HashrateDistributor _hashrateDistributor;
        private readonly FullscreenDetector _fullscreenDetector;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isMining = false;
        private bool _isPaused = false;

        public event Action<HashrateInfo> HashrateUpdated;
        public event Action<bool> MiningStatusChanged;
        public event Action<string> StatusMessage;

        public bool IsMining => _isMining && !_isPaused;
        public bool IsPaused => _isPaused;

        public MiningManager(AppConfig config)
        {
            _config = config;
            _logger = Logger.GetInstance(
                config.Logging.LogFilePath,
                ParseLogLevel(config.Logging.LogLevel),
                config.Logging.ConsoleOutput,
                config.Logging.MaxLogSizeMB
            );

            _hashrateDistributor = new HashrateDistributor(config);
            _fullscreenDetector = new FullscreenDetector();

            _hashrateDistributor.HashrateUpdated += OnHashrateUpdated;
        }

        public async Task StartMiningAsync()
        {
            if (_isMining) return;

            _logger.Info("Запуск майнера RVN...");

            try
            {
                // Проверяем конфигурацию
                ValidateConfiguration();

                // Добавляем в исключения Windows Defender если нужно
                if (_config.Security.AddToDefenderExclusions)
                {
                    await DefenderManager.AddToDefenderExclusionsAsync();
                }

                _cancellationTokenSource = new CancellationTokenSource();
                _isMining = true;

                // Запускаем мониторинг полноэкранных приложений
                if (_config.Performance.AutoPauseOnFullscreen)
                {
                    _ = MonitorFullscreenAsync(_cancellationTokenSource.Token);
                }

                // Запускаем майнинг
                await PerformMiningAsync(_cancellationTokenSource.Token);

            }
            catch (Exception ex)
            {
                _logger.Error("Ошибка при запуске майнера", ex);
                _isMining = false;
                throw;
            }
        }

        public async Task StopMiningAsync()
        {
            if (!_isMining) return;

            _logger.Info("Остановка майнера...");

            _cancellationTokenSource?.Cancel();
            _isMining = false;
            _isPaused = false;

            MiningStatusChanged?.Invoke(false);
            StatusMessage?.Invoke("Майнер остановлен");
        }

        public void PauseMining()
        {
            if (!_isMining || _isPaused) return;

            _logger.Info("Пауза майнера");
            _isPaused = true;
            MiningStatusChanged?.Invoke(false);
            StatusMessage?.Invoke("Майнер на паузе (полноэкранное приложение)");
        }

        public void ResumeMining()
        {
            if (!_isMining || !_isPaused) return;

            _logger.Info("Возобновление майнера");
            _isPaused = false;
            MiningStatusChanged?.Invoke(true);
            StatusMessage?.Invoke("Майнинг возобновлен");
        }

        private async Task PerformMiningAsync(CancellationToken cancellationToken)
        {
            var miningTasks = new List<Task>();

            // Определяем количество потоков
            int threadCount = _config.Performance.Threads > 0
                ? _config.Performance.Threads
                : Environment.ProcessorCount;

            _logger.Info($"Запуск майнинга с {threadCount} потоками");

            // Создаем задачи майнинга
            for (int i = 0; i < threadCount; i++)
            {
                var task = Task.Run(() => MineAsync(i, cancellationToken), cancellationToken);
                miningTasks.Add(task);
            }

            // Ждем завершения всех задач
            try
            {
                await Task.WhenAll(miningTasks);
            }
            catch (OperationCanceledException)
            {
                _logger.Info("Майнинг отменен");
            }
            catch (Exception ex)
            {
                _logger.Error("Ошибка в процессе майнинга", ex);
            }
        }

        private async Task MineAsync(int threadId, CancellationToken cancellationToken)
        {
            var kawpow = new KawPowHasher(_config);

            while (!cancellationToken.IsCancellationRequested && _isMining && !_isPaused)
            {
                try
                {
                    // Получаем задачу для майнинга
                    var job = await GetMiningJobAsync();
                    if (job == null) continue;

                    // Выполняем майнинг
                    var result = await kawpow.MineAsync(job, cancellationToken);

                    if (result != null)
                    {
                        // Распределяем результат между адресами
                        await _hashrateDistributor.DistributeHashrateAsync(result, job);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Ошибка в потоке майнинга {threadId}", ex);
                    await Task.Delay(1000, cancellationToken);
                }
            }
        }

        private async Task<MiningJob> GetMiningJobAsync()
        {
            // Здесь будет реализация получения задач от пула
            // Пока возвращаем тестовую задачу
            await Task.Delay(100); // Симуляция сетевого запроса

            return new MiningJob
            {
                JobId = "test_job",
                HeaderHash = new byte[32],
                Target = new byte[32],
                NonceStart = 0,
                NonceEnd = 1000000
            };
        }

        private async Task MonitorFullscreenAsync(CancellationToken cancellationToken)
        {
            await FullscreenDetector.MonitorFullscreenAsync((isFullscreen, processName) =>
            {
                if (isFullscreen && _config.Performance.PauseOnGameDetected)
                {
                    if (!_isPaused)
                    {
                        PauseMining();
                    }
                }
                else if (!isFullscreen && _isPaused)
                {
                    ResumeMining();
                }

                if (isFullscreen)
                {
                    _logger.Info($"Обнаружено полноэкранное приложение: {processName}");
                }
            }, cancellationToken);
        }

        private void ValidateConfiguration()
        {
            if (string.IsNullOrEmpty(_config.Mining.PrimaryAddress))
                throw new ArgumentException("Не указан основной адрес для майнинга");

            if (string.IsNullOrEmpty(_config.Mining.SecondaryAddress))
                throw new ArgumentException("Не указан вторичный адрес для майнинга");

            if (_config.Mining.PrimaryPercentage + _config.Mining.SecondaryPercentage != 100)
                throw new ArgumentException("Сумма процентов распределения должна равняться 100");

            if (string.IsNullOrEmpty(_config.Mining.PoolUrl))
                throw new ArgumentException("Не указан URL пула");
        }

        private void OnHashrateUpdated(HashrateInfo info)
        {
            HashrateUpdated?.Invoke(info);
        }

        private LogLevel ParseLogLevel(string level)
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

    public class HashrateInfo
    {
        public double TotalHashrate { get; set; }
        public double PrimaryHashrate { get; set; }
        public double SecondaryHashrate { get; set; }
        public int SharesFound { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class MiningJob
    {
        public string JobId { get; set; }
        public byte[] HeaderHash { get; set; }
        public byte[] Target { get; set; }
        public ulong NonceStart { get; set; }
        public ulong NonceEnd { get; set; }
    }

    public class MiningResult
    {
        public byte[] Nonce { get; set; }
        public byte[] Hash { get; set; }
        public ulong NonceValue { get; set; }
    }
}