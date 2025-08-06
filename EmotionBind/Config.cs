using System.ComponentModel;
using Exiled.API.Interfaces;

namespace EmotionSwitcher
{
    public class Config : IConfig
    {
        [Description("Включен ли плагин")]
        public bool IsEnabled { get; set; } = true;

        [Description("Включена ли отладка")]
        public bool Debug { get; set; } = true;
    }
}