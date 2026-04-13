using System.IO;
using UnityEngine;

namespace Arkeum.Prototype
{
    public sealed class PrototypeSaveService
    {
        private const string ProfileFileName = "arkeum_profile.json";
        private readonly string profilePath;

        public PrototypeSaveService()
        {
            profilePath = Path.Combine(Application.persistentDataPath, ProfileFileName);
        }

        public ProfileSaveData LoadProfile()
        {
            if (!File.Exists(profilePath))
            {
                return new ProfileSaveData();
            }

            string json = File.ReadAllText(profilePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new ProfileSaveData();
            }

            ProfileSaveData data = JsonUtility.FromJson<ProfileSaveData>(json);
            return data ?? new ProfileSaveData();
        }

        public void SaveProfile(ProfileSaveData data)
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(profilePath, json);
        }

        public string GetProfilePath()
        {
            return profilePath;
        }
    }
}
