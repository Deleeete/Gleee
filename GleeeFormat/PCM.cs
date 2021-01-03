namespace Gleee.Formats
{
    /// <summary>
    /// PCM数据提取
    /// </summary>
    public static class PCM
    {
        /// <summary>
        /// 提取单声道数据
        /// </summary>
        /// <returns></returns>
        public static ushort[] ConvertMono16(byte[] data)
        {
            ushort[] sdata = new ushort[data.Length/2];
            for (int n = 0; n < sdata.Length; n++)
            {
                sdata[n] = (ushort)((data[2*n] << 8) + data[2 * n+1]);
            }
            return sdata;
        }
    }
}
