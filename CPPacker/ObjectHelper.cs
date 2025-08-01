using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace CPPacker
{
    public class ObjectHelper
    {
        [System.Runtime.ExceptionServices.HandleProcessCorruptedStateExceptionsAttribute()] //加上此属性才能捕获到Win32内存异常
        /// <summary>
        /// Byte[]转换为struct
        /// </summary>
        /// <param name="bytes">要转换的数据</param>
        /// <param name="startIndex">要转换的数据起始位置的索引（从零开始）。</param>
        /// <param name="strcutType">结构体类型</param>
        public static T BytesToStruct<T>(byte[] bytes, int startIndex = 0)
        {
            Int32 size = Marshal.SizeOf(typeof(T));
            IntPtr buffer = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.Copy(bytes, startIndex, buffer, size);               // 将托管的8位无符号整数数组复制到非托管内存指针  
                return (T)Marshal.PtrToStructure(buffer, typeof(T));  // 将数据从非托管内存块封送到指定类型的托管对象  
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
    }
}
