using System;

namespace LightestNight.System.ServiceResolution
{
    /// <summary>
    /// Creates an object of the given Type
    /// </summary>
    /// <param name="serviceType">The type of the object to create</param>
    /// <param name="args">Any arguments to pass to the constructor when creating the object</param>
    public delegate object? ServiceFactory(Type serviceType, params object[] args);
}