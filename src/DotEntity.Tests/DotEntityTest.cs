using System;

namespace DotEntity.Tests
{
    public class DotEntityTest
    {
        protected bool IsAppVeyor = Environment.GetEnvironmentVariable("appveyor") == "true";
    }
}