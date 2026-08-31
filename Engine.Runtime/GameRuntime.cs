using System.Reflection;

using Engine.Core.Services;

namespace Engine.Runtime
{
    public sealed class GameRuntime : IDisposable
    {
        public GameRuntime(ServiceEnvironment environment, IEnumerable<Assembly> assemblies)
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Update(TimeSpan elapsed)
        {
            throw new NotImplementedException();
        }

        public void Draw()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public T Get<T>() where T : class, IService
        {
            throw new NotImplementedException();
        }
    }
}
