﻿namespace Nancy.Bootstrappers.Windsor
{
    using System;
    using Nancy.Bootstrapper;

    /// <summary>
    /// Windsor ModuleKey generator - handles Castle proxy classes correctly
    /// </summary>
    public class WindsorModuleKeyGenerator : IModuleKeyGenerator
    {
        /// <summary>
        /// Returns a string key for the given type
        /// </summary>
        /// <param name = "moduleType">NancyModule type</param>
        /// <returns>String key</returns>
        public string GetKeyForModuleType(Type moduleType)
        {
            return moduleType.FullName.StartsWith("Castle.Proxies")
                ? moduleType.BaseType.FullName
                : moduleType.FullName;
        }
    }
}