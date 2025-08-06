using System;
using Exiled.API.Features;
using Exiled.API.Interfaces;

namespace EmotionSwitcher
{
    public class EmotionSwitcher : Plugin<Config>
    {
        public override string Name => "EmotionSwitcher";
        public override string Author => "DX";
        public override Version Version => new Version(1, 0, 0);

        private EventHandlers _eventHandlers;
        public static EmotionSwitcher Instance { get; private set; }

        public override void OnEnabled()
        {
            Instance = this;
            _eventHandlers = new EventHandlers();
            _eventHandlers.RegisterEvents();
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            _eventHandlers?.UnregisterEvents();
            Instance = null;
            base.OnDisabled();
        }
    }
}