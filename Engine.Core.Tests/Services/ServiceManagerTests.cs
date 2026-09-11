using System.Collections.Concurrent;

using Engine.Core.Platform;
using Engine.Core.Services;
using Engine.Core.Services.Attributes;

using FluentAssertions;

using static Engine.Core.Tests.Services.ServiceManagerTests;

namespace Engine.Core.Tests.Services
{
    public sealed class ServiceManagerTests
    {
        [Fact]
        public async Task InitializeAsync_Initializes_And_Reports_Properly()
        {
            TestState.Reset();

            ServiceRegistrar registrar = new();

            registrar.RegisterService(
                typeof(IFirstService),
                typeof(FirstService),
                priority: 0,
                loadingWeight: 10,
                failureBehavior: ServiceFailureBehavior.StopInitialization);

            registrar.RegisterService(
                typeof(ISecondService),
                typeof(SecondService),
                priority: 0,
                loadingWeight: 20,
                failureBehavior: ServiceFailureBehavior.StopInitialization);

            registrar.RegisterService(
                typeof(IThirdService),
                typeof(ThirdService),
                priority: 0,
                loadingWeight: 70,
                failureBehavior: ServiceFailureBehavior.StopInitialization);

            ServiceManager manager = new(registrar, CreateWindowedEnvironment());
            CapturingProgress progress = new();

            Task initialization = manager.InitializeAsync(progress, CancellationToken.None);

            // Both independent Tier 0 services should begin.
            Task bothStarted = Task.WhenAll(TestState.FirstStarted.Task, TestState.SecondStarted.Task);
            Task completed = await Task.WhenAny(bothStarted, initialization, Task.Delay(TimeSpan.FromSeconds(1)));

            if (completed == initialization)
            {
                // Rethrows the real ServiceManager exception with its stack trace.
                await initialization;
            }

            if (completed != bothStarted)
            {
                throw new TimeoutException($"Services did not start. Manager task status: {initialization.Status}");
            }

            await bothStarted;

            // The Tier 1 service must not start until Tier 0 completes.
            TestState.ThirdStarted.Task.IsCompleted.Should().BeFalse();

            TestState.ReleaseFirst.TrySetResult();
            TestState.ReleaseSecond.TrySetResult();

            await initialization;

            TestState.ThirdStarted.Task.IsCompleted.Should().BeTrue();

            manager.Get<IFirstService>().Should().NotBeNull();
            manager.Get<ISecondService>().Should().NotBeNull();
            manager.Get<IThirdService>().Should().NotBeNull();

            progress.Updates
                .Select(update => update.OverallProgress)
                .Should()
                .Contain(value => MathF.Abs(value - 0.05f) < 0.001f);

            progress.Updates
                .Select(update => update.OverallProgress)
                .Should()
                .Contain(value => MathF.Abs(value - 0.15f) < 0.001f);

            progress.Updates.Last().OverallProgress.Should().Be(1f);
        }

        [Fact]
        public async Task InitializeAsync_Selects_The_Highest_Priority_Service()
        {
            ServiceRegistrar registrar = new();

            registrar.RegisterService(
                typeof(IPriorityService),
                typeof(LowPriorityService),
                priority: 10,
                loadingWeight: 1,
                failureBehavior: ServiceFailureBehavior.StopInitialization);

            registrar.RegisterService(
                typeof(IPriorityService),
                typeof(HighPriorityService),
                priority: 20,
                loadingWeight: 1,
                failureBehavior: ServiceFailureBehavior.StopInitialization);

            ServiceManager manager = new(registrar, CreateWindowedEnvironment());

            await manager.InitializeAsync(null, CancellationToken.None);

            manager.Get<IPriorityService>()
                .Should()
                .BeOfType<HighPriorityService>();
        }

        [Fact]
        public async Task InitializeAsync_Rejects_Highest_Priority_Tie()
        {
            TieServiceA.InstancesCreated = 0;
            TieServiceB.InstancesCreated = 0;

            ServiceRegistrar registrar = new();

            registrar.RegisterService(
                typeof(ITieService),
                typeof(TieServiceA),
                priority: 20,
                loadingWeight: 1,
                failureBehavior: ServiceFailureBehavior.StopInitialization);

            registrar.RegisterService(
                typeof(ITieService),
                typeof(TieServiceB),
                priority: 20,
                loadingWeight: 1,
                failureBehavior: ServiceFailureBehavior.StopInitialization);

            ServiceManager manager = new(registrar, CreateWindowedEnvironment());

            Func<Task> act = () => manager.InitializeAsync(null, CancellationToken.None);

            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("*Multiple compatible implementations*");

            TieServiceA.InstancesCreated.Should().Be(0);
            TieServiceB.InstancesCreated.Should().Be(0);
        }

        [Fact]
        public async Task InitializeAsync_Ignores_An_Incompatible_Platform_Service()
        {
            ServiceRegistrar registrar = new();

            registrar.RegisterService(
                typeof(IPlatformService),
                typeof(LinuxOnlyService),
                priority: 100,
                loadingWeight: 1,
                failureBehavior: ServiceFailureBehavior.StopInitialization);

            registrar.RegisterService(
                typeof(IPlatformService),
                typeof(CrossPlatformService),
                priority: 10,
                loadingWeight: 1,
                failureBehavior: ServiceFailureBehavior.StopInitialization);

            ServiceManager manager = new(registrar, CreateWindowedEnvironment());

            await manager.InitializeAsync(null, CancellationToken.None);

            manager.Get<IPlatformService>()
                .Should()
                .BeOfType<CrossPlatformService>();
        }

        [Fact]
        public async Task InitializeAsync_Rejects_Missing_Dependencies()
        {
            MissingDependencyService.InstancesCreated = 0;

            ServiceRegistrar registrar = new();

            registrar.RegisterService(
                typeof(IMissingDependencyService),
                typeof(MissingDependencyService),
                priority: 0,
                loadingWeight: 1,
                failureBehavior: ServiceFailureBehavior.StopInitialization);

            ServiceManager manager = new(registrar, CreateWindowedEnvironment());

            Func<Task> act = () => manager.InitializeAsync(null, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>();

            MissingDependencyService.InstancesCreated.Should().Be(0);
        }


        [Fact]
        public async Task InitializeAsync_Rejects_Dependency_Cycles()
        {
            CycleServiceA.InstancesCreated = 0;
            CycleServiceB.InstancesCreated = 0;

            ServiceRegistrar registrar = new();

            registrar.RegisterService(
                typeof(ICycleServiceA),
                typeof(CycleServiceA),
                priority: 0,
                loadingWeight: 1,
                failureBehavior: ServiceFailureBehavior.StopInitialization);

            registrar.RegisterService(
                typeof(ICycleServiceB),
                typeof(CycleServiceB),
                priority: 0,
                loadingWeight: 1,
                failureBehavior: ServiceFailureBehavior.StopInitialization);

            ServiceManager manager = new(registrar, CreateWindowedEnvironment());

            Func<Task> act = () => manager.InitializeAsync(null, CancellationToken.None);

            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("*Circular dependency*");

            CycleServiceA.InstancesCreated.Should().Be(0);
            CycleServiceB.InstancesCreated.Should().Be(0);
        }

        [Fact]
        public async Task InitializeAsync_shuts_down_completed_services_after_a_later_failure()
        {
            RollbackState.Events.Clear();

            ServiceRegistrar registrar = new();

            registrar.RegisterService(
                typeof(IRollbackFirstService),
                typeof(RollbackFirstService),
                priority: 0,
                loadingWeight: 1,
                failureBehavior: ServiceFailureBehavior.StopInitialization);

            registrar.RegisterService(
                typeof(IRollbackFailingService),
                typeof(RollbackFailingService),
                priority: 0,
                loadingWeight: 1,
                failureBehavior: ServiceFailureBehavior.StopInitialization);

            ServiceManager manager = new(registrar, CreateWindowedEnvironment());

            Func<Task> act = () => manager.InitializeAsync(null, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>();

            RollbackState.Events.Should().Equal(
                "First.Initialize",
                "Failing.Initialize",
                "First.Shutdown");

            manager.TryGet<IRollbackFirstService>(out _).Should().BeFalse();
        }

        [Theory]
        [InlineData(ServiceFailureBehavior.ContinueInitialization)]
        [InlineData(ServiceFailureBehavior.TryNextService)]
        [InlineData(ServiceFailureBehavior.Retry)]
        public async Task InitializeAsync_Rejects_Unsupported_Failure_Behavior(ServiceFailureBehavior failureBehavior)
        {
            UnsupportedBehaviorService.InstancesCreated = 0;

            ServiceRegistrar registrar = new();

            registrar.RegisterService(
                typeof(IUnsupportedBehaviorService),
                typeof(UnsupportedBehaviorService),
                priority: 0,
                loadingWeight: 1,
                failureBehavior: failureBehavior);

            ServiceManager manager = new(registrar, CreateWindowedEnvironment());

            Func<Task> act = () => manager.InitializeAsync(
                progress: null,
                CancellationToken.None);

            await act.Should().ThrowAsync<NotSupportedException>();

            UnsupportedBehaviorService.InstancesCreated.Should().Be(0);
        }

        [Fact]
        public async Task InitializeAsync_Sets_State_To_Initialized()
        {
            ServiceRegistrar registrar = new();
            registrar.RegisterService(
                typeof(IPriorityService),
                typeof(LowPriorityService),
                priority: 10,
                loadingWeight: 1,
                failureBehavior: ServiceFailureBehavior.StopInitialization);

            ServiceManager manager = new(registrar, CreateWindowedEnvironment());
            await manager.InitializeAsync(null, CancellationToken.None);
            manager.State.Should().Be(LifecycleState.Initialized);
        }

        [Fact]
        public async Task InitializeAsync_Called_Twice_Throws()
        {
            ServiceRegistrar registrar = new();
            registrar.RegisterService(
                typeof(IPriorityService),
                typeof(LowPriorityService),
                priority: 10,
                loadingWeight: 1,
                failureBehavior: ServiceFailureBehavior.StopInitialization);

            ServiceManager manager = new(registrar, CreateWindowedEnvironment());
            await manager.InitializeAsync(null, CancellationToken.None);
            Func<Task> act = () => manager.InitializeAsync(null, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*manager state is Initialized*");
        }

        [Fact]
        public async Task Shutdown_Before_Initialization_Is_Safe()
        {
            ServiceRegistrar registrar = new();
            registrar.RegisterService(
                typeof(IPriorityService),
                typeof(LowPriorityService),
                priority: 10,
                loadingWeight: 1,
                failureBehavior: ServiceFailureBehavior.StopInitialization);

            ServiceManager manager = new(registrar, CreateWindowedEnvironment());
            Func<Action> action = () => manager.Shutdown;
            action.Should().NotThrow();
        }

        [Fact]
        public async Task Shutdown_Twice_Only_Shuts_Each_Service_Down_Once()
        {
            ShutdownTrackingService.ShutdownCalls = 0;

            ServiceRegistrar registrar = new();

            registrar.RegisterService(
                typeof(IShutdownTrackingService),
                typeof(ShutdownTrackingService),
                priority: 0,
                loadingWeight: 1,
                failureBehavior: ServiceFailureBehavior.StopInitialization);

            ServiceManager manager = new(registrar, CreateWindowedEnvironment());

            await manager.InitializeAsync(null, CancellationToken.None);

            manager.Shutdown();
            manager.Shutdown();

            ShutdownTrackingService.ShutdownCalls.Should().Be(1);
            manager.State.Should().Be(LifecycleState.Shutdown);
        }

        [Fact]
        public async Task Failed_Initialization_Sets_State_To_Failed()
        {
            ServiceRegistrar registrar = new();

            registrar.RegisterService(
                typeof(IFailingService),
                typeof(FailingService),
                priority: 0,
                loadingWeight: 1,
                failureBehavior: ServiceFailureBehavior.StopInitialization);

            ServiceManager manager = new(registrar, CreateWindowedEnvironment());

            Func<Task> act = () => manager.InitializeAsync(null, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>();

            manager.State.Should().Be(LifecycleState.Failed);
        }

        [Fact]
        public async Task Cancelled_Initialization_Sets_State_To_Cancelled()
        {
            CancellationTestService.Reset();

            ServiceRegistrar registrar = new();

            registrar.RegisterService(
                typeof(ICancellationTestService),
                typeof(CancellationTestService),
                priority: 0,
                loadingWeight: 1,
                failureBehavior: ServiceFailureBehavior.StopInitialization);

            ServiceManager manager = new(registrar, CreateWindowedEnvironment());

            using CancellationTokenSource cancellation = new();

            Task initialization = manager.InitializeAsync(null, cancellation.Token);

            await CancellationTestService.Started.Task.WaitAsync(TimeSpan.FromSeconds(1));

            cancellation.Cancel();

            Func<Task> act = () => initialization;

            await act.Should().ThrowAsync<OperationCanceledException>();

            manager.State.Should().Be(LifecycleState.Cancelled);
        }

        [Fact]
        public async Task Shutdown_Continues_When_A_Service_Shutdown_Throws()
        {
            ShutdownFailureState.Events.Clear();

            ServiceRegistrar registrar = new();

            registrar.RegisterService(
                typeof(IGoodShutdownService),
                typeof(GoodShutdownService),
                priority: 0,
                loadingWeight: 1,
                failureBehavior: ServiceFailureBehavior.StopInitialization);

            registrar.RegisterService(
                typeof(IBadShutdownService),
                typeof(BadShutdownService),
                priority: 0,
                loadingWeight: 1,
                failureBehavior: ServiceFailureBehavior.StopInitialization);

            ServiceManager manager = new(registrar, CreateWindowedEnvironment());

            await manager.InitializeAsync(null, CancellationToken.None);

            manager.Shutdown();

            ShutdownFailureState.Events.Should().ContainInOrder(
                "Bad.Shutdown",
                "Good.Shutdown");

            manager.State.Should().Be(LifecycleState.Shutdown);
        }

        #region Helper Functions
        private static ServiceEnvironment CreateWindowedEnvironment()
        {
            return new ServiceEnvironment(
                new Version(1, 0),
                RuntimePlatform.Windows,
                ApplicationMode.Windowed,
                GraphicsAPI.OpenGL);
        }

        private sealed class CapturingProgress : IProgress<ServiceInitializationProgress>
        {
            public ConcurrentQueue<ServiceInitializationProgress> Updates { get; } = [];

            public void Report(ServiceInitializationProgress value)
            {
                Updates.Enqueue(value);
            }
        }
        #endregion

        #region Data Structures
        public interface IFirstService : IService;
        public interface ISecondService : IService;
        public interface IThirdService : IService;
        public interface IPriorityService : IService;
        public interface ITieService : IService;
        public interface IPlatformService : IService;
        public interface IMissingDependencyService : IService;
        public interface IUnregisteredService : IService;
        public interface ICycleServiceA : IService;
        public interface ICycleServiceB : IService;
        public interface IRollbackFirstService : IService;
        public interface IRollbackFailingService : IService;
        public interface IUnsupportedBehaviorService : IService;
        public interface IShutdownTrackingService : IService;
        public interface IFailingService : IService;
        public interface ICancellationTestService : IService;
        public interface IGoodShutdownService : IService;
        public interface IBadShutdownService : IService;
        public sealed class FirstService : IFirstService
        {
            public async Task InitializeAsync(IServiceProvider services, IProgress<float> progress, CancellationToken cancellation)
            {
                progress.Report(0.5f);
                TestState.FirstStarted.TrySetResult();

                await TestState.ReleaseFirst.Task.WaitAsync(cancellation);

                progress.Report(1f);
            }

            public void Shutdown() { }
        }

        public sealed class SecondService : ISecondService
        {
            public async Task InitializeAsync(IServiceProvider services, IProgress<float> progress, CancellationToken cancellation)
            {
                progress.Report(0.5f);
                TestState.SecondStarted.TrySetResult();

                await TestState.ReleaseSecond.Task.WaitAsync(cancellation);

                progress.Report(1f);
            }

            public void Shutdown() { }
        }

        [DependsOn(typeof(IFirstService))]
        [DependsOn(typeof(ISecondService))]
        public sealed class ThirdService : IThirdService
        {
            public Task InitializeAsync(IServiceProvider services, IProgress<float> progress, CancellationToken cancellation)
            {
                services.GetService(typeof(IFirstService)).Should().NotBeNull();
                services.GetService(typeof(ISecondService)).Should().NotBeNull();

                TestState.ThirdStarted.TrySetResult();
                progress.Report(1f);

                return Task.CompletedTask;
            }

            public void Shutdown() { }
        }

        public sealed class LowPriorityService : IPriorityService
        {
            public Task InitializeAsync(IServiceProvider services, IProgress<float> progress, CancellationToken _) => Task.CompletedTask;

            public void Shutdown() { }
        }

        public sealed class HighPriorityService : IPriorityService
        {
            public Task InitializeAsync(IServiceProvider services, IProgress<float> progress, CancellationToken _) => Task.CompletedTask;

            public void Shutdown() { }
        }

        public sealed class TieServiceA : ITieService
        {
            public static int InstancesCreated { get; set; }

            public TieServiceA() => InstancesCreated++;

            public Task InitializeAsync(IServiceProvider services, IProgress<float> progress, CancellationToken _) => Task.CompletedTask;

            public void Shutdown() { }
        }

        public sealed class TieServiceB : ITieService
        {
            public static int InstancesCreated { get; set; }

            public TieServiceB() => InstancesCreated++;

            public Task InitializeAsync(IServiceProvider services, IProgress<float> progress, CancellationToken _) => Task.CompletedTask;

            public void Shutdown() { }
        }

        [RuntimePlatform(RuntimePlatform.Linux)]
        public sealed class LinuxOnlyService : IPlatformService
        {
            public Task InitializeAsync(IServiceProvider services, IProgress<float> progress, CancellationToken _) => Task.CompletedTask;

            public void Shutdown() { }
        }

        public sealed class CrossPlatformService : IPlatformService
        {
            public Task InitializeAsync(IServiceProvider services, IProgress<float> progress, CancellationToken _) => Task.CompletedTask;

            public void Shutdown() { }
        }

        [DependsOn(typeof(IUnregisteredService))]
        public sealed class MissingDependencyService : IMissingDependencyService
        {
            public static int InstancesCreated { get; set; }

            public MissingDependencyService() => InstancesCreated++;

            public Task InitializeAsync(IServiceProvider services, IProgress<float> progress, CancellationToken _) => Task.CompletedTask;

            public void Shutdown() { }
        }

        [DependsOn(typeof(ICycleServiceB))]
        public sealed class CycleServiceA : ICycleServiceA
        {
            public static int InstancesCreated { get; set; }

            public CycleServiceA() => InstancesCreated++;

            public Task InitializeAsync(IServiceProvider services, IProgress<float> progress, CancellationToken _) => Task.CompletedTask;

            public void Shutdown() { }
        }

        [DependsOn(typeof(ICycleServiceA))]
        public sealed class CycleServiceB : ICycleServiceB
        {
            public static int InstancesCreated { get; set; }

            public CycleServiceB() => InstancesCreated++;

            public Task InitializeAsync(IServiceProvider services, IProgress<float> progress, CancellationToken _) => Task.CompletedTask;

            public void Shutdown() { }
        }

        public sealed class RollbackFirstService : IRollbackFirstService
        {
            public Task InitializeAsync(IServiceProvider services, IProgress<float> progress, CancellationToken _)
            {
                RollbackState.Events.Add("First.Initialize");
                return Task.CompletedTask;
            }

            public void Shutdown()
            {
                RollbackState.Events.Add("First.Shutdown");
            }
        }

        [DependsOn(typeof(IRollbackFirstService))]
        public sealed class RollbackFailingService : IRollbackFailingService
        {
            public Task InitializeAsync(IServiceProvider services, IProgress<float> progress, CancellationToken _)
            {
                RollbackState.Events.Add("Failing.Initialize");
                throw new InvalidOperationException("Expected test failure.");
            }

            public void Shutdown() { }
        }

        public sealed class UnsupportedBehaviorService : IUnsupportedBehaviorService
        {
            public static int InstancesCreated { get; set; }

            public UnsupportedBehaviorService() => InstancesCreated++;

            public Task InitializeAsync(IServiceProvider services, IProgress<float> progress, CancellationToken _) => Task.CompletedTask;

            public void Shutdown() { }
        }

        public sealed class ShutdownTrackingService : IShutdownTrackingService
        {
            public static int ShutdownCalls { get; set; }

            public Task InitializeAsync(
                IServiceProvider services,
                IProgress<float> progress,
                CancellationToken cancellation)
            {
                progress.Report(1f);
                return Task.CompletedTask;
            }

            public void Shutdown()
            {
                ShutdownCalls++;
            }
        }

        public sealed class FailingService : IFailingService
        {
            public Task InitializeAsync(
                IServiceProvider services,
                IProgress<float> progress,
                CancellationToken cancellation)
            {
                throw new InvalidOperationException("Expected test failure.");
            }

            public void Shutdown() { }
        }

        public sealed class CancellationTestService : ICancellationTestService
        {
            public static TaskCompletionSource Started { get; private set; } = null!;

            public static void Reset()
            {
                Started = new TaskCompletionSource(
                    TaskCreationOptions.RunContinuationsAsynchronously);
            }

            public async Task InitializeAsync(
                IServiceProvider services,
                IProgress<float> progress,
                CancellationToken cancellation)
            {
                Started.TrySetResult();

                await Task.Delay(Timeout.InfiniteTimeSpan, cancellation);
            }

            public void Shutdown() { }
        }

        public sealed class GoodShutdownService : IGoodShutdownService
        {
            public Task InitializeAsync(
                IServiceProvider services,
                IProgress<float> progress,
                CancellationToken cancellation)
            {
                return Task.CompletedTask;
            }

            public void Shutdown()
            {
                ShutdownFailureState.Events.Add("Good.Shutdown");
            }
        }

        [DependsOn(typeof(IGoodShutdownService))]
        public sealed class BadShutdownService : IBadShutdownService
        {
            public Task InitializeAsync(
                IServiceProvider services,
                IProgress<float> progress,
                CancellationToken cancellation)
            {
                return Task.CompletedTask;
            }

            public void Shutdown()
            {
                ShutdownFailureState.Events.Add("Bad.Shutdown");
                throw new InvalidOperationException("Expected shutdown failure.");
            }
        }

        private static class ShutdownFailureState
        {
            public static List<string> Events { get; } = [];
        }

        private static class RollbackState
        {
            public static List<string> Events { get; } = [];
        }

        private static class TestState
        {
            public static TaskCompletionSource FirstStarted { get; private set; } = null!;
            public static TaskCompletionSource SecondStarted { get; private set; } = null!;
            public static TaskCompletionSource ThirdStarted { get; private set; } = null!;

            public static TaskCompletionSource ReleaseFirst { get; private set; } = null!;
            public static TaskCompletionSource ReleaseSecond { get; private set; } = null!;

            public static void Reset()
            {
                FirstStarted = NewSource();
                SecondStarted = NewSource();
                ThirdStarted = NewSource();
                ReleaseFirst = NewSource();
                ReleaseSecond = NewSource();
            }

            private static TaskCompletionSource NewSource()
            {
                return new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            }
        }
        #endregion
    }
}
