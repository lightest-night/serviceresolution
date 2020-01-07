# Lightest Night
## Service Resolution

All things related to making Service Resolution easy and encapsulated throughout the LightestNight ecosystem.

Allows a single abstraction across Service Resolution to keep dependencies to a minimum

#### How To Use
##### Service Resolution
* Asp.Net Standard/Core Dependency Injection
  * Use the provided `services.AddServiceResolution()` method
  
* Other Containers
  * Map `ServiceFactory` to your container's activator. For example, in StructureMap it would look something like:
  > `cfg.For<ServiceFactory>().Use<ServiceFactory>(ctx => ctx.GetInstance);`

----                                                                                                                                        
                                                                                                                                                                                                                                                                                  
##### Assembly Scanning
It is possible to utilise Assembly Scanning to load services into the dependency container.
* Use the static `AssemblyScanning.RegisterServices(IServiceCollection, IEnumerable<Assembly>, IEnumerable<ConcreteRegistration>)` method to specify the assembly and interfaces to map for.