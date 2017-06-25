// #region Author Information
// // Singleton.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using SpruceFramework.Reflection;

namespace SpruceFramework
{
    public class Singleton<T> where T : class
    {
        private static T instance;
        private static readonly object padlock = new object();

        public static T Instance
        {
            get
            {
                lock (padlock)
                {
                    return instance ?? (instance = Instantiator<T>.Instance());
                }
            }
        }
    }
}