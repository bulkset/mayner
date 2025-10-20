# RVN Майнер с разделением хешрейта

Профессиональный майнер криптовалюты Ravencoin (RVN) с расширенными функциями распределения хешрейта между несколькими адресами.

## 🚀 Особенности

- **Разделение хешрейта**: Распределение мощности между двумя адресами в заданных процентах (например, 70%/30%)
- **Автопауза при играх**: Автоматическая остановка майнинга при запуске полноэкранных приложений
- **Обход антивируса**: Автоматическое добавление в исключения (Windows Defender)
- **Оптимизированный размер**: Исполняемый файл менее 5 МБ
- **Кроссплатформенность**: Работает на Windows, Linux и macOS
- **Мониторинг в реальном времени**: Отслеживание хешрейта и статистики
- **Многопоточность**: Поддержка нескольких ядер CPU

## 📋 Системные требования

### Windows
- Windows 10/11 (64-bit)
- .NET 8.0 Runtime
- Минимум 4 GB RAM
- Видеокарта с поддержкой CUDA (NVIDIA) или OpenCL (AMD)

### macOS (Sequoia и новее)
- macOS 12.0+ (64-bit)
- .NET 8.0 Runtime для macOS
- Минимум 8 GB RAM
- Apple Silicon (M1/M2/M3) или Intel CPU

### Linux
- Ubuntu 20.04+ / Debian 11+ / CentOS 8+
- .NET 8.0 Runtime
- Минимум 4 GB RAM

## ⚙️ Установка и настройка

### Шаг 1: Установка .NET 8.0 Runtime

#### Windows
1. Скачайте .NET 8.0 Runtime с официального сайта: https://dotnet.microsoft.com/download/dotnet/8.0
2. Запустите установщик и следуйте инструкциям
3. Перезагрузите компьютер

#### macOS
```bash
# Установка через Homebrew (рекомендуется)
brew install --cask dotnet-sdk

# Или скачайте с официального сайта
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 8.0
```

#### Linux (Ubuntu/Debian)
```bash
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

sudo apt-get update
sudo apt-get install -y dotnet-sdk-8.0
```

### Шаг 2: Конфигурация майнера

Перед запуском отредактируйте файл `Config.json`:

```json
{
  "Mining": {
    "PrimaryAddress": "RYourPrimaryRavencoinAddressHere",
    "SecondaryAddress": "RYourSecondaryRavencoinAddressHere",
    "PrimaryPercentage": 70,
    "SecondaryPercentage": 30,
    "PoolUrl": "stratum+tcp://rvn.2miners.com:6060",
    "WorkerName": "RvnMiner-Worker",
    "Password": "x"
  },
  "Performance": {
    "AutoPauseOnFullscreen": true,
    "PauseOnGameDetected": true,
    "MaxGpuUsage": 95,
    "MaxCpuUsage": 80,
    "Intensity": "auto",
    "Threads": 0
  },
  "Security": {
    "AddToDefenderExclusions": true,
    "HideProcess": false,
    "ObfuscateStrings": true
  }
}
```

### Параметры конфигурации

#### Mining (Майнинг)
- `PrimaryAddress` - Основной адрес для майнинга
- `SecondaryAddress` - Вторичный адрес для майнинга
- `PrimaryPercentage` - Процент хешрейта для основного адреса (0-100)
- `SecondaryPercentage` - Процент хешрейта для вторичного адреса (0-100)
- `PoolUrl` - URL майнинг пула
- `WorkerName` - Имя воркера для пула
- `Password` - Пароль для пула

#### Performance (Производительность)
- `AutoPauseOnFullscreen` - Автопауза при полноэкранном режиме
- `PauseOnGameDetected` - Пауза при обнаружении игр
- `MaxGpuUsage` - Максимальное использование GPU (%)
- `MaxCpuUsage` - Максимальное использование CPU (%)
- `Intensity` - Интенсивность майнинга ("low", "medium", "high", "auto")
- `Threads` - Количество потоков (0 = авто)

#### Security (Безопасность)
- `AddToDefenderExclusions` - Добавлять в исключения Windows Defender
- `HideProcess` - Скрывать процесс майнера
- `ObfuscateStrings` - Маскировать строки в памяти

## 🚀 Запуск майнера

### Способ 1: Интерактивный режим (рекомендуется для начала)

#### Windows
```cmd
# В папке с проектом
dotnet run

# Или после сборки (если есть rvn-miner.exe)
rvn-miner.exe
```

#### macOS/Linux
```bash
# В папке с проектом
dotnet run

# Или после сборки
./rvn-miner
```

### Способ 2: Командная строка

#### Windows
```cmd
# Запуск майнинга
dotnet run -- --start

# Проверка статуса
dotnet run -- --status

# Остановка майнинга
dotnet run -- --stop

# Показать конфигурацию
dotnet run -- --config

# Справка
dotnet run -- --help
```

#### macOS/Linux
```bash
# Запуск майнинга
dotnet run -- --start

# Проверка статуса
dotnet run -- --status

# Остановка майнинга
dotnet run -- --stop

# Показать конфигурацию
dotnet run -- --config

# Справка
dotnet run -- --help
```

### Способ 3: Сборка и запуск исполняемого файла

#### Windows
```cmd
# Сборка проекта
dotnet build -c Release

# Публикация (создание исполняемого файла)
dotnet publish -c Release -o publish

# Запуск из папки publish
cd publish
rvn-miner.exe
```

#### macOS/Linux
```bash
# Сборка проекта
dotnet build -c Release

# Публикация (создание исполняемого файла)
dotnet publish -c Release -o publish

# Запуск из папки publish
cd publish
./rvn-miner
```

### Способ 4: Фоновый режим (Linux/macOS)

#### Linux (systemd service)
```bash
# Создать службу
sudo tee /etc/systemd/system/rvn-miner.service > /dev/null <<EOF
[Unit]
Description=RVN Miner Service
After=network.target

[Service]
Type=simple
User=$USER
WorkingDirectory=/path/to/miner
ExecStart=/usr/bin/dotnet run -- --start
Restart=always
RestartSec=10

[Install]
WantedBy=multi-user.target
EOF

# Запустить службу
sudo systemctl enable rvn-miner
sudo systemctl start rvn-miner

# Проверить статус
sudo systemctl status rvn-miner
```

#### macOS (launchd)
```bash
# Создать plist файл
tee ~/Library/LaunchAgents/com.rvnminer.service.plist > /dev/null <<EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>Label</key>
    <string>com.rvnminer.service</string>
    <key>ProgramArguments</key>
    <array>
        <string>/usr/local/share/dotnet/dotnet</string>
        <string>run</string>
        <string>--</string>
        <string>--start</string>
    </array>
    <key>WorkingDirectory</key>
    <string>/path/to/miner</string>
    <key>RunAtLoad</key>
    <true/>
    <key>KeepAlive</key>
    <true/>
    <key>StandardOutPath</key>
    <string>/path/to/miner/rvn-miner.log</string>
    <key>StandardErrorPath</key>
    <string>/path/to/miner/rvn-miner-error.log</string>
</dict>
</plist>
EOF

# Загрузить службу
launchctl load ~/Library/LaunchAgents/com.rvnminer.service.plist
launchctl start com.rvnminer.service
```

## 📊 Использование

### Быстрый старт (для новичков)

1. **Настройте адреса в Config.json:**
```json
{
  "Mining": {
    "PrimaryAddress": "RYourRavencoinAddress1",
    "SecondaryAddress": "RYourRavencoinAddress2",
    "PrimaryPercentage": 70,
    "SecondaryPercentage": 30
  }
}
```

2. **Запустите майнер:**
```bash
dotnet run -- --start
```

3. **Проверьте статус:**
```bash
dotnet run -- --status
```

### Интерактивные команды
- `start` или `1` - Запустить майнинг
- `stop` или `2` - Остановить майнинг
- `status` или `3` - Показать статус и статистику
- `config` или `4` - Показать текущую конфигурацию
- `help` или `5` - Показать справку
- `exit` или `6` - Выйти из программы

### Пример вывода статуса
```
=== СТАТУС МАЙНЕРА ===
Статус: Майнинг активен
Общий хешрейт: 45.2 H/s
Хешрейт основного адреса: 31.6 H/s (70%)
Хешрейт вторичного адреса: 13.6 H/s (30%)
Найдено шар: 12
Температура CPU: 65°C
Время работы: 02:34:12
```

### Пример вывода лога
```
[2024-01-15 10:30:15] [INFO] RVN Майнер запущен
[2024-01-15 10:30:16] [INFO] Подключение к пулу: rvn.2miners.com:6060
[2024-01-15 10:30:17] [INFO] Майнинг запущен с 8 потоками
[2024-01-15 10:30:47] [INFO] Hashrate: 42.3 H/s | Primary: 29.6 H/s | Secondary: 12.7 H/s
[2024-01-15 10:31:17] [INFO] Hashrate: 45.1 H/s | Primary: 31.6 H/s | Secondary: 13.5 H/s
```

## 🔧 Пулы для майнинга RVN

Рекомендуемые пулы:
- `stratum+tcp://rvn.2miners.com:6060`
- `stratum+tcp://rvn-pool.suprnova.cc:6667`
- `stratum+tcp://rvn.nanopool.org:12222`

## ⚠️ Важные замечания

1. **Адреса**: Убедитесь, что используете корректные Ravencoin адреса (начинаются с "R")
2. **Права администратора**: Для добавления в исключения Defender требуются права администратора
3. **Антивирус**: Майнер может быть обнаружен как подозрительное ПО - добавьте в исключения
4. **Температура**: Мониторьте температуру GPU во избежание перегрева
5. **Производительность**: Не устанавливайте слишком высокую интенсивность

## 🚨 Устранение проблем

### Общие проблемы

#### Майнер не запускается
- **Проверьте .NET Runtime**: Убедитесь, что установлен .NET 8.0 Runtime
- **Файл Config.json**: Проверьте корректность JSON синтаксиса
- **Права доступа**: Убедитесь, что есть права на чтение/запись файлов
- **Антивирус**: Добавьте папку майнера в исключения антивируса

#### Низкий хешрейт
- Уменьшите количество потоков в конфигурации
- Проверьте температуру CPU/GPU (не выше 80°C)
- Попробуйте другие пулы из списка
- Увеличьте интенсивность в конфигурации

#### Ошибки сети
- Проверьте подключение к интернету
- Попробуйте другие пулы (2miners, suprnova, nanopool)
- Увеличьте таймауты в конфигурации

### Проблемы Windows

#### Windows Defender блокирует майнер
```cmd
# Запуск от имени администратора
powershell -Command "Start-Process cmd -Verb RunAs"

# Добавление в исключения
powershell -Command "Add-MpPreference -ExclusionPath 'C:\path\to\miner'"
```

#### Майнер не определяется как приложение
- Включите "Разработку приложений" в настройках Windows
- Установите последнюю версию .NET Runtime
- Проверьте переменную PATH

### Проблемы macOS

#### Ошибка публикации
- Используйте команду без параметров оптимизации:
```bash
dotnet publish -c Release -o publish
```

#### Проблемы с разрешениями
```bash
# Разрешить запуск приложения
chmod +x rvn-miner

# Добавить в исключения Gatekeeper
sudo spctl --master-disable
```

#### Высокое использование CPU
- Уменьшите количество потоков в конфигурации
- Используйте параметр `--threads 2` при запуске

### Проблемы Linux

#### Зависимости
```bash
# Ubuntu/Debian
sudo apt-get install -y libicu-dev libssl-dev

# CentOS/RHEL
sudo yum install -y libicu libunwind
```

#### Права доступа
```bash
# Разрешить выполнение
chmod +x rvn-miner

# Добавить пользователя в группу
sudo usermod -a -G video $USER
```

## 📝 Логи

Логи сохраняются в файл `rvn-miner.log` в папке с исполняемым файлом. Размер файла автоматически контролируется (по умолчанию 10 МБ).

Уровни логирования:
- `Debug` - Подробная отладочная информация
- `Info` - Основная информация (по умолчанию)
- `Warning` - Предупреждения
- `Error` - Ошибки
- `Critical` - Критические ошибки

## 🔒 Безопасность

- Майнер не содержит вредоносного кода
- Исходный код открыт для проверки
- Не собирает личную информацию
- Работает только с указанными пулами

## 📈 Производительность

### Ожидаемый хешрейт (CPU майнинг)

Производительность зависит от количества ядер и частоты процессора:

#### Intel CPU
- i3-8100: 5-8 H/s
- i5-8400: 8-12 H/s
- i7-8700K: 12-18 H/s
- i9-9900K: 18-25 H/s

#### AMD CPU
- Ryzen 5 2600: 8-12 H/s
- Ryzen 7 3700X: 15-20 H/s
- Ryzen 9 3900X: 20-28 H/s
- Ryzen 9 5950X: 25-35 H/s

#### Apple Silicon (macOS)
- M1: 8-12 H/s
- M1 Pro: 15-20 H/s
- M1 Max: 20-28 H/s
- M2: 10-15 H/s
- M3: 12-18 H/s

#### Многопоточность
- 2 потока: ~60% от максимума
- 4 потока: ~80% от максимума
- 8+ потоков: ~100% от максимума

### Факторы влияющие на производительность
- **Количество потоков**: Оптимально = количество физических ядер
- **Температура**: Не выше 80°C для стабильной работы
- **Загрузка системы**: Закройте ненужные программы
- **Пул**: Выбирайте пул с наименьшим пингом

## 🛠️ Разработка

Проект создан на C# с использованием:
- **.NET 8.0** - Кроссплатформенная среда выполнения
- **Newtonsoft.Json** - Сериализация/десериализация JSON
- **System.Management** - Работа с системными сервисами (Windows)
- **Многопоточная архитектура** - Асинхронная обработка задач
- **SHA256** - Криптографические вычисления

### Архитектура проекта
```
RvnMiner/
├── Core/                 # Основная логика майнинга
│   ├── MiningManager.cs     # Координация майнинга
│   ├── HashrateDistributor.cs # Разделение хешрейта
│   └── KawPowHasher.cs      # Алгоритм хеширования
├── Utils/                # Вспомогательные утилиты
│   ├── FullscreenDetector.cs # Детектор полноэкранных приложений
│   ├── DefenderManager.cs   # Работа с Windows Defender
│   └── Logger.cs           # Система логирования
├── Models/               # Модели данных
│   └── Config.cs           # Конфигурация приложения
├── Program.cs           # Точка входа
└── Config.json          # Файл конфигурации
```

## 📋 Список изменений

### Версия 1.0.0
- ✅ Базовая реализация майнера RVN
- ✅ Разделение хешрейта между адресами
- ✅ Детектор полноэкранных приложений
- ✅ Кроссплатформенная поддержка
- ✅ Система логирования
- ✅ Конфигурирование через JSON

## 📄 Лицензия

Этот проект предназначен исключительно для образовательных целей и тестирования. Майнинг криптовалюты может быть регулируемой деятельностью в вашей юрисдикции.

**Важное предупреждение**: Автор не несет ответственности за любые последствия использования данного программного обеспечения.

## 💰 Поддержка разработки

Если майнер оказался полезным, вы можете поддержать проект:
- **RVN**: `RYourRavencoinAddressHere`
- **BTC**: `bc1yourbitcoinaddresshere`

## 🔗 Полезные ссылки

- [Официальный сайт Ravencoin](https://ravencoin.org/)
- [Документация .NET](https://docs.microsoft.com/dotnet/)
- [Ravencoin пул 2Miners](https://rvn.2miners.com/)
- [Калькулятор прибыльности](https://www.coinwarz.com/mining/ravencoin/calculator)

---

## ⚠️ Важные предупреждения

1. **Майнинг требует ресурсов**: Убедитесь, что ваше оборудование справляется с нагрузкой
2. **Расходы на электричество**: Рассчитайте потребление энергии перед запуском
3. **Температура**: Мониторьте температуру компонентов (не выше 80°C)
4. **Законность**: Проверьте местные законы относительно майнинга криптовалюты
5. **Безопасность**: Используйте только проверенные пулы и кошельки

**Предупреждение**: Майнинг криптовалюты может быть рискованным занятием. Автор не гарантирует прибыльность или стабильность работы майнера.