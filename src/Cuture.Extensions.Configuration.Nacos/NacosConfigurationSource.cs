using System;
using System.Threading.Tasks;

using Cuture.Extensions.Configuration.Nacos;

using Microsoft.Extensions.Configuration;

namespace Nacos
{
    /// <summary>
    /// Nacos配置源
    /// </summary>
    public class NacosConfigurationSource : IConfigurationSource
    {
        #region Private 字段

        private readonly NacosConfigurationProviderOptions _providerOptions;

        #endregion Private 字段

        #region Public 构造函数

        /// <summary>
        /// <inheritdoc cref="NacosConfigurationSource"/>
        /// </summary>
        /// <param name="providerOptions"></param>
        public NacosConfigurationSource(NacosConfigurationProviderOptions providerOptions)
        {
            _providerOptions = providerOptions ?? throw new ArgumentNullException(nameof(providerOptions));
        }

        #endregion Public 构造函数

        #region Public 方法

        /// <inheritdoc/>
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            var provider = new NacosConfigurationProvider(_providerOptions);

            provider.InitAsync().WaitWithoutContext();

            return provider;
        }

        /// <inheritdoc/>
        public override string ToString() => $"NacosConfigurationSource for {_providerOptions.Descriptor}";

        #endregion Public 方法
    }
}