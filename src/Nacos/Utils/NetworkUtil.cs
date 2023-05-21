using System;
using System.Collections;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Nacos.Utils;

/// <summary>
/// 网络工具
/// </summary>
public static class NetworkUtil
{
    /// <summary>
    /// 获取所有Ip地址，不包括本地回环
    /// </summary>
    /// <returns></returns>
    public static IPAddress[] GetAllAddress()
    {
        return NetworkInterface.GetAllNetworkInterfaces()
                               .Where(m => m.OperationalStatus == OperationalStatus.Up)
                               .Select(m => m.GetIPProperties())
                               .SelectMany(m => m.UnicastAddresses)
                               .Select(m => m.Address)
                               .ToArray();
    }

    /// <summary>
    /// 获取一个有价值的Ip地址
    /// </summary>
    /// <param name="subnetMask"></param>
    /// <returns></returns>
    public static IPAddress GetValuableIPAddress(string? subnetMask = null)
    {
        var addresses = GetAllAddress();
        if (addresses is null
            || !addresses.Any())
        {
            throw new NotSupportedException("当前没有有效的网络地址");
        }

        //获取属于指定子网的IP
        if (!string.IsNullOrWhiteSpace(subnetMask))
        {
            foreach (var address in addresses)
            {
                if (address.IsInSubnet(subnetMask))
                {
                    return address;
                }
            }
            throw new NotSupportedException($"没有找到指定子网 {subnetMask} 的IP.");
        }

        //TODO 尽可能获取一个与Nacos同网段的地址

        //尽可能获取一个公网地址
        return addresses.OrderByDescending(ScoringIPAddress).First();
    }

    /// <summary>
    /// 判断地址是否属于对应子网 <para/>
    /// see: https://stackoverflow.com/questions/1499269/how-to-check-if-an-ip-address-is-within-a-particular-subnet
    /// </summary>
    /// <param name="address"></param>
    /// <param name="subnetMask"></param>
    /// <returns></returns>
    public static bool IsInSubnet(this IPAddress address, string subnetMask)
    {
        var slashIdx = subnetMask.IndexOf("/");
        if (slashIdx == -1)
        { // We only handle netmasks in format "IP/PrefixLength".
            throw new NotSupportedException("Only SubNetMasks with a given prefix length are supported.");
        }

        // First parse the address of the netmask before the prefix length.
        var maskAddress = IPAddress.Parse(subnetMask.Substring(0, slashIdx));

        if (maskAddress.AddressFamily != address.AddressFamily)
        { // We got something like an IPV4-Address for an IPv6-Mask. This is not valid.
            return false;
        }

        // Now find out how long the prefix is.
        int maskLength = int.Parse(subnetMask.Substring(slashIdx + 1));

        if (maskAddress.AddressFamily == AddressFamily.InterNetwork)
        {
            // Convert the mask address to an unsigned integer.
            var maskAddressBits = BitConverter.ToUInt32(maskAddress.GetAddressBytes().Reverse().ToArray(), 0);

            // And convert the IpAddress to an unsigned integer.
            var ipAddressBits = BitConverter.ToUInt32(address.GetAddressBytes().Reverse().ToArray(), 0);

            // Get the mask/network address as unsigned integer.
            uint mask = uint.MaxValue << (32 - maskLength);

            // https://stackoverflow.com/a/1499284/3085985
            // Bitwise AND mask and MaskAddress, this should be the same as mask and IpAddress
            // as the end of the mask is 0000 which leads to both addresses to end with 0000
            // and to start with the prefix.
            return (maskAddressBits & mask) == (ipAddressBits & mask);
        }

        if (maskAddress.AddressFamily == AddressFamily.InterNetworkV6)
        {
            // Convert the mask address to a BitArray.
            var maskAddressBits = new BitArray(maskAddress.GetAddressBytes());

            // And convert the IpAddress to a BitArray.
            var ipAddressBits = new BitArray(address.GetAddressBytes());

            if (maskAddressBits.Length != ipAddressBits.Length)
            {
                throw new ArgumentException("Length of IP Address and Subnet Mask do not match.");
            }

            // Compare the prefix bits.
            for (int maskIndex = 0; maskIndex < maskLength; maskIndex++)
            {
                if (ipAddressBits[maskIndex] != maskAddressBits[maskIndex])
                {
                    return false;
                }
            }

            return true;
        }

        throw new NotSupportedException("Only InterNetworkV6 or InterNetwork address families are supported.");
    }

    #region ScoringIPAddress

    /// <summary>
    /// IPV4
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    private static int ScoringInterNetwork(IPAddress address)
    {
        // 公网地址分数
        const int NetworkAddressScore = 1000;

        // A类子网 分数
        const int PrivateAddressScoreTypeA = 900;

        // B类子网 分数
        const int PrivateAddressScoreTypeB = 905;

        // C类子网 分数
        const int PrivateAddressScoreTypeC = 910;

        // 没有正确DHCP
        const int NoneDHCPScore = -1;

        //本地环回
        if (IPAddress.IsLoopback(address))
        {
            return 0;
        }

        var values = address.ToString().Split('.');
        return values[0] switch
        {
            "10" => PrivateAddressScoreTypeA,    //A类保留子网
            "172" => Scoring172(values),    //可能为B类保留子网
            "192" => Scoring192(values),    //可能为C类保留子网
            "169" => Scoring169(values),    //可能为未正确获取IP
            _ => NetworkAddressScore,  //公网IP？
        };

        //分数优先级：C类子网 > B类子网 > A类子网

        static int Scoring172(string[] address)
        {
            var second = int.Parse(address[1]);

            //172.16.0.0—172.31.255.255 B类保留子网
            if (second >= 16 && second <= 31)
            {
                return PrivateAddressScoreTypeB;
            }

            return NetworkAddressScore;
        }

        static int Scoring169(string[] address)
        {
            //169.254.*.* 未正确获取IP
            return address[1] == "254" ? NoneDHCPScore : NetworkAddressScore;
        }

        static int Scoring192(string[] address)
        {
            //192.168.*.* C类保留子网
            return address[1] == "168" ? PrivateAddressScoreTypeC : NetworkAddressScore;
        }
    }

    /// <summary>
    /// IPV6
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    private static int ScoringInterNetworkV6(IPAddress address)
    {
        //HACK 先直接0分再说吧
        return 0;
    }

    /// <summary>
    /// 给Ip地址打分
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    private static int ScoringIPAddress(IPAddress address)
    {
        return address.AddressFamily switch
        {
            AddressFamily.InterNetwork => ScoringInterNetwork(address),
            AddressFamily.InterNetworkV6 => ScoringInterNetworkV6(address),
            _ => 0,
        };
    }

    #endregion ScoringIPAddress
}
