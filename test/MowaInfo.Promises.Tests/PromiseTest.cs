using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MowaInfo.Promises.Tests
{
    public class PromiseTest
    {
        [Fact]
        public async Task FailedWithTimeoutTest()
        {
            var promise = new Promise<int>();
            promise.SetTimeout(TimeSpan.FromSeconds(2), TimeOrigin.Current);
            Assert.True(promise.TrySetException(new AnyException()));

            await Assert.ThrowsAsync<AnyException>(async () =>
            {
                var _ = await promise.Task;
            });

            await Task.Delay(TimeSpan.FromSeconds(3));
        }

        [Fact]
        public async Task SuccessAfterTimeoutTest()
        {
            var promise = new Promise<int>();
            promise.SetTimeout(TimeSpan.FromSeconds(2), TimeOrigin.Current);

            await Task.Delay(TimeSpan.FromSeconds(3));

            Assert.False(promise.TrySetResult(1));

            await Assert.ThrowsAsync<TimeoutException>(async () =>
            {
                var _ = await promise.Task;
            });
        }

        [Fact]
        public async Task SuccessTest()
        {
            var promise = new Promise<int>();
            promise.SetResult(1);
            Assert.Equal(1, await promise.Task);
        }

        [Fact]
        public async Task SuccessWithTimeoutTest()
        {
            var promise = new Promise<int>();
            promise.SetTimeout(TimeSpan.FromSeconds(2), TimeOrigin.Current);

            Assert.True(promise.TrySetResult(1));
            Assert.Equal(1, promise.Task.Result);

            await Task.Delay(TimeSpan.FromSeconds(3));
        }

        [Fact]
        public async Task TimeoutTest()
        {
            var promise = new Promise<int>();
            promise.SetTimeout(TimeSpan.FromSeconds(2), TimeOrigin.Current);
            await Assert.ThrowsAsync<TimeoutException>(async () =>
            {
                var _ = await promise.Task;
            });
        }

        [Fact]
        public async Task ImmediateTest()
        {
            var promise = new Promise<int>();
            await Task.Delay(TimeSpan.FromSeconds(3));
            promise.SetTimeout(TimeSpan.FromSeconds(2), TimeOrigin.Begin);
            await Assert.ThrowsAsync<TimeoutException>(async () =>
            {
                var _ = await promise.Task;
            });
        }

        [Fact]
        public void InfiniteTest()
        {
            var promise = new Promise<int>();
            promise.SetTimeout(Timeout.InfiniteTimeSpan, TimeOrigin.Begin);
            Assert.Null(promise.Deadline);
        }

        [Fact]
        public async Task ChangeTimeoutTest()
        {
            var promise = new Promise<int>();
            promise.SetTimeout(TimeSpan.FromSeconds(3), TimeOrigin.Current);
            await Task.Delay(2);
            promise.SetTimeout(TimeSpan.FromSeconds(3), TimeOrigin.Current);
            await Task.Delay(2);
            Assert.True(promise.TrySetResult(1));
            Assert.Equal(1, await promise.Task);
        }
    }
}
