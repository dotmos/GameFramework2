using System;
using System.Collections.Generic;

namespace Framework.Services {
    /// <summary>
    /// Class for handling injection of services using the [InjectService] attribute
    /// </summary>
    public class ServiceInjector {
        /// <summary>
        /// Services registered with the injector. These are ready to be used with the [InjectService] attribute
        /// </summary>
        public static readonly Type InjectAttributeType = typeof(InjectServiceAttribute);
        Dictionary<Type, IService> registeredServices = new Dictionary<Type, IService>();

        public ServiceInjector() {
            registeredServices = new Dictionary<Type, IService>();
        }

        /// <summary>
        /// Register a service with the injector. Registers ALL interfaces of the service and the service class
        /// </summary>
        /// <param name="service"></param>
        public void Register(IService service) {
            //Register all interfaces
            Type[] serviceTypes = service.GetType().GetInterfaces();
            foreach(Type t in serviceTypes) {
                if (registeredServices.ContainsKey(t)) {
                    registeredServices[t] = service;
                } else {
                    registeredServices.Add(t, service);
                }
            }
            //Register class
            if (registeredServices.ContainsKey(service.GetType())) {
                registeredServices[service.GetType()] = service;
            } else {
                registeredServices.Add(service.GetType(), service);
            }
        }

        /// <summary>
        /// Registers the service with the injector, using the given generic T as key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service"></param>
        public void Register<T>(IService service) {
            Type interfaceType = typeof(T);
            if (registeredServices.ContainsKey(interfaceType)) {
                registeredServices[interfaceType] = service;
            } else {
                registeredServices.Add(interfaceType, service);
            }
        }

        /// <summary>
        /// Remove a service from the injector
        /// </summary>
        /// <param name="service"></param>
        public void Unregister(IService service) {
            if (registeredServices.ContainsKey(service.GetType())) {
                registeredServices.Remove(service.GetType());
            }
        }

        /// <summary>
        /// Resolve a generic service
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T Resolve<T>() where T : IService {
            return (T)Resolve(typeof(T));
        }

        /// <summary>
        /// Resolve a service by type
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        object Resolve(Type serviceType) {
            if (registeredServices.ContainsKey(serviceType)) {
                return registeredServices[serviceType];
            } else {
                throw new Exception("Could not resolve service:" + serviceType);
            }
        }

        /// <summary>
        /// Inject all requested services (marked with [InjectService]) to the given instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        public void InjectServicesFor<T>(T instance, bool includeBaseClasses = true) where T : class {
            Type type = instance.GetType();

			do {
				System.Reflection.FieldInfo[] properties = type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);//.Where(prop => prop.IsDefined(typeof(LocalizedDisplayNameAttribute), false));
				for (int i = 0; i < properties.Length; ++i) {
					System.Reflection.FieldInfo prop = properties[i];
					if (prop.IsDefined(InjectAttributeType, false)) {
						prop.SetValue(instance, Resolve(prop.FieldType));
						//Debug.Log("- Resolved " + prop.FieldType);
						//Debug.Log("Resolved " + prop.DeclaringType);
					}
				}

				if (includeBaseClasses) {
					type = type.BaseType;
				} else {
					type = null;
				}
			} while (type != null);

            // To get the values themselves, you'd use:
            //var attributes = (LocalizedDisplayNameAttribute[])
            //prop.GetCustomAttributes(typeof(LocalizedDisplayNameAttribute), false);
        }
    }
}