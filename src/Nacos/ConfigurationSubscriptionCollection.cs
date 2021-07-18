using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Nacos
{
    /// <summary>
    /// nacos配置订阅集合
    /// </summary>
    public sealed class ConfigurationSubscriptionCollection : IDisposable
    {
        #region Private 字段

        private readonly Dictionary<INacosUniqueConfiguration, ConfigurationSubscribeState> _subscribeStates = new(new INacosUniqueConfigurationEqualityComparer());

        #endregion Private 字段

        #region Public 方法

        /// <summary>
        /// 添加订阅
        /// </summary>
        /// <param name="descriptor"></param>
        /// <param name="notifyCallback"></param>
        /// <returns>是否为新建</returns>
        public bool AddSubscribe(NacosConfigurationDescriptor descriptor, ConfigurationChangeNotifyCallback notifyCallback)
        {
            lock (_subscribeStates)
            {
                if (_subscribeStates.TryGetValue(descriptor, out var existSubscribeState))
                {
                    existSubscribeState.NotifyCallback += notifyCallback;
                    return false;
                }
                else
                {
                    _subscribeStates.Add(descriptor, new(descriptor, notifyCallback));
                    return true;
                }
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            lock (_subscribeStates)
            {
                _subscribeStates.Clear();
            }
        }

        /// <summary>
        /// 获取所有订阅信息
        /// </summary>
        /// <returns></returns>
        public ConfigurationSubscribeState[] GetAllSubscription()
        {
            lock (_subscribeStates)
            {
                return _subscribeStates.Values.ToArray();
            }
        }

        /// <summary>
        /// 移除订阅
        /// </summary>
        /// <param name="descriptor"></param>
        /// <param name="notifyCallback"></param>
        /// <returns>是否移除了所有订阅</returns>
        public bool RemoveSubscribe(NacosConfigurationDescriptor descriptor, ConfigurationChangeNotifyCallback notifyCallback)
        {
            lock (_subscribeStates)
            {
                if (_subscribeStates.TryGetValue(descriptor, out var subscribeState))
                {
                    var existNotifyCallback = subscribeState.NotifyCallback;
                    existNotifyCallback -= notifyCallback;

                    if (existNotifyCallback is null)
                    {
                        _subscribeStates.Remove(descriptor);
                        return true;
                    }
                    else
                    {
                        subscribeState.NotifyCallback = existNotifyCallback;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 尝试获取订阅状态
        /// </summary>
        /// <param name="nacosUniqueConfiguration"></param>
        /// <param name="subscribeState"></param>
        /// <returns></returns>
        public bool TryGetSubscribe(INacosUniqueConfiguration nacosUniqueConfiguration, [NotNullWhen(true)] out ConfigurationSubscribeState? subscribeState)
        {
            lock (_subscribeStates)
            {
                return _subscribeStates.TryGetValue(nacosUniqueConfiguration, out subscribeState);
            }
        }

        #endregion Public 方法
    }
}