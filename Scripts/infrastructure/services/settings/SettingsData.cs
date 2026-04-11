using System;

namespace infrastructure.services.settings
{
    [Serializable]
    public class SettingsData
    {
        public float sfxVolume = 1f;
        public float musicVolume = 1f;
        public int graphicsPreset = 2;   // 0=Minimum … 4=Maximum
        public int renderDistance = 5;   // 3–10
        public bool shadowsEnabled = true;
        public int shadowQuality = 1;    // 0=Low, 1=Medium, 2=High
        public float sensitivityX = 1f;  // multiplier relative to inspector base
        public float sensitivityY = 1f;
    }
}
