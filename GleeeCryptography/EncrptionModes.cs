using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Gleee.Utils.Extension;
using System.Diagnostics;

namespace Gleee.Cryptography.EncrptionModes
{
    public class EncryptionAlgorithm
    {
        private static readonly RNGCryptoServiceProvider rng_csp = new RNGCryptoServiceProvider();

        private byte[] DefaultPadder(byte[] plain)
        {
            int pad = PlainBlockSize - (plain.Length % PlainBlockSize);
            var list = plain.ToList();
            for (int i = 0; i < pad; i++)
            {
                list.Add(0x00);
            }
            return list.ToArray();
        }
        private void DefaultRandomizer(byte[] iv) => rng_csp.GetBytes(iv);
        private byte[] PreProcessCBC(byte[] plain)
        {
            byte[] padded_plain;
            //调用补齐函数，如果没有指定则使用默认
            if (Padder == null)
                padded_plain = DefaultPadder(plain);
            else
                padded_plain = Padder(plain);
            var list = new List<byte>(padded_plain.Length + PlainBlockSize);
            list.AddRange(InitializeVector);
            list.AddRange(padded_plain);
            return list.ToArray();
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
            for (int i = 0; i < PlainBlockSize; i++)
            {
                right[right_index + i] = (byte)(left[left_index + i] ^ right[right_index + i]);
            }
        }

        /// <summary>
        /// 明文块长度
        /// </summary>
        public int PlainBlockSize { get; set; }
        /// <summary>
        /// 密钥块长度
        /// </summary>
        public byte[] InitializeVector { get; set; }
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
        /// 如果这个值为null，算法将调用默认的随机数生成器，后者使用填充0x00填充所有字节。
        /// </summary>
        public Action<byte[]> Randomizer { get; set; }

        public EncryptionAlgorithm(int block_size, Action<byte[], int, byte[]> encryptor, Action<byte[], int, byte[]> decryptor)
        {
            PlainBlockSize = block_size;
            Encryptor = encryptor;
            Decryptor = decryptor;
            InitializeVector = new byte[PlainBlockSize];
        }

        /// <summary>
        /// 以CBC模式加密明文二进制数组
        /// </summary>
        /// <param name="iv">初始向量。如果该参数所有字节为0x00，则算法会调用Randomizer随机化；否则将使用指定的初始向量。如果没有指定Randomizer将默认使用System.Security.Cryptography.RNGCryptoServiceProvider</param>
        /// <param name="plain">明文</param>
        /// <param name="key">密钥</param>
        /// <returns>解密后的</returns>
        public byte[] EncryptCBC(out byte[] iv, byte[] plain, byte[] key)
        {
            byte[] p = PreProcessCBC(plain);
            RandomizeIV();
            iv = InitializeVector;
            Debug.Print($"iv = {iv.ToHexString()}");
            int block_count = p.Length / PlainBlockSize;
            for (int i = 0; i < block_count; i++)
            {
                int this_block = i * PlainBlockSize;
                int pre_block = this_block - PlainBlockSize;

                if (i == 0)
                    XorBlock(InitializeVector, 0, p, this_block);
                else
                    XorBlock(p, pre_block, p, this_block);
                Encryptor(p, this_block, key);
            }
            return p.SubArray(InitializeVector.Length, p.Length - InitializeVector.Length);
        }
        public byte[] DecryptCBC(out byte[] iv, byte[] cipher, byte[] key)
        {
            byte[] c = (byte[])cipher.Clone();
            int block_count = c.Length / PlainBlockSize;
            for (int i = block_count - 1; i > 0; i--)
            {
                int this_block = i * PlainBlockSize;
                int pre_block = this_block - PlainBlockSize;

                Decryptor(c, this_block, key);
                if (i == 1)
                    XorBlock(InitializeVector, 0, c, this_block);
                else
                    XorBlock(c, pre_block, c, this_block);
            }
            iv = c.SubArray(0, PlainBlockSize);
            return c;
        }
    }
}
