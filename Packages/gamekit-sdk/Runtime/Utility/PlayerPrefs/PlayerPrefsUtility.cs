namespace Estoty.Gamekit.Utility.PlayerPrefs
{
	public static class PlayerPrefsUtility
	{
		public static void SetBool(string key, bool value)
		{
			UnityEngine.PlayerPrefs.SetInt(key, value ? 1 : 0);
			UnityEngine.PlayerPrefs.Save();
		}

		public static bool GetBool(string key, bool defaultValue = false)
		{
			return UnityEngine.PlayerPrefs.HasKey(key) 
				? UnityEngine.PlayerPrefs.GetInt(key) == 1 
				: defaultValue;
		}
	}
}