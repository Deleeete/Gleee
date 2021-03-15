using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Gleee.Cryptography
{
    /// <summary>
    /// TEA算法类
    /// </summary>
    public class TEA
    {
        /// <summary>
        /// TEA倍数常数，默认为0x9e3779b9
        /// </summary>
        public uint Delta { get; private set; } = 0x9e3779b9;

        public TEA() { }
        public TEA(uint delta)
        {
            Delta = delta;
        }

        /// <summary>
        /// 加密一个8字节密文块。循环32次。密文块和密钥块均以uint数组形式提供。
        /// </summary>
        /// <param name="plain">密文块</param>
        /// <param name="key">密钥块</param>
        public void EncryptUint(uint[] plain, uint[] key)
        {
            uint y = plain[0], z = plain[1], sum = 0;
            uint a = key[0], b = key[1], c = key[2], d = key[3];
            for (int i = 0; i < 32; i++)
            {
                sum += Delta;
                y += ((z << 2) + a) ^ (z + sum) ^ ((z >> 7) + b);
                z += ((y << 2) + c) ^ (y + sum) ^ ((y >> 7) + d);
            }
            plain[0] = y;
            plain[1] = z;
        }
        /// <summary>
        /// 加密一个8字节密文块，循环次数为32。密文和密钥均来源于字节数组，分别由各自的索引指定起始点。
        /// </summary>
        /// <param name="n">TEA加密器的循环次数</param>
        /// <param name="plain">保存有密文块的字节数组</param>
        /// <param name="plain_index">密文块的起始索引</param>
        /// <param name="key">保存有密钥块的字节数组</param>
        /// <param name="key_index">密钥块的起始索引</param>
        /// <returns>加密后的字节数组</returns>
        public byte[] Encryptor(byte[] plain, int plain_index, byte[] key, int key_index)
        {
            uint y = BitConverter.ToUInt32(plain, plain_index);
            uint z = BitConverter.ToUInt32(plain, plain_index + sizeof(uint));

            uint a = BitConverter.ToUInt32(key, key_index);
            uint b = BitConverter.ToUInt32(key, key_index + sizeof(uint));
            uint c = BitConverter.ToUInt32(key, key_index + 2 * sizeof(uint));
            uint d = BitConverter.ToUInt32(key, key_index + 3 * sizeof(uint));

            uint sum = 0;
            for (int i = 0; i < 32; i++)
            {
                sum += Delta;
                y += ((z << 2) + a) ^ (z + sum) ^ ((z >> 7) + b);
                z += ((y << 2) + c) ^ (y + sum) ^ ((y >> 7) + d);
            }
            Debug.Print($"delta = {Delta:x4}\tsum = {sum:x4}");
            return GetBytes(y, z);
        }
        /// <summary>
        /// 加密一个8字节密文块，循环次数为n。密文和密钥均来源于字节数组，分别由各自的索引指定起始点。
        /// </summary>
        /// <param name="n">TEA加密器的循环次数</param>
        /// <param name="plain">保存有密文块的字节数组</param>
        /// <param name="plain_index">密文块的起始索引</param>
        /// <param name="key">保存有密钥块的字节数组</param>
        /// <param name="key_index">密钥块的起始索引</param>
        /// <returns>加密后的字节数组</returns>
        public byte[] Encryptor(int n, byte[] plain, int plain_index, byte[] key, int key_index)
        {
            uint y = BitConverter.ToUInt32(plain, plain_index);
            uint z = BitConverter.ToUInt32(plain, plain_index + sizeof(uint));

            uint a = BitConverter.ToUInt32(key, key_index);
            uint b = BitConverter.ToUInt32(key, key_index + sizeof(uint));
            uint c = BitConverter.ToUInt32(key, key_index + 2*sizeof(uint));
            uint d = BitConverter.ToUInt32(key, key_index + 3*sizeof(uint));

            uint sum = 0;
            for (int i = 0; i < n; i++)
            {
                sum += Delta;
                y += ((z << 2) + a) ^ (z + sum) ^ ((z >> 7) + b);
                z += ((y << 2) + c) ^ (y + sum) ^ ((y >> 7) + d);
            }
            Debug.Print($"delta = {Delta:x4}\tsum = {sum:x4}");
            return GetBytes(y, z);
        }
        /// <summary>
        /// 加密一个8字节密文块，循环次数为n。密文和密钥均来源于字节数组，且起始于索引0。
        /// </summary>
        /// <param name="n">TEA加密器的循环次数</param>
        /// <param name="plain">保存有密文块的字节数组</param>
        /// <param name="key">保存有密钥块的字节数组</param>
        /// <returns>加密后的字节数组</returns>
        public byte[] Encryptor(int n, byte[] plain, byte[] key) => Encryptor(n, plain, 0, key, 0);

        /// <summary>
        /// 解密一个8字节密文块。循环32次。密文块和密钥块均以uint数组形式提供。
        /// </summary>
        /// <param name="plain">密文块</param>
        /// <param name="key">密钥块</param>
        public void DecryptUint(uint[] cipher, uint[] key)
        {
            uint y = cipher[0], z = cipher[1], sum = 0xC6EF3720;
            uint a = key[0], b = key[1], c = key[2], d = key[3];

            for (int i = 0; i < 32; i++)
            {
                z -= ((y << 2) + c) ^ (y + sum) ^ ((y >> 7) + d);
                y -= ((z << 2) + a) ^ (z + sum) ^ ((z >> 7) + b);
                sum -= Delta;
            }
            cipher[0] = y;
            cipher[1] = z;
        }
        /// <summary>
        /// 解密一个8字节密文块，循环次数为32。密文和密钥均来源于字节数组，分别由各自的索引指定起始点。
        /// </summary>
        /// <param name="n">TEA解密器的循环次数</param>
        /// <param name="cipher">保存有密文块的字节数组</param>
        /// <param name="cipher_index">密文块的起始索引</param>
        /// <param name="key">保存有密钥块的字节数组</param>
        /// <param name="key_index">密钥块的起始索引</param>
        /// <returns>解密后的字节数组</returns>
        public byte[] Decryptor(byte[] cipher, int cipher_index, byte[] key, int key_index)
        {
            uint y = BitConverter.ToUInt32(cipher, cipher_index);
            uint z = BitConverter.ToUInt32(cipher, cipher_index + sizeof(uint));

            uint a = BitConverter.ToUInt32(key, key_index);
            uint b = BitConverter.ToUInt32(key, key_index + sizeof(uint));
            uint c = BitConverter.ToUInt32(key, key_index + 2 * sizeof(uint));
            uint d = BitConverter.ToUInt32(key, key_index + 3 * sizeof(uint));

            uint sum = Delta * 32;
            //Debug.Print($"sum init = {sum:x4}");
            for (int i = 0; i < 32; i++)
            {
                z -= ((y << 2) + c) ^ (y + sum) ^ ((y >> 7) + d);
                y -= ((z << 2) + a) ^ (z + sum) ^ ((z >> 7) + b);
                sum -= Delta;
            }
            //Debug.Print($"delta = {Delta:x4}\tsum = {sum:x4}");
            return GetBytes(y, z);
        }

        /// <summary>
        /// 解密一个8字节密文块，循环次数为n。密文和密钥均来源于字节数组，分别由各自的索引指定起始点。
        /// </summary>
        /// <param name="n">TEA解密器的循环次数</param>
        /// <param name="cipher">保存有密文块的字节数组</param>
        /// <param name="cipher_index">密文块的起始索引</param>
        /// <param name="key">保存有密钥块的字节数组</param>
        /// <param name="key_index">密钥块的起始索引</param>
        /// <returns>解密后的字节数组</returns>
        public byte[] Decryptor(int n, byte[] cipher, int cipher_index, byte[] key, int key_index)
        {
            uint y = BitConverter.ToUInt32(cipher, cipher_index);
            uint z = BitConverter.ToUInt32(cipher, cipher_index + sizeof(uint));

            uint a = BitConverter.ToUInt32(key, key_index);
            uint b = BitConverter.ToUInt32(key, key_index + sizeof(uint));
            uint c = BitConverter.ToUInt32(key, key_index + 2 * sizeof(uint));
            uint d = BitConverter.ToUInt32(key, key_index + 3 * sizeof(uint));

            uint sum = (uint)(Delta * n);
            //Debug.Print($"sum init = {sum:x4}");
            for (int i = 0; i < n; i++)
            {
                z -= ((y << 2) + c) ^ (y + sum) ^ ((y >> 7) + d);
                y -= ((z << 2) + a) ^ (z + sum) ^ ((z >> 7) + b);
                sum -= Delta;
            }
            //Debug.Print($"delta = {Delta:x4}\tsum = {sum:x4}");
            return GetBytes(y, z);
        }
        /// <summary>
        /// 加密一个8字节密文块，循环次数为n。密文和密钥均来源于字节数组，且起始于索引0。
        /// </summary>
        /// <param name="n">TEA加密器的循环次数</param>
        /// <param name="plain">保存有密文块的字节数组</param>
        /// <param name="key">保存有密钥块的字节数组</param>
        /// <returns>加密后的字节数组</returns>
        public byte[] Decryptor(int n, byte[] cipher, byte[] key) => Decryptor(n, cipher, 0, key, 0);

        private byte[] GetBytes(uint a, uint b)
        {
            List<byte> bin = new List<byte>(sizeof(uint) * 2);
            bin.AddRange(BitConverter.GetBytes(a));
            bin.AddRange(BitConverter.GetBytes(b));
            return bin.ToArray();
        }
    }
}
