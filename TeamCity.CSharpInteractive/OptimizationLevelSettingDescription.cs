// ReSharper disable ClassNeverInstantiated.Global
namespace TeamCity.CSharpInteractive
{
    using System;
    using Microsoft.CodeAnalysis;

    internal class OptimizationLevelSettingDescription : ISettingDescription
    {
        public bool IsVisible => true;

        public Type SettingType => typeof(OptimizationLevel);

        public string Key => "ol";

        public string Description => "Set an optimization level";
    }
}