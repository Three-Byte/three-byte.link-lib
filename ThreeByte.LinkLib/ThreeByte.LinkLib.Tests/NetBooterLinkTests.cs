using System;
using System.ComponentModel;
using ThreeByte.LinkLib.NetBooter;
using Xunit;

namespace ThreeByte.LinkLib.Tests
{
    public class NetBooterLinkTests
    {
        [Fact]
        public void Constructor_CreatesInstance()
        {
            var link = new NetBooterLink("192.168.1.100");
            Assert.NotNull(link);
        }

        [Fact]
        public void Indexer_UnknownPort_ReturnsFalse()
        {
            var link = new NetBooterLink("192.168.1.100");

            Assert.False(link[1]);
            Assert.False(link[2]);
            Assert.False(link[99]);
        }

        [Fact]
        public void Indexer_NegativePort_ReturnsFalse()
        {
            var link = new NetBooterLink("192.168.1.100");

            Assert.False(link[-1]);
        }

        [Fact]
        public void ImplementsINotifyPropertyChanged()
        {
            var link = new NetBooterLink("192.168.1.100");
            Assert.IsAssignableFrom<INotifyPropertyChanged>(link);
        }

        [Fact]
        public void ErrorOccurred_EventCanBeSubscribed()
        {
            var link = new NetBooterLink("192.168.1.100");
            Exception? receivedEx = null;
            link.ErrorOccurred += (s, ex) => { receivedEx = ex; };

            Assert.Null(receivedEx);
        }

        [Fact]
        public void PropertyChanged_EventCanBeSubscribed()
        {
            var link = new NetBooterLink("192.168.1.100");
            string? changedProp = null;
            link.PropertyChanged += (s, e) => { changedProp = e.PropertyName; };

            Assert.Null(changedProp);
        }
    }
}
