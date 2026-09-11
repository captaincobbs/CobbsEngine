namespace Engine.Core.Services.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class RegisterServiceAttribute : Attribute
    {
        public Type ServiceType { get; }
        public int Priority { get; }
        public int LoadingWeight { get; }
        public ServiceFailureBehavior FailureBehavior { get; }

        public RegisterServiceAttribute(Type interfaceType, int priority = 0, int loadingWeight = 1, ServiceFailureBehavior failureBehavior = ServiceFailureBehavior.StopInitialization)
        {
            ServiceType = interfaceType;
            Priority = priority;
            LoadingWeight = loadingWeight;
            FailureBehavior = failureBehavior;
        }
    }
}
