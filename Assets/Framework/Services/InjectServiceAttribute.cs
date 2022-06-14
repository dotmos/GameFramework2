using System;

namespace Framework.Services {
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class InjectServiceAttribute : Attribute {

    }
}