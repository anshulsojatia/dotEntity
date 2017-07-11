// #region Author Information
// // Singleton.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using DotEntity.Reflection;

namespace DotEntity
{
    public class Singleton<T> where T : class
    {
        private static T instance = Instantiator<T>.Instance();
       // private static readonly object padlock = new object();

        public static T Instance
        {
            get
            {
               /* if (instance == null)
                {
                    lock (padlock)
                    {
                        if (instance == null)
                            instance = Instantiator<T>.Instance();
                    }
                }*/
                return instance;

            }
        }
    }
}