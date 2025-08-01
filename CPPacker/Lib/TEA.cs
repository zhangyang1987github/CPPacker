using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CPPacker.Lib
{
    public class TEA
    {
        private uint[] _Key;
        private uint _EncodeLoopCount;
        private uint _MaxSum = 0;

        /// <summary>
        /// 创建一个TEA算法加密解密对象
        /// </summary>
        /// <param name="key">加密解密使用的Key</param>
        /// <param name="EncodeLoopCount">加密解密轮询次数</param>
        public TEA(byte[] key, uint EncodeLoopCount = 32)
        {
            if (key.Length != 16)
                Array.Resize<byte>(ref key, 16);

            List<uint> keyl = new List<uint>();
            for (int i = 0; i < key.Length; i += 4)
                keyl.Add(BitConverter.ToUInt32(key, i));
            this._Key = keyl.ToArray();

            this._EncodeLoopCount = EncodeLoopCount;
            if (this._EncodeLoopCount < 1)
                this._EncodeLoopCount = 1;
        }

        /// <summary>
        /// 使用TEA算法加密数据
        /// </summary>
        /// <param name="input">输入数据</param>
        public byte[] Encode(byte[] input)
        {
            List<byte> output = new List<byte>();
            int input_length_black = (input.Length + 7) & ~7;
            if (input.Length < input_length_black)
                Array.Resize<byte>(ref input, input_length_black);

            for (int i = 0; i < input.Length; i += 8)
            {
                uint v0 = BitConverter.ToUInt32(input, i);
                uint v1 = BitConverter.ToUInt32(input, i + 4);
                output.AddRange(this.tea_encrypt(v0, v1));
            }
            if (output.Count < 1)
                return null;
            else
                return output.ToArray();
        }
        /// <summary>
        /// 使用TEA算法解密数据
        /// </summary>
        /// <param name="input">输入数据</param>
        /// <param name="output_length">输出数据实际长度</param>
        public byte[] Decode(byte[] input, int output_length)
        {
            List<byte> output = new List<byte>();
            int input_length_black = (input.Length + 7) & ~7;
            if (input.Length < input_length_black)
                Array.Resize<byte>(ref input, input_length_black);

            for (int i = 0; i < input.Length; i += 8)
            {
                uint v0 = BitConverter.ToUInt32(input, i);
                uint v1 = BitConverter.ToUInt32(input, i + 4);
                output.AddRange(this.tea_decrypt(v0, v1));
            }
            byte[] output_bytes = output.ToArray();
            if (output.Count < 1)
                return null;
            if (output_bytes.Length > output_length)
                Array.Resize<byte>(ref output_bytes, output_length);
            return output_bytes;
        }

        private byte[] tea_encrypt(uint v0, uint v1)
        {
            uint sum = 0;
            for (int i = 0; i < this._EncodeLoopCount; i++)
            {
                sum += 0x9e3779b9;

                v0 = v0 + (((v1 << 4) + this._Key[0]) ^ (v1 + sum) ^ ((v1 >> 5) + this._Key[1]));
                v1 = v1 + (((v0 << 4) + this._Key[2]) ^ (v0 + sum) ^ ((v0 >> 5) + this._Key[3]));
            }
            List<byte> output = new List<byte>();
            output.AddRange(BitConverter.GetBytes(v0));
            output.AddRange(BitConverter.GetBytes(v1));
            return output.ToArray();
        }

        private byte[] tea_decrypt(uint v0, uint v1)
        {
            if (this._MaxSum == 0)
            {
                for (int i = 0; i < this._EncodeLoopCount; i++)
                    this._MaxSum += 0x9e3779b9;
            }
            uint sum = this._MaxSum;
            for (int i = 0; i < this._EncodeLoopCount; i++)
            {
                v1 = v1 - (((v0 << 4) + this._Key[2]) ^ (v0 + sum) ^ ((v0 >> 5) + this._Key[3]));
                v0 = v0 - (((v1 << 4) + this._Key[0]) ^ (v1 + sum) ^ ((v1 >> 5) + this._Key[1]));

                sum -= 0x9e3779b9;
            }
            List<byte> output = new List<byte>();
            output.AddRange(BitConverter.GetBytes(v0));
            output.AddRange(BitConverter.GetBytes(v1));
            return output.ToArray();
        }
    }
}
