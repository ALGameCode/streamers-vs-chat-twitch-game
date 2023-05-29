/// Created by Hellen Caroline Salvato - Project Mini Games (2023)
using System;
namespace Utils
{
    /// <summary>
    /// Abstract and generalist singleton class to facilitate 
    /// the application of the singleton pattern in other classes
    /// </summary>
    public abstract class Singleton<T> where T : class
    {
        /*
        * I used the Lazy<T> class to ensure lazy initialization of the single instance of the class. 
        * This means that the instance will only be created when it is really needed, and not during 
        * application startup. This improves performance and avoids unnecessary startups.
        */
        private static readonly Lazy<T> instance = new Lazy<T>(() => CreateInstance());

        public static T Instance => instance.Value;

        protected Singleton()
        {
            // Protects the constructor to prevent direct creation of the Singleton class.
            // Only derived classes can access this constructor.
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private static T CreateInstance()
        {
            /*
            * I used Activator.CreateInstance to create the derived class instance. This allows us to 
            * handle non-public or parameterized constructors. If an exception occurs during instance 
            * creation, we throw a more suitable exception. 
            */
            try
            {
                return Activator.CreateInstance(typeof(T), nonPublic: true) as T;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create an instance of {typeof(T)}.", ex);
            }
        }
    }
}