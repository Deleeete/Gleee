using Microsoft.DirectX.DirectSound;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Gleee.DXSound
{
    /// <summary>
    /// 音频捕获类
    /// </summary>
    public class DXSCapture
    {
        private Notify Notify { get; set; }
        private Capture Capture { get; set; }
        public CaptureBuffer Buffer { get; set; }
        private AutoResetEvent NotifyEvent { get; set; }
        private Thread EventListeningThread { get; set; }
        private int buffer_offset = 0;
        public Action ProcessNotify { get; set; } = () => { Debug.Print("请在DXSCapture.ProcessNotify中设置此处要进行的操作"); };
        /// <summary>
        /// 初始化DXSCapture对象
        /// </summary>
        public DXSCapture()
        {
            Initialize();
        }
        /// <summary>
        /// 在指定设备上初始化DXSCapture对象
        /// </summary>
        /// <param name="device_index">捕获设备的索引</param>
        public DXSCapture(int device_index)
        {
            CaptrueDeviceIndex = device_index;
            Initialize();
        }
        /// <summary>
        /// 初始化DXSCapture对象
        /// </summary>
        /// <param name="samples_per_slice">每一节的采样数</param>
        /// <param name="sample_rate">每秒采样次数</param>
        public DXSCapture(int samples_per_slice, int sample_rate)
        {
            SamplesPerSlice = samples_per_slice;
            SampleRate = sample_rate;
            Initialize();
        }
        /// <summary>
        /// 获取或设置每一节包含的采样数
        /// </summary>
        public int SamplesPerSlice = 4096;
        /// <summary>
        /// 采样位深，即每个采样点的比特数。默认值为16
        /// </summary>
        public short BitDepth { get; set; } = 16;
        /// <summary>
        /// 采样率，即每秒采样的个数。默认值为44100
        /// </summary>
        public int SampleRate { get; set; } = 44100;
        /// <summary>
        /// 缓冲区中采样点的总个数。默认为44100，即单声道下时长=1s
        /// </summary>
        public int BufferSampleCount { get; set; } = 11025;
        /// <summary>
        /// 获取缓冲区的总字节数
        /// </summary>
        public int TotalBufferBytes { get => BufferSampleCount * Channels * BitDepth / 8; }
        /// <summary>
        /// 声道数。默认值为1
        /// </summary>
        public short Channels { get; set; } = 1;
        /// <summary>
        /// 获取或设置捕获设备的索引，默认为1
        /// </summary>
        public int CaptrueDeviceIndex { get; set; } = 1;
        /// <summary>
        /// 当前捕获设备的GUID
        /// </summary>
        public Guid CaptureDeviceGuid { get => new CaptureDevicesCollection()[CaptrueDeviceIndex].DriverGuid; }
        /// <summary>
        /// 将捕获设备重设为默认
        /// </summary>
        /// <returns></returns>
        public void SetCaptureDeviceToDefault() => CaptrueDeviceIndex = 0;
        /// <summary>
        /// 用当前配置初始化DXSCapture对象
        /// </summary>
        public void Initialize()
        {
            buffer_offset = 0;
            //计算并设置缓冲区格式参数
            WaveFormat wave_format = new WaveFormat
            {
                BitsPerSample = BitDepth,
                SamplesPerSecond = SampleRate,
                Channels = Channels,
                BlockAlign = (short)(Channels * BitDepth / 8),
                AverageBytesPerSecond = Channels * BitDepth / 8 * SampleRate
            };
            wave_format.FormatTag = WaveFormatTag.Pcm;
            CaptureBufferDescription desc = new CaptureBufferDescription
            {
                Format = wave_format,
                BufferBytes = BufferSampleCount * wave_format.BlockAlign
            };
            //初始化Capture、缓冲区和通知对象。若指针指向的对象不为空，则先释放相应资源
            if (Capture != null) Capture.Dispose();
            Capture = new Capture(CaptureDeviceGuid);
            if (Buffer != null) Buffer.Dispose();
            Buffer = new CaptureBuffer(desc, Capture);
            if (Notify != null) Notify.Dispose();
            Notify = new Notify(Buffer);
            if (NotifyEvent != null) NotifyEvent.Close();
            NotifyEvent = new AutoResetEvent(false);
            //缓冲区被分成n节，则有n个同步点
            int block_count = BufferSampleCount / SamplesPerSlice;
            var notify_positions = new BufferPositionNotify[block_count];
            for (int n = 0; n < block_count; n++)
            {
                notify_positions[n] = new BufferPositionNotify();
                notify_positions[n].Offset = BitDepth / 8 * (n * SamplesPerSlice + SamplesPerSlice) - 1;
                notify_positions[n].EventNotifyHandle = NotifyEvent.SafeWaitHandle.DangerousGetHandle();
            }
            Notify.SetNotificationPositions(notify_positions);
            //终止潜在的废进程
            if (EventListeningThread != null && EventListeningThread.IsAlive) EventListeningThread.Abort();
            EventListeningThread = new Thread(new ThreadStart(WaitForNotify));
        }
        /// <summary>
        /// 等待Notify事件发生。事件发生后函数将执行ProcessNotify中的任务
        /// </summary>
        private void WaitForNotify()
        {
            while (true)
            {
                NotifyEvent.WaitOne(Timeout.Infinite, true);
                ProcessNotify.Invoke();
            }
        }
        /// <summary>
        /// 开始捕获数据，并返回数据缓冲区
        /// </summary>
        /// <returns>存放捕获数据的缓冲区</returns>
        public void StartCapture()
        {
            buffer_offset = 0;
            Buffer.Start(true); //开始循环写入缓冲区
            EventListeningThread.Start();
            Debug.Print("[开始]捕获已开始");
        }
        /// <summary>
        /// 停止捕获数据
        /// </summary>
        /// <param name="buffer"></param>
        public void StopCapture()
        {
            if (Buffer.Capturing)
            {
                buffer_offset = 0;
                Buffer.Stop();
                NotifyEvent.Set();
                EventListeningThread.Abort();
            }
            else Debug.Print("[停止]未在捕获，已忽略");
        }
        /// <summary>
        /// 从捕获缓冲区读取一段字节数组
        /// </summary>
        /// <param name="offset">读取的起始点</param>
        /// <param name="bytes_count">读取的长度</param>
        /// <returns>读取的字节数组</returns>
        public byte[] ReadBuffer(int offset, int bytes_count)
        {
            if (offset + bytes_count > TotalBufferBytes) throw new Exception("指示读取的内容超出了缓冲区范围");
            byte[] data = new byte[bytes_count];
            Array array = Buffer.Read(offset, typeof(byte), LockFlag.FromWriteCursor, new int[] { bytes_count });
            for (int i = offset; i < offset+bytes_count; i++)
            {
                data[i] = (byte)array.GetValue(i);
            }
            return data;
        }
        /// <summary>
        /// 返回当前缓冲区中的全部字节
        /// </summary>
        /// <param name="buffer">捕获缓冲区</param>
        /// <returns></returns>
        public byte[] ReadLatestBufferSlice()
        {
            int slice_bytes_count = SamplesPerSlice * BitDepth / 8;
            //Debug.Print("正在读取...");
            Buffer.GetCurrentPosition(out int capture_cursor, out int read_cursor);
            //Debug.Print($"偏移量为{buffer_offset}，读指针为{read_cursor}，捕获指针为{capture_cursor}");
            //if (length == 0) return new byte[0];
            //else if (length < 0) length += TotalBufferBytes;
            byte[] data = (byte[])Buffer.Read(buffer_offset, typeof(byte), LockFlag.FromWriteCursor, slice_bytes_count);
            //Debug.Print($"数据长度为{data.Length}");
            buffer_offset += slice_bytes_count;
            buffer_offset %= TotalBufferBytes;
            //Debug.Print($"新偏移量为{buffer_offset}");
            return data;
        }
        /// <summary>
        /// 读取当前缓冲区中的全部字节到流对象
        /// </summary>
        /// <param name="buffer">读取的缓冲区</param>
        /// <param name="stream">输出到的流</param>
        public void ReadBuffer(CaptureBuffer buffer, Stream stream)
        {
            buffer.Read(0, stream, 100, LockFlag.FromWriteCursor);
        }
        /// <summary>
        /// 释放全部资源
        /// </summary>
        public void Dispose()
        {
            Notify.Dispose();
            Buffer.Dispose();
            Capture.Dispose();
        }
        /// <summary>
        /// 获取捕获设备列表
        /// </summary>
        /// <returns>捕获设备列表</returns>
        public static string[] GetCaptureDevices()
        {
            var dvcs = new CaptureDevicesCollection();
            List<string> device_desc = new List<string>(dvcs.Count);
            foreach (DeviceInformation dv in dvcs)
            {
                device_desc.Add(dv.Description);
            }
            return device_desc.ToArray();
        }
    }

    public static class PCM
    {
        /// <summary>
        /// 提取单声道数据
        /// </summary>
        /// <returns></returns>
        public static ushort[] ConvertMono16(byte[] data)
        {
            ushort[] sdata = new ushort[data.Length / 2];
            for (int n = 0; n < sdata.Length; n++)
            {
                sdata[n] = (ushort)((data[2 * n] << 8) + data[2 * n + 1]);
            }
            return sdata;
        }
    }
}
