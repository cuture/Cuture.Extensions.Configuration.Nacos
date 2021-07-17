using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nacos.Tests
{
    [TestClass]
    public class ConfigurationSubscriptionCollectionTest
    {
        #region Private 字段

        private ConfigurationSubscriptionCollection _subscriptionCollection;

        #endregion Private 字段

        #region Public 方法

        [TestMethod]
        public void AddAndRemoveManyTimes()
        {
            const int Count = 10;

            IsEmptyCollection();

            ConfigurationChangeNotifyCallback notifyCallback = (_, _) => Task.CompletedTask;

            for (int i = 0; i < Count; i++)
            {
                _subscriptionCollection.AddSubscribe(CreateDescriptor("test"), notifyCallback);
            }

            var result = _subscriptionCollection.TryGetSubscribe(CreateDescriptor("test"), out var state);

            Assert.IsTrue(result);
            Assert.IsNotNull(state);
            Assert.AreNotEqual(notifyCallback, state.NotifyCallback);

            for (int i = 0; i < Count - 1; i++)
            {
                Assert.IsFalse(_subscriptionCollection.RemoveSubscribe(CreateDescriptor("test"), notifyCallback));
            }

            Assert.IsTrue(_subscriptionCollection.RemoveSubscribe(CreateDescriptor("test"), notifyCallback));

            IsEmptyCollection();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _subscriptionCollection.Dispose();
        }

        [TestMethod]
        public void GetWithOutSubscription()
        {
            IsEmptyCollection();

            var result = _subscriptionCollection.TryGetSubscribe(CreateDescriptor("test"), out var state);

            Assert.IsFalse(result);
            Assert.IsNull(state);

            IsEmptyCollection();
        }

        [TestMethod]
        public void GetWithSubscription()
        {
            IsEmptyCollection();

            ConfigurationChangeNotifyCallback notifyCallback = (_, _) => Task.CompletedTask;

            _subscriptionCollection.AddSubscribe(CreateDescriptor("test"), notifyCallback);

            var result = _subscriptionCollection.TryGetSubscribe(CreateDescriptor("test"), out var state);

            Assert.IsTrue(result);
            Assert.IsNotNull(state);
            Assert.AreEqual(notifyCallback, state.NotifyCallback);
        }

        [TestInitialize]
        public void Init()
        {
            _subscriptionCollection = new ConfigurationSubscriptionCollection();
        }

        [TestMethod]
        public void RemoveWithDifferentDescriptor()
        {
            IsEmptyCollection();

            ConfigurationChangeNotifyCallback notifyCallback = (_, _) => Task.CompletedTask;

            _subscriptionCollection.AddSubscribe(CreateDescriptor("test"), notifyCallback);

            Assert.IsFalse(_subscriptionCollection.RemoveSubscribe(CreateDescriptor("test1"), notifyCallback));
            Assert.IsTrue(_subscriptionCollection.RemoveSubscribe(CreateDescriptor("test"), notifyCallback));

            IsEmptyCollection();
        }

        [TestMethod]
        public void RemoveWithDifferentNotifyCallback()
        {
            IsEmptyCollection();

            ConfigurationChangeNotifyCallback notifyCallback1 = (_, _) => Task.CompletedTask;
            ConfigurationChangeNotifyCallback notifyCallback2 = (_, _) => Task.CompletedTask;

            _subscriptionCollection.AddSubscribe(CreateDescriptor("test"), notifyCallback1);

            Assert.IsFalse(_subscriptionCollection.RemoveSubscribe(CreateDescriptor("test"), notifyCallback2));
            Assert.IsTrue(_subscriptionCollection.RemoveSubscribe(CreateDescriptor("test"), notifyCallback1));

            IsEmptyCollection();
        }

        [TestMethod]
        public void RemoveWithOutSubscription()
        {
            IsEmptyCollection();

            Assert.IsFalse(_subscriptionCollection.RemoveSubscribe(CreateDescriptor("test"), (_, _) => Task.CompletedTask));

            IsEmptyCollection();
        }

        [TestMethod]
        public void RemoveWithSubscription()
        {
            IsEmptyCollection();

            ConfigurationChangeNotifyCallback notifyCallback = (_, _) => Task.CompletedTask;

            _subscriptionCollection.AddSubscribe(CreateDescriptor("test"), notifyCallback);

            Assert.IsTrue(_subscriptionCollection.RemoveSubscribe(CreateDescriptor("test"), notifyCallback));
            Assert.IsFalse(_subscriptionCollection.RemoveSubscribe(CreateDescriptor("test"), notifyCallback));

            IsEmptyCollection();
        }

        #endregion Public 方法

        #region Private 方法

        private static NacosConfigurationDescriptor CreateDescriptor(string prefix)
        {
            return new NacosConfigurationDescriptor($"{prefix}_namespace", $"{prefix}_dataId");
        }

        private void IsEmptyCollection()
        {
            Assert.AreEqual(0, _subscriptionCollection.GetAllSubscription().Length);
        }

        #endregion Private 方法
    }
}