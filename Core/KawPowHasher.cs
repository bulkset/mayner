using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using RvnMiner.Models;
using RvnMiner.Utils;

namespace RvnMiner.Core
{
    public class KawPowHasher
    {
        private readonly AppConfig _config;
        private readonly Logger _logger;
        private readonly SHA256 _sha256;

        // Константы для алгоритма KawPoW
        private const int NONCE_SIZE = 8;
        private const int HASH_SIZE = 32;
        private const ulong MAX_TARGET = 0x1e00ffffUL; // Базовая сложность

        public KawPowHasher(AppConfig config)
        {
            _config = config;
            _logger = Logger.GetInstance();
            _sha256 = SHA256.Create();
        }

        public async Task<MiningResult> MineAsync(MiningJob job, CancellationToken cancellationToken)
        {
            byte[] nonce = new byte[NONCE_SIZE];
            ulong nonceValue = job.NonceStart;

            // Устанавливаем начальное значение nonce
            Array.Copy(BitConverter.GetBytes(nonceValue), nonce, NONCE_SIZE);

            while (nonceValue < job.NonceEnd && !cancellationToken.IsCancellationRequested)
            {
                // Создаем блок данных для хеширования
                byte[] blockData = CreateBlockData(job, nonce);

                // Вычисляем хеш блока
                byte[] hash = ComputeBlockHash(blockData);

                // Проверяем, удовлетворяет ли хеш цели
                if (IsValidHash(hash, job.Target))
                {
                    _logger.Info($"Найден валидный хеш! Nonce: {nonceValue}");

                    return new MiningResult
                    {
                        Nonce = nonce,
                        Hash = hash,
                        NonceValue = nonceValue
                    };
                }

                // Увеличиваем nonce
                nonceValue++;
                Array.Copy(BitConverter.GetBytes(nonceValue), nonce, NONCE_SIZE);

                // Периодически проверяем токен отмены
                if (nonceValue % 10000 == 0)
                {
                    await Task.Delay(1, cancellationToken);
                }
            }

            return null; // Не найдено решение в пределах диапазона
        }

        private byte[] CreateBlockData(MiningJob job, byte[] nonce)
        {
            // Создаем блок данных для майнинга
            // В реальности здесь должна быть полная структура блока Ravencoin
            using (var memoryStream = new MemoryStream())
            using (var writer = new BinaryWriter(memoryStream))
            {
                // Заголовок блока
                writer.Write(job.HeaderHash); // 32 байта
                writer.Write(nonce); // 8 байт nonce
                writer.Write((uint)0); // timestamp (упрощено)
                writer.Write((uint)0); // bits (упрощено)
                writer.Write((uint)0); // merkle root (упрощено)

                return memoryStream.ToArray();
            }
        }

        private byte[] ComputeBlockHash(byte[] blockData)
        {
            // Упрощенная реализация хеширования
            // В реальной реализации KawPoW здесь должна быть полная последовательность:
            // SHA3 -> ProgPoW -> SHA3 -> BLAKE2b

            // Первое SHA3 хеширование (упрощено как SHA256)
            byte[] firstHash = _sha256.ComputeHash(blockData);

            // Симуляция ProgPoW (в реальности это очень сложный алгоритм)
            byte[] progpowResult = SimulateProgPoW(firstHash);

            // Второе SHA3 хеширование
            byte[] secondHash = _sha256.ComputeHash(progpowResult);

            return secondHash;
        }

        private byte[] SimulateProgPoW(byte[] input)
        {
            // Упрощенная симуляция ProgPoW алгоритма
            // В реальности это включает:
            // - Инициализацию состояния
            // - Множественные раунды математических операций
            // - FNV хеширование
            // - Random math operations

            byte[] result = new byte[HASH_SIZE];
            Array.Copy(input, result, Math.Min(input.Length, HASH_SIZE));

            // Симулируем несколько раундов математических операций
            for (int i = 0; i < 64; i++)
            {
                uint value = BitConverter.ToUInt32(result, i % HASH_SIZE);
                value = RotateLeft(value, i % 32) ^ (uint)i;
                value = FNVHash(value, (uint)i);

                Array.Copy(BitConverter.GetBytes(value), 0, result, i % HASH_SIZE, 4);
            }

            return result;
        }

        private uint RotateLeft(uint value, int count)
        {
            return (value << count) | (value >> (32 - count));
        }

        private uint FNVHash(uint value, uint data)
        {
            const uint FNV_PRIME = 16777619;
            const uint FNV_OFFSET_BASIS = 2166136261;

            uint hash = FNV_OFFSET_BASIS;
            hash ^= value;
            hash *= FNV_PRIME;
            hash ^= data;
            hash *= FNV_PRIME;

            return hash;
        }

        private bool IsValidHash(byte[] hash, byte[] target)
        {
            // Проверяем, меньше ли хеш цели (для proof-of-work)
            // В Bitcoin/Ravencoin используется little-endian формат

            // Преобразуем big-endian хеш в little-endian для сравнения
            byte[] hashLE = new byte[HASH_SIZE];
            Array.Copy(hash, hashLE, HASH_SIZE);
            Array.Reverse(hashLE);

            // Сравниваем с целью
            for (int i = 0; i < HASH_SIZE; i++)
            {
                if (hashLE[i] < target[i])
                    return true;
                if (hashLE[i] > target[i])
                    return false;
            }

            return true; // Хеш равен цели
        }

        public static byte[] HexStringToByteArray(string hex)
        {
            int length = hex.Length;
            byte[] bytes = new byte[length / 2];

            for (int i = 0; i < length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }

            return bytes;
        }

        public static string ByteArrayToHexString(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
    }
}