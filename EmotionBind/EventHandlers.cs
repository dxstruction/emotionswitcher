using System;
using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.FirstPersonControl.Thirdperson.Subcontrollers;

namespace EmotionSwitcher
{
    internal class EventHandlers
    {
        private const string PluginName = "EmotionSwitcher";

        // Индекс эмоций для каждого игрока по ID
        private readonly Dictionary<int, int> _playerEmotionIndex = new Dictionary<int, int>();

        // Список эмоций (должны соответствовать EmotionPresetType)
        private readonly string[] _emotions = new string[]
        {
            "Neutral", "Happy", "AwkwardSmile", "Scared", "Angry", "Chad", "Ogre"
        };

        // Цвета для эмоций по названию
        private readonly Dictionary<string, string> _emotionColors = new Dictionary<string, string>
        {
            {"Neutral", "#FFD700"},      // жёлтый
            {"Happy", "#98FB98"},        // зелёный
            {"AwkwardSmile", "#87CEEB"}, // голубой
            {"Scared", "#FF9900"},       // оранжевый
            {"Angry", "#FF5555"},        // красный
            {"Chad", "#C586FF"},         // фиолетовый
            {"Ogre", "#BBBBBB"}          // серый
        };

        internal void RegisterEvents()
        {
            Exiled.Events.Handlers.Player.TogglingNoClip += OnTogglingNoClip;
            Exiled.Events.Handlers.Player.Left += OnPlayerLeft;
            Exiled.Events.Handlers.Server.RestartingRound += OnRoundRestarting;
        }

        internal void UnregisterEvents()
        {
            Exiled.Events.Handlers.Player.TogglingNoClip -= OnTogglingNoClip;
            Exiled.Events.Handlers.Player.Left -= OnPlayerLeft;
            Exiled.Events.Handlers.Server.RestartingRound -= OnRoundRestarting;
        }

        private void OnTogglingNoClip(TogglingNoClipEventArgs ev)
        {
            var player = ev.Player;

            // Проверки на SCP, смерть, разрешённый noclip
            if (player == null || player.ReferenceHub == null ||
                player.Role.Team == Team.SCPs || !player.IsAlive ||
                FpcNoclip.IsPermitted(player.ReferenceHub))
                return;

            SwitchEmotion(player);

            // Всегда запрещаем noclip для обычных игроков
            ev.IsAllowed = false;
        }

        private void OnPlayerLeft(LeftEventArgs ev)
        {
            if (ev.Player != null)
                _playerEmotionIndex.Remove(ev.Player.Id);
        }

        private void OnRoundRestarting()
        {
            _playerEmotionIndex.Clear();
        }

        private void SwitchEmotion(Player player)
        {
            if (player == null || player.ReferenceHub == null)
                return;

            // Не обрабатываем Chaos Insurgency
            if (player.Role.Team == Team.ChaosInsurgency)
                return;
            // Не обрабатываем Tutorial (обучающий режим)
            if (player.Role == RoleTypeId.Tutorial)
                return;

            // Не обрабатываем все роли NTF (начинаются с "Ntf")
            if (player.Role.Type.ToString().StartsWith("Ntf", StringComparison.OrdinalIgnoreCase)) 
                return;

            int id = player.Id;
            if (!_playerEmotionIndex.ContainsKey(id))
                _playerEmotionIndex[id] = 0;

            _playerEmotionIndex[id] = (_playerEmotionIndex[id] + 1) % _emotions.Length;
            string current = _emotions[_playerEmotionIndex[id]];

            // Цвет для текущей эмоции, если вдруг ключ не найден — белый
            string color = _emotionColors.ContainsKey(current) ? _emotionColors[current] : "#FFFFFF";

            // Смена эмоции через API
            if (Enum.TryParse<EmotionPresetType>(current, true, out var preset))
            {
                player.Emotion = preset;
            }
            else
            {
                Log.Warn($"[{PluginName}] Неизвестная эмоция: {current}");
            }

            // Красивая цветная подсказка
            player.ShowHint(
                $"\n\n<b><color=#ffcc99>Emote:</color> <color={color}>{current}</color></b>",
                3f
            );

            // Debug log (только если включён в конфиге)
            var pluginInstance = EmotionSwitcher.Instance;
            if (pluginInstance?.Config?.Debug ?? false)
                Log.Info($"[{PluginName}] {player.Nickname} переключил эмоцию на {current}");
        }
    }
}
