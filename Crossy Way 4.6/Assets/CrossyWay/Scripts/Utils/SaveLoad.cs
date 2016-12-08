using UnityEngine;
using System.Collections;

public static class SaveLoad {

	public static int score{
		set{
			if(score < value)
				PlayerPrefs.SetInt("BestScore", value);
		}
		get{return PlayerPrefs.GetInt("BestScore", 0);}
	}

	public static string characterRecentUse{
		set{
			PlayerPrefs.SetString("CharacterRecentUse", value);
		}
		get{
			return PlayerPrefs.GetString("CharacterRecentUse", CharacterShop.instance.characters[0].chara.name);
		}
	}

	public static int coins{
		set{PlayerPrefs.SetInt("Coin", value);}
		get{return PlayerPrefs.GetInt("Coin", 0);}
	}
}
