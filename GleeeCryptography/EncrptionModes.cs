using System;
using System.Linq;
using System.Security.Cryptography;
using Gleee.Utils.Extension;
using System.Diagnostics;

namespace Gleee.Cryptography
{
    /// <summary>
    /// 分组加密算法类，用函数式方法实现
    /// </summary>
    //[Obsolete]
    public class EncryptionMode
    {
        private static readonly RNGCryptoServiceProvider rng_csp = new RNGCryptoServiceProvider();

        private byte[] DefaultPadder(byte[] plain)
        {
            int pad = BlockSize - (plain.Length % BlockSize);
            var list = plain.ToList();
            if (pad != 8)
            {
                for (int i = 0; i < pad; i++)
                {
                    list.Add(0x00);
                }
                return list.ToArray();
            }
            else return (byte[])plain.Clone();
        }
        private void DefaultRandomizer(byte[] iv) => rng_csp.GetBytes(iv);
        private byte[] PreProcessEBC(byte[] plain)
        {
            byte[] padded_plain;
            //调用补齐函数，如果没有指定则使用默认
            if (Padder == null)
                padded_plain = DefaultPadder(plain);
            else
                padded_plain = Padder(plain);
            return padded_plain;
        }
        private byte[] PreProcessCBC(byte[] plain)
        {
            byte[] padded_plain;
            //调用补齐函数，如果没有指定则使用默认
            if (Padder == null)
                padded_plain = DefaultPadder(plain);
            else
                padded_plain = Padder(plain);
            return padded_plain;
        }
        private void RandomizeIV()
        {
            if (Randomizer == null)
                DefaultRandomizer(InitializeVector);
            else
                Randomizer(InitializeVector);
        }
        private void XorBlock(byte[] left, int left_index, byte[] right, int right_index)
        {
            for (int i = 0; i < BlockSize; i++)
            {
                right[right_index + i] = (byte)(left[left_index + i] ^ right[right_index + i]);
            }
        }

        /// <summary>
        /// 明文块长度
        /// </summary>
        public int BlockSize { get; set; }
        /// <summary>
        /// 初始向量
        /// </summary>
        private byte[] InitializeVector { get; set; }
        /// <summary>
        /// 加密器。函数签名为void Encryptor(byte[] plain, int index, byte[] key)
        /// 其中，plain是包含明文的字节数组；index是明文在字节数组中的起点；key是密钥数组
        /// </summary>
        public Action<byte[], int, byte[]> Encryptor { get; set; }
        /// <summary>
        /// 解密器。函数签名为void Decryptor(byte[] cipher, int index, byte[] key)
        /// 其中，cipher是包含密文的字节数组；index是密文在字节数组中的起点；key是密钥数组
        /// </summary>
        public Action<byte[], int, byte[]> Decryptor { get; set; }
        /// <summary>
        /// 获取或设置该算法使用的填充器。函数签名为byte[] Padder(byte[] input)
        /// 其中，input为需要填充的原始字节数组。
        /// 这个函数需要将input填充为PlainBlockSize的整数倍并返回。
        /// 如果这个值为null，算法将调用默认的填充器，后者使用0x00填充末尾。
        /// </summary>
        public Func<byte[], byte[]> Padder { get; set; }
        /// <summary>
        /// 获取或设置该算法使用的随机数发生器。函数签名为void Randomizer(byte[] input)
        /// 其中，input为需要随机化的原始字节数组
        /// 如果这个值为null，算法将调用默认的随机数生成器System.Security.Cryptography.RNGCryptoServiceProvider。
        /// </summary>
        public Action<byte[]> Randomizer { get; set; }

        public EncryptionMode(int block_size, Action<byte[], int, byte[]> encryptor, Action<byte[], int, byte[]> decryptor)
        {
            BlockSize = block_size;
            Encryptor = encryptor;
            Decryptor = decryptor;
            InitializeVector = new byte[BlockSize];
        }

        /// <summary>
        /// 以CBC模式加密明文二进制数组
        /// </summary>
        /// <param name="iv">输出本次加密使用的初始向量。算法会使用Randomizer成员初始化这个数组</param>
        /// <param name="plain">明文</param>
        /// <param name="key">密钥</param>
        /// <returns>加密后的数据块（不包括初始向量）</returns>
        public byte[] EncryptCBC(out byte[] iv, byte[] plain, byte[] key)
        {
            byte[] p = PreProcessCBC(plain);
            RandomizeIV();
            iv = InitializeVector;
            int block_count = p.Length / BlockSize;
            for (int i = 0; i < block_count; i++)
            {
                int this_block = i * BlockSize;
                int pre_block = this_block - BlockSize;

                if (i == 0)
                    XorBlock(iv, 0, p, this_block);
                else
                    XorBlock(p, pre_block, p, this_block);
                Encryptor(p, this_block, key);
            }
            return p;
        }
        /// <summary>
        /// 以CBC模式解密密文二进制数组
        /// </summary>
        /// <param name="iv">初始向量。用来保存解密最后一个数据块得到的初始向量</param>
        /// <param name="cipher">密文</param>
        /// <param name="key">密钥</param>
        /// <returns>解密后的数据块</returns>
        public byte[] DecryptCBC(byte[] iv, byte[] cipher, byte[] key)
        {
            byte[] c = (byte[])cipher.Clone();
            int block_count = c.Length / BlockSize;
            for (int i = block_count - 1; i >= 0; i--)
            {
                int this_block = i * BlockSize;
                int pre_block = this_block - BlockSize;

                Decryptor(c, this_block, key);
                if (i == 0)
                    XorBlock(iv, 0, c, this_block);
                else
                    XorBlock(c, pre_block, c, this_block);
            }
            return c;
        }

        /// <summary>
        /// 以EBC模式加密明文二进制数组
        /// </summary>
        /// <param name="plain">明文</param>
        /// <param name="key">密钥</param>
        /// <returns>密文</returns>
        public byte[] EncryptEBC(byte[] plain, byte[] key)
        {
            byte[] p = PreProcessEBC(plain);
            int block_count = p.Length / BlockSize;
            for (int i = 0; i < block_count; i++)
            {
                Encryptor(p, i*BlockSize, key);
            }
            return p;
        }
        /// <summary>
        /// 以EBC模式解密密文二进制数组
        /// </summary>
        /// <param name="cipher">密文</param>
        /// <param name="key">密钥</param>
        /// <returns>明文</returns>
        public byte[] DecryptEBC(byte[] cipher, byte[] key)
        {
            byte[] c = (byte[])cipher.Clone();
            int block_count = c.Length / BlockSize;
            for (int i = block_count - 1; i > 0; i--)
            {
                Decryptor(c, i * BlockSize, key);
            }
            return c;
        }
    }

    /// <summary>
    /// 分组加密算法接口
    /// </summary>
    public interface IBlockEncryption
    {
        /// <summary>
        /// 分组长度，单位为字节
        /// </summary>
        int BlockSize { get; set; }

        /// <summary>
        /// 随机化器。用于在CBC模式中生成初始向量
        /// </summary>
        /// <param name="buffer">缓冲区</param>
        void Randomizer(byte[] buffer);
        /// <summary>
        /// 填充器。用于补齐数据长度至BlockSize的整数倍
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        byte[] Padder(byte[] input);
        /// <summary>
        /// 加密器。以块为单位，使用密钥key，加密bin中指定位置起始的数据。
        /// 加密结果需要直接覆写至相应的明文数据块。
        /// </summary>
        /// <param name="bin">包含明文的字节数组</param>
        /// <param name="plain_index">起始索引</param>
        /// <param name="key">密钥数据</param>
        void Encryptor(byte[] bin, int plain_index, byte[] key);
        /// <summary>
        /// 解密器。以块为单位，使用密钥key，解密bin中指定位置起始的数据。
        /// 解密结果需要直接覆写至相应的密文数据块。
        /// </summary>
        /// <param name="bin">包含密文的字节数组</param>
        /// <param name="cipher_index">起始索引</param>
        /// <param name="key">密钥数据</param>
        void Decryptor(byte[] bin, int cipher_index, byte[] key);
    }

    /// <summary>
    /// 分组加密算法接口扩展
    /// </summary>
    public static class EncrytionModes
    {
        private static void XorBlock(IBlockEncryption enc, byte[] left, int left_index, byte[] right, int right_index)
        {
            int BlockSize = enc.BlockSize;
            if (left_index + BlockSize > left.Length)
                throw new Exception($"异或式的左值长度不足，索引[{left_index}]之后的数据长度不足{BlockSize}");
            else if (right_index + BlockSize > right.Length)
                throw new Exception($"异或式的右值长度不足，索引[{right_index}]之后的数据长度不足{BlockSize}");

            for (int i = 0; i < enc.BlockSize; i++)
            {
                right[right_index + i] = (byte)(left[left_index + i] ^ right[right_index + i]);
            }
        }

        /// <summary>
        /// 以CBC模式加密明文二进制数组
        /// </summary>
        /// <param name="iv">输出本次加密使用的初始向量。算法会使用Randomizer成员初始化这个数组</param>
        /// <param name="plain">明文</param>
        /// <param name="key">密钥</param>
        /// <returns>加密后的数据块（不包括初始向量）</returns>
        public static byte[] EncryptCBC(this IBlockEncryption enc, out byte[] iv, byte[] plain, byte[] key)
        {
            int BlockSize = enc.BlockSize;
            byte[] p = enc.Padder(plain);
            iv = new byte[BlockSize];
            enc.Randomizer(iv); Debug.Print(iv.ToHexString());

            int block_count = p.Length / BlockSize;
            for (int i = 0; i < block_count; i++)
            {
                int this_block = i * BlockSize;
                int pre_block = this_block - BlockSize;

                if (i == 0)
                    XorBlock(enc, iv, 0, p, this_block);
                else
                    XorBlock(enc, p, pre_block, p, this_block);
                enc.Encryptor(p, this_block, key);
            }
            return p;
        }
        /// <summary>
        /// 以CBC模式解密密文二进制数组
        /// </summary>
        /// <param name="iv">初始向量。用于解密第一个数据块</param>
        /// <param name="cipher">密文</param>
        /// <param name="key">密钥</param>
        /// <returns>解密后的数据块（不包括初始向量）</returns>
        public static byte[] DecryptCBC(this IBlockEncryption enc, byte[] iv, byte[] cipher, byte[] key)
        {
            int BlockSize = enc.BlockSize;

            byte[] c = (byte[])cipher.Clone();
            int block_count = c.Length / BlockSize;
            for (int i = block_count - 1; i >= 0; i--)
            {
                int this_block = i * BlockSize;
                int pre_block = this_block - BlockSize;

                enc.Decryptor(c, this_block, key);
                if (i == 0)
                    XorBlock(enc, iv, 0, c, this_block);
                else
                    XorBlock(enc, c, pre_block, c, this_block);
            }
            return c;
        }

        /// <summary>
        /// 以EBC模式加密明文二进制数组
        /// </summary>
        /// <param name="plain">明文</param>
        /// <param name="key">密钥</param>
        /// <returns>密文</returns>
        public static byte[] EncryptEBC(this IBlockEncryption enc, byte[] plain, byte[] key)
        {
            int BlockSize = enc.BlockSize;
            byte[] p = enc.Padder(plain);
            int block_count = p.Length / BlockSize;
            for (int i = 0; i < block_count; i++)
            {
                enc.Encryptor(p, i * BlockSize, key);
            }
            return p;
        }
        /// <summary>
        /// 以EBC模式解密密文二进制数组
        /// </summary>
        /// <param name="cipher">密文</param>
        /// <param name="key">密钥</param>
        /// <returns>明文</returns>
        public static byte[] DecryptEBC(this IBlockEncryption enc, byte[] cipher, byte[] key)
        {
            int BlockSize = enc.BlockSize;
            byte[] c = (byte[])cipher.Clone();
            int block_count = c.Length / BlockSize;
            for (int i = block_count - 1; i > 0; i--)
            {
                enc.Decryptor(c, i * BlockSize, key);
            }
            return c;
        }
    }
}
