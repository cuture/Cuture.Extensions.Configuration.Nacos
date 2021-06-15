﻿// <Auto-Generated></Auto-Generated>
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Nacos.Messages
{
    /// <summary>
    /// Nacos请求的Headers
    /// </summary>
    public class NacosHeaders : IDictionary<string, string?>
    {
        #region Private 字段

        private readonly Dictionary<string, string?> _headers = new(32);

        #endregion Private 字段

        #region Public 属性

        /// <summary>
        /// 
        /// </summary>
        public string? AccessToken { get => InternalGetValue(Constants.Headers.ACCESS_TOKEN); set => InternalSetValue(Constants.Headers.ACCESS_TOKEN, value); }

        /// <summary>
        /// 
        /// </summary>
        public string? SpasAccessKey { get => InternalGetValue(Constants.Headers.SPAS_ACCESSKEY); set => InternalSetValue(Constants.Headers.SPAS_ACCESSKEY, value); }

        /// <summary>
        /// 
        /// </summary>
        public string? SpasSignature { get => InternalGetValue(Constants.Headers.SPAS_SIGNATURE); set => InternalSetValue(Constants.Headers.SPAS_SIGNATURE, value); }

        /// <summary>
        /// 
        /// </summary>
        public string? ClientAppName { get => InternalGetValue(Constants.Headers.CLIENT_APPNAME_HEADER); protected set => InternalSetValue(Constants.Headers.CLIENT_APPNAME_HEADER, value); }

        /// <summary>
        /// 
        /// </summary>
        public string? ClientRequestTimestamp { get => InternalGetValue(Constants.Headers.CLIENT_REQUEST_TS_HEADER); set => InternalSetValue(Constants.Headers.CLIENT_REQUEST_TS_HEADER, value); }

        /// <summary>
        /// 
        /// </summary>
        public string? ClientRequestToken { get => InternalGetValue(Constants.Headers.CLIENT_REQUEST_TOKEN_HEADER); set => InternalSetValue(Constants.Headers.CLIENT_REQUEST_TOKEN_HEADER, value); }

        /// <summary>
        /// 
        /// </summary>
        public string? ClientVersion { get => InternalGetValue(Constants.Headers.CLIENT_VERSION_HEADER); protected set => InternalSetValue(Constants.Headers.CLIENT_VERSION_HEADER, value); }

        /// <summary>
        /// 
        /// </summary>
        public string? EXConfigInfo { get => InternalGetValue(Constants.Headers.EX_CONFIG_INFO_HEADER); set => InternalSetValue(Constants.Headers.EX_CONFIG_INFO_HEADER, value); }

        /// <summary>
        /// 
        /// </summary>
        public string? AcceptCharset { get => InternalGetValue(Constants.Headers.ACCEPT_CHARSET); set => InternalSetValue(Constants.Headers.ACCEPT_CHARSET, value); }

        /// <summary>
        /// 
        /// </summary>
        public string? Timestamp { get => InternalGetValue(Constants.Headers.TIMESTAMP); set => InternalSetValue(Constants.Headers.TIMESTAMP, value); }

        #endregion Public 属性

        /// <inheritdoc cref="NacosHeaders"/>
        public NacosHeaders()
        {
            SetGenericHeaders();
        }

        /// <summary>
        /// <inheritdoc cref="NacosHeaders"/>
        /// </summary>
        /// <param name="setGenericHeaders">是否设置通用头部</param>
        public NacosHeaders(bool setGenericHeaders = false)
        {
            if (setGenericHeaders)
            {
                SetGenericHeaders();
            }
        }

        /// <summary>
        /// 设置部分通用头
        /// </summary>
        public void SetGenericHeaders()
        {
            ClientVersion = Constants.ClientVersion;
            ClientAppName = AppDomain.CurrentDomain.FriendlyName;
            EXConfigInfo = "true";
            AcceptCharset = "UTF-8";
        }

        #region Private 方法

        private string? InternalGetValue(string key)
        {
            if (_headers.TryGetValue(key, out var value))
            {
                return value;
            }
            return null;
        }

        private void InternalSetValue(string key, string? value)
        {
            if (value is null
                && _headers.ContainsKey(key))
            {
                _headers.Remove(key);
            }
            else
            {
                _headers[key] = value;
            }
        }

        #endregion Private 方法

        #region IDictionary

        /// <inheritdoc/>
        int ICollection<KeyValuePair<string, string?>>.Count => ((ICollection<KeyValuePair<string, string?>>)_headers).Count;
        /// <inheritdoc/>
        bool ICollection<KeyValuePair<string, string?>>.IsReadOnly => ((ICollection<KeyValuePair<string, string?>>)_headers).IsReadOnly;
        /// <inheritdoc/>
        ICollection<string> IDictionary<string, string?>.Keys => ((IDictionary<string, string?>)_headers).Keys;
        /// <inheritdoc/>
        ICollection<string?> IDictionary<string, string?>.Values => ((IDictionary<string, string?>)_headers).Values;

        /// <inheritdoc/>
        public string? this[string key] { get => ((IDictionary<string, string?>)_headers)[key]; set => ((IDictionary<string, string?>)_headers)[key] = value; }

        /// <inheritdoc/>
        public void Add(string key, string? value)
        {
            ((IDictionary<string, string?>)_headers).Add(key, value);
        }

        /// <inheritdoc/>
        public void Add(KeyValuePair<string, string?> item)
        {
            ((ICollection<KeyValuePair<string, string?>>)_headers).Add(item);
        }

        /// <inheritdoc/>
        public void Clear()
        {
            ((ICollection<KeyValuePair<string, string?>>)_headers).Clear();
        }

        /// <inheritdoc/>
        public bool Contains(KeyValuePair<string, string?> item)
        {
            return ((ICollection<KeyValuePair<string, string?>>)_headers).Contains(item);
        }

        /// <inheritdoc/>
        public bool ContainsKey(string key)
        {
            return ((IDictionary<string, string?>)_headers).ContainsKey(key);
        }

        /// <inheritdoc/>
        public void CopyTo(KeyValuePair<string, string?>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, string?>>)_headers).CopyTo(array, arrayIndex);
        }

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<string, string?>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, string?>>)_headers).GetEnumerator();
        }

        /// <inheritdoc/>
        public bool Remove(string key)
        {
            return ((IDictionary<string, string?>)_headers).Remove(key);
        }

        /// <inheritdoc/>
        public bool Remove(KeyValuePair<string, string?> item)
        {
            return ((ICollection<KeyValuePair<string, string?>>)_headers).Remove(item);
        }

        /// <inheritdoc/>
        public bool TryGetValue(string key, [MaybeNullWhen(false)] out string value)
        {
            return ((IDictionary<string, string?>)_headers).TryGetValue(key, out value);
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_headers).GetEnumerator();
        }

        #endregion IDictionary
    }
}