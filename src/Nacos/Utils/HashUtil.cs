using System.Security.Cryptography;
using System.Text;

namespace Nacos.Utils
{
    /// <summary>
    /// Hash工具
    /// </summary>
    public static class HashUtil
    {
        #region Public 方法

        /// <summary>
        /// 获取Md5
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetMd5(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return GetMd5(Encoding.UTF8.GetBytes(value));
        }

        /// <summary>
        /// 获取Md5
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetMd5(byte[] value)
        {
            if (value is null
                || value.Length == 0)
            {
                return string.Empty;
            }

            using var md5 = MD5.Create();

            byte[] hash = md5.ComputeHash(value);

            var builder = new StringBuilder(32);

            foreach (byte item in hash)
            {
                builder.Append(item.ToString("x2"));
            }

            return builder.ToString();
        }

        #endregion Public 方法
    }
}