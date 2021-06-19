using System;

namespace Nacos
{
    /// <summary>
    /// 配置描述
    /// </summary>
    public class NacosConfigurationDescriptor : INacosUniqueConfiguration
    {
        #region Private 字段

        private string? _uniqueKey;

        #endregion Private 字段

        #region Public 属性

        /// <summary>
        /// 内容
        /// </summary>
        public string? Content { get; init; }

        /// <inheritdoc/>
        public string DataId { get; init; }

        /// <inheritdoc/>
        public string Group { get; init; }

        /// <summary>
        /// 配置的Hash（Md5）
        /// </summary>
        public string? Hash { get; init; }

        /// <inheritdoc/>
        public string Namespace { get; init; }

        #endregion Public 属性

        #region Public 构造函数

        /// <summary>
        /// <inheritdoc cref="NacosConfigurationDescriptor"/>
        /// </summary>
        /// <param name="namespace"></param>
        /// <param name="dataId"></param>
        /// <param name="group"></param>
        /// <param name="hash"></param>
        /// <param name="content"></param>
        public NacosConfigurationDescriptor(string @namespace, string dataId, string group = Constants.DEFAULT_GROUP, string? content = null, string? hash = null)
        {
            Namespace = string.IsNullOrWhiteSpace(@namespace) ? throw new ArgumentNullException(nameof(@namespace)) : @namespace;
            DataId = string.IsNullOrWhiteSpace(dataId) ? throw new ArgumentNullException(nameof(dataId)) : dataId;
            Hash = hash;
            Group = group ?? string.Empty;
            Content = content;
        }

        #endregion Public 构造函数

        #region Public 方法

        /// <inheritdoc/>
        public string GetUniqueKey() => _uniqueKey ??= $"{Namespace}+{Group}+{DataId}";

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Namespace: {Namespace} Group: {Group} DataId: {DataId}";
        }

        /// <summary>
        /// 以当前对象为基础，新建一个 <see cref="NacosConfigurationDescriptor"/> 并为其设置内容
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public NacosConfigurationDescriptor WithContent(string? content)
        {
            return new NacosConfigurationDescriptor(Namespace, DataId, Group, content, Hash);
        }

        /// <summary>
        /// 以当前对象为基础，新建一个 <see cref="NacosConfigurationDescriptor"/> 并为其设置内容
        /// </summary>
        /// <param name="content"></param>
        /// <param name="hash"></param>
        /// <returns></returns>
        public NacosConfigurationDescriptor WithContent(string? content, string? hash)
        {
            return new NacosConfigurationDescriptor(Namespace, DataId, Group, content, hash);
        }

        #endregion Public 方法
    }
}