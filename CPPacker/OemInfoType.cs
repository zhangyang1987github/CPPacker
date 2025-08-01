using CPPacker.Lib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace CPPacker
{
    public enum OemInfoType : uint
    {
        /// <summary>
        /// 公司国际化名称 (如: Cloudpoint)
        /// </summary>
        CompanyName,
        /// <summary>
        /// 公司本地名称 (如: 云点科技)
        /// </summary>
        CompanyNameLocal,
        /// <summary>
        /// 软件套件系统名称 (如: vMatrix)
        /// </summary>
        SoftSuiteDisplayName,
        /// <summary>
        /// 在路径中使用的软件套件系统名称 (如: vMatrix)
        /// </summary>
        SoftSuiteNameByPath,
        /// <summary>
        /// 公司本地语言代码 (如: zh-cn)
        /// </summary>
        LocalLanguageCode,
        /// <summary>
        /// 管理程序标志名 (如: CloudpointServerManager)
        /// </summary>
        SoftServerManagerName,
        /// <summary>
        /// 管理程序友好名称 (如: Cloudpoint Server Manager)
        /// </summary>
        SoftServerManagerDisplayName,
        /// <summary>
        /// 系统管理程序文件名 (如: CpManager.exe)
        /// </summary>
        SoftServerManagerFileName,
        /// <summary>
        /// 驻守服务标识名 (如: CloudpointDaemonService)
        /// </summary>
        SoftDaemonServiceName,
        /// <summary>
        /// 驻守服务友好名称 (如: Cloudpoint Daemon Service)
        /// </summary>
        SoftDaemonServiceDisplayName,
        /// <summary>
        /// 驻守服务描述
        /// </summary>
        SoftDaemonServiceDescription,
        /// <summary>
        /// 驻守服务文件名 (如: CpDaemon.exe)
        /// </summary>
        SoftDaemonServiceFileName,
        /// <summary>
        /// 驱动后台服务标识名 (如: CloudpointHostService)
        /// </summary>
        SoftHostServiceName,
        /// <summary>
        /// 驱动后台服务友好名称 (如: Cloudpoint Daemon Service)
        /// </summary>
        SoftHostServiceDisplayName,
        /// <summary>
        /// 驱动后台服务描述
        /// </summary>
        SoftHostServiceDescription,
        /// <summary>
        /// 驱动后台服务文件名 (如: CpAccel.exe)
        /// </summary>
        SoftHostServiceFileName,
        /// <summary>
        /// 驻守服务防火墙规则名称 (如: Cloudpoint Daemon Service)
        /// </summary>
        WindowsFirewallRuleName_DaemonService,
        /// <summary>
        /// 驱动后台服务防火墙规则名称 (如: Cloudpoint Host Service)
        /// </summary>
        WindowsFirewallRuleName_HostService,
        /// <summary>
        /// Windows远程桌面防火墙规则名称 (如: Windows Remote Desktop Protocol)
        /// </summary>
        WindowsFirewallRuleName_RdpService,
        /// <summary>
        /// 用户部署程序文件名 (如: CpDeploy.exe)
        /// </summary>
        SoftUserLogonDeployToolsFileName,
        /// <summary>
        /// 安装部署程序文件名 (如: InstallDeployTools.exe)
        /// </summary>
        SoftInstallDeployToolsFileName,
        /// <summary>
        /// 诊断程序文件名 (如: DiagnosticTools.exe)
        /// </summary>
        SoftDiagnosticToolsFileName,
        /// <summary>
        /// 用来远程连接服务器的本地用户组名称 (如: CloudpointServerRemoteUsers)
        /// </summary>
        WindowsRemoteUserGroupName,
        /// <summary>
        /// 用来远程连接服务器的本地用户组描述 (如: Cloudpoint Server Remote Users)
        /// </summary>
        WindowsRemoteUserGroupComment,
        /// <summary>
        /// 用户私有空间根目录文件夹固定名称 (如: CloudpointUserStorage)
        /// </summary>
        UserStorageRootFolderName,
        /// <summary>
        /// 用户私有空间根目录文件夹注释 (如: Cloudpoint User Private Storage Space)
        /// </summary>
        UserStorageRootFolderNote,
        /// <summary>
        /// 用来显示的公司全名
        /// </summary>
        CompanyDisplayFullName,
        /// <summary>
        /// “关于”产品的显示文本
        /// </summary>
        AboutProductText,
        /// <summary>
        /// 管理程序启动LOGO
        /// </summary>
        SoftServerManagerStartLogo,
        /// <summary>
        /// 管理程序背景LOGO
        /// </summary>
        SoftServerManagerBgLogo,
        /// <summary>
        /// 终端型号对应的显示名称表
        /// </summary>
        TerminalModelDisplayNameMap,
        /// <summary>
        /// 扩展插件页面地址
        /// </summary>
        AddOnsWebUrl,
        ////////////////////////////////////--vdi---///////////////////////////////////////////////
        /// <summary>
        /// virspire软件套件名称 ，如virspire
        /// </summary>
        vxSoftSuiteDisplayName,
        /// <summary>
        /// virspire服务名称，如VxDaemonService 
        /// </summary>
        vxDaemonServiceName,

        /// <summary>
        /// virspire服务描述，如 virspire daemon service
        /// </summary>
        vxDaemonServiceDescription,
        /// <summary>
        /// virspire托盘名称
        /// </summary>
        vxTrayName,
        //////////////////////////////////////////////////////////////////////////////
        ///用于替换vmatrix程序桌面快捷方式的显示名称
        ShortcutDisplayName,


        /// <summary>
        /// 屏蔽changelog按钮
        /// </summary>
        ChangelogButtonHidden,


    }

    public struct OemFileHeader
    {
        /// <summary>
        /// OEM信息类型
        /// </summary>
        public OemInfoType type;
        /// <summary>
        /// 信息在文件里的偏移
        /// </summary>
        public uint pos;
        /// <summary>
        /// 信息解密后的实际大小
        /// </summary>
        public uint length;
    }

    public class OemInfoHelper
    {
        private static Mutex ThreadLocker = new Mutex(true);
        private static string OemInfoFilePath = null;
        private static string OemInfoRsaPublicKey = null;
        private static FileStream OemInfoFS = null;
        private static TEA TeaDecoder = null;
        private static long OemInfoFileDataStartOffset = 0;
        private static Dictionary<OemInfoType, OemFileHeader> OemInfoHeaderMap = new Dictionary<OemInfoType, OemFileHeader>();
        private static Dictionary<OemInfoType, object> OemInfoDataMap = new Dictionary<OemInfoType, object>();

        /// <summary>
        /// 初始化OEM信息
        /// </summary>
        /// <param name="SetOemInfoFilePath">设定OEM信息文件的路径，如果为空值则使用默认路径</param>
        public static void Initialize(string SetOemInfoFilePath, string key)
        {
            lock (ThreadLocker)
            {
                OemInfoFilePath = SetOemInfoFilePath;
                OemInfoRsaPublicKey = key;

                try
                {
                    OemInfoFS = new FileStream(OemInfoFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    byte[] ibytes = new byte[4];
                    if (OemInfoFS.Read(ibytes, 0, ibytes.Length) != ibytes.Length)
                        throw new Exception();
                    if (Encoding.ASCII.GetString(ibytes) != "CPOI")
                        throw new Exception();
                    if (OemInfoFS.Read(ibytes, 0, ibytes.Length) != ibytes.Length)
                        throw new Exception();
                    byte[] KeyHeaderBytes = new byte[BitConverter.ToInt32(ibytes, 0)];

                    if (OemInfoFS.Read(KeyHeaderBytes, 0, KeyHeaderBytes.Length) != KeyHeaderBytes.Length)
                        throw new Exception();
                    OpenSSL.Core.BIO bio = new OpenSSL.Core.BIO(OemInfoRsaPublicKey);
                    OpenSSL.Crypto.RSA rsa = OpenSSL.Crypto.RSA.FromPublicKey(bio);
                    KeyHeaderBytes = rsa.PublicDecrypt(KeyHeaderBytes, OpenSSL.Crypto.RSA.Padding.PKCS1);
                    if (KeyHeaderBytes.Length != 20)
                        throw new Exception();
                    byte[] TeaKey = new byte[16];
                    Array.Copy(KeyHeaderBytes, TeaKey, TeaKey.Length);
                    TeaDecoder = new TEA(TeaKey, 16);
                    int HeaderLength = BitConverter.ToInt32(KeyHeaderBytes, 16);
                    byte[] HeaderBytes = new byte[HeaderLength];
                    int header_length_black = (HeaderBytes.Length + 7) & ~7;
                    if (HeaderBytes.Length < header_length_black)
                        Array.Resize<byte>(ref HeaderBytes, header_length_black);
                    if (OemInfoFS.Read(HeaderBytes, 0, HeaderBytes.Length) != HeaderBytes.Length)
                        throw new Exception();
                    OemInfoFileDataStartOffset = OemInfoFS.Position;
                    HeaderBytes = TeaDecoder.Decode(HeaderBytes, HeaderLength);
                    int headerMixLength = Marshal.SizeOf(typeof(OemFileHeader));
                    for (int index = 0; index < HeaderBytes.Length - headerMixLength + 1; index += headerMixLength)
                    {
                        OemFileHeader h = ObjectHelper.BytesToStruct<OemFileHeader>(HeaderBytes, index);
                        if (!OemInfoHeaderMap.ContainsKey(h.type))
                            OemInfoHeaderMap.Add(h.type, h);
                    }
                }
#if DEBUG
                finally { }
#else
                    catch
                    {
                        throw new Exception(string.Format("Load {0} Fail!", Path.GetFileName(OemInfoFilePath)));
                    }
#endif

            }
        }


        /// <summary>
        /// 获取OEM信息字节数据
        /// </summary>
        /// <param name="type">OEM信息类型</param>
        /// <param name="NeedCache">是否需要缓存</param>
        public static T GetOemInfo<T>(OemInfoType type, bool NeedCache = true)
        {
            try
            {
                lock (ThreadLocker)
                {
                    if (OemInfoFS == null || OemInfoHeaderMap.Count < 1)
                        throw new Exception();

                    Type OutputType = typeof(T);
                    object d = null;
                    if (OemInfoDataMap.ContainsKey(type))
                    {
                        d = OemInfoDataMap[type];
                        if (d != null && d.GetType() == typeof(T))
                            return (T)d;
                    }
                    byte[] bytes = GetOemInfoRow(type);
                    if (OutputType == typeof(string))
                        d = Encoding.Unicode.GetString(bytes).TrimEnd('\0');
                    else if (OutputType == typeof(Bitmap))
                    {
                        using (MemoryStream ms = new MemoryStream(bytes))
                            d = new Bitmap(ms);
                    }
                    else
                        throw new Exception();
                    if (NeedCache)
                    {
                        if (OemInfoDataMap.ContainsKey(type))
                            OemInfoDataMap[type] = d;
                        else
                            OemInfoDataMap.Add(type, d);
                    }
                    return (T)d;
                }
            }
            catch
            {
                throw new Exception(string.Format("Load {0} Fail!", Path.GetFileName(OemInfoFilePath)));
            }
        }


        /// <summary>
        /// 获取原始的OEM信息字节数据
        /// </summary>
        /// <param name="type">OEM信息类型</param>
        private static byte[] GetOemInfoRow(OemInfoType type)
        {
            byte[] data = null;
            lock (ThreadLocker)
            {
                if (OemInfoHeaderMap.ContainsKey(type))
                {
                    OemFileHeader h = OemInfoHeaderMap[type];
                    data = new byte[h.length];
                    int data_length_black = (data.Length + 7) & ~7;
                    if (data.Length < data_length_black)
                        Array.Resize<byte>(ref data, data_length_black);
                    OemInfoFS.Seek(OemInfoFileDataStartOffset + h.pos, SeekOrigin.Begin);
                    OemInfoFS.Read(data, 0, data.Length);
                    data = TeaDecoder.Decode(data, (int)h.length);
                }
                else
                    data = new byte[0];
            }
            return data;
        }
    }
}
