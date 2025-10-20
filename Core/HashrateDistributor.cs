using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RvnMiner.Models;
using RvnMiner.Utils;

namespace RvnMiner.Core
{
    public class HashrateDistributor
    {
        private readonly AppConfig _config;
        private readonly Logger _logger;
        private readonly ConcurrentDictionary<string, int> _primaryShares = new ConcurrentDictionary<string, int>();
        private readonly ConcurrentDictionary<string, int> _secondaryShares = new ConcurrentDictionary<string, int>();
        private readonly Random _random = new Random();
        private long _totalHashes = 0;
        private DateTime _lastReportTime = DateTime.Now;

        public event Action<HashrateInfo> HashrateUpdated;

        public HashrateDistributor(AppConfig config)
        {
            _config = config;
            _logger = Logger.GetInstance();
        }

        public async Task DistributeHashrateAsync(MiningResult result, MiningJob job)
        {
            Interlocked.Increment(ref _totalHashes);

            // Определяем, какому адресу отправить результат
            bool sendToPrimary = ShouldSendToPrimary();

            if (sendToPrimary)
            {
                await SubmitToPoolAsync(result, job, _config.Mining.PrimaryAddress);
                _primaryShares.AddOrUpdate(_config.Mining.PrimaryAddress, 1, (key, value) => value + 1);
            }
            else
            {
                await SubmitToPoolAsync(result, job, _config.Mining.SecondaryAddress);
                _secondaryShares.AddOrUpdate(_config.Mining.SecondaryAddress, 1, (key, value) => value + 1);
            }

            // Периодически отправляем отчет о хешрейте
            if ((DateTime.Now - _lastReportTime).TotalSeconds >= _config.Advanced.HashrateReportIntervalSeconds)
            {
                ReportHashrate();
            }
        }

        private bool ShouldSendToPrimary()
        {
            // Используем распределение по процентам
            int randomValue = _random.Next(1, 101); // 1-100
            return randomValue <= _config.Mining.PrimaryPercentage;
        }

        private async Task SubmitToPoolAsync(MiningResult result, MiningJob job, string address)
        {
            try
            {
                // Здесь будет реализация отправки результата в пул
                // Пока просто логируем
                _logger.Debug($"Отправка шары в пул для адреса: {address}");

                // Симуляция сетевого запроса
                await Task.Delay(10);

                // В реальности здесь будет:
                // - Формирование share данных
                // - Отправка на пул через stratum протокол
                // - Обработка ответа пула
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка при отправке шары для адреса {address}", ex);
            }
        }

        private void ReportHashrate()
        {
            try
            {
                double timeSpan = (DateTime.Now - _lastReportTime).TotalSeconds;
                if (timeSpan == 0) return;

                double totalHashes = Interlocked.Exchange(ref _totalHashes, 0);
                double hashrate = totalHashes / timeSpan; // hashes per second

                int primaryShares = _primaryShares.Values.Sum();
                int secondaryShares = _secondaryShares.Values.Sum();

                double primaryHashrate = (primaryShares / (double)(primaryShares + secondaryShares)) * hashrate;
                double secondaryHashrate = (secondaryShares / (double)(primaryShares + secondaryShares)) * hashrate;

                var hashrateInfo = new HashrateInfo
                {
                    TotalHashrate = hashrate,
                    PrimaryHashrate = primaryHashrate,
                    SecondaryHashrate = secondaryHashrate,
                    SharesFound = primaryShares + secondaryShares,
                    Timestamp = DateTime.Now
                };

                HashrateUpdated?.Invoke(hashrateInfo);

                _logger.Info($"Hashrate: {hashrate:F2} H/s | Primary: {primaryHashrate:F2} H/s ({_config.Mining.PrimaryPercentage}%) | Secondary: {secondaryHashrate:F2} H/s ({_config.Mining.SecondaryPercentage}%) | Shares: {primaryShares + secondaryShares}");

                _lastReportTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                _logger.Error("Ошибка при формировании отчета хешрейта", ex);
            }
        }

        public HashrateInfo GetCurrentHashrate()
        {
            double timeSpan = (DateTime.Now - _lastReportTime).TotalSeconds;
            if (timeSpan == 0) return new HashrateInfo();

            double totalHashes = Interlocked.Read(ref _totalHashes);
            double hashrate = totalHashes / timeSpan;

            int primaryShares = _primaryShares.Values.Sum();
            int secondaryShares = _secondaryShares.Values.Sum();

            double primaryHashrate = (primaryShares / (double)(primaryShares + secondaryShares)) * hashrate;
            double secondaryHashrate = (secondaryShares / (double)(primaryShares + secondaryShares)) * hashrate;

            return new HashrateInfo
            {
                TotalHashrate = hashrate,
                PrimaryHashrate = primaryHashrate,
                SecondaryHashrate = secondaryHashrate,
                SharesFound = primaryShares + secondaryShares,
                Timestamp = DateTime.Now
            };
        }

        public Dictionary<string, int> GetShareCounts()
        {
            return new Dictionary<string, int>
            {
                { _config.Mining.PrimaryAddress, _primaryShares.Values.Sum() },
                { _config.Mining.SecondaryAddress, _secondaryShares.Values.Sum() }
            };
        }
    }
}