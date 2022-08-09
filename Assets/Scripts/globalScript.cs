using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

[System.Serializable]
public class SampleState {
	public int currentQuest;
	public string currentWeapon;
	public string equippedSword;
	public string equippedBow;
	public string equippedHelm;
	public string equippedMagic;
	public int questStep;
	public int currentGold;
	public string lastScene;
	public string prevScene;
	public bool bow1Purchased;
	public bool bow2Purchased;
	public bool bow3Purchased;
	public bool bow4Purchased;
	public bool bow5Purchased;
	public bool sword1Purchased;
	public bool sword2Purchased;
	public bool sword3Purchased;
	public bool sword4Purchased;
	public bool sword5Purchased;
	public bool helm1Purchased;
	public bool helm2Purchased;
	public bool helm3Purchased;
	public bool helm4Purchased;
	public bool helm5Purchased;
}

public class globalScript : MonoBehaviour {
	public static bool useBackend = false;

	public string androidMoreGamesURL;
	public string iosMoreGamesURL;
	public string androidDemoButtonURL;
	public string iOSDemoButtonURL;

	public string unlockAllIapAndroid;
	public string unlockAllIapIOS;

    public bool removeAds;
	public bool removeIAP;
	public bool resetsSavedData;

	public static string moreGamesMarketURL = "";
	public static string demoButtonURL = "";

	public static string unlockAlliAP;

	public static bool hasAlreadyLoaded;


	public static string previousScene;
	public static string selectionHandle;

	public static int currentQuest;
	public static int questStep;
	public static string currentWeapon = "sword";
	public static string equippedSword = "sword1";
	public static string equippedBow = "bow1";
	public static string equippedHelm = "";
	public static string equippedMagic = "earth";
	public static bool bow1Purchased;
	public static bool bow2Purchased;
	public static bool bow3Purchased;
	public static bool bow4Purchased;
	public static bool bow5Purchased;
	public static bool sword1Purchased;
	public static bool sword2Purchased;
	public static bool sword3Purchased;
	public static bool sword4Purchased;
	public static bool sword5Purchased;
	public static bool helm1Purchased;
	public static bool helm2Purchased;
	public static bool helm3Purchased;
	public static bool helm4Purchased;
	public static bool helm5Purchased;
	public static bool chaosHitEtherboyOnce;

	public static string gameState;

	public static int currentGold;

	public static float magicTimer;

	public static int startingOrderEnemies;
	public static int startingOrderNPCs;

	public static float shakeScreenTime;
	public static GameObject groupToShake;
	public static Vector2 basePositionGroupToShake;

	public static string lastPlayedScene;
	public static string sceneBeforeDeath;


	private static GameObject fader;


	public static void giveCoins () {
		int currentCoins = PlayerPrefs.GetInt ("currentCoins");
		currentCoins += 1000;
		PlayerPrefs.SetInt ("currentCoins", currentCoins);
	}

	public static void showFullScreenAd () {
		bool noAds = false;
		if (PlayerPrefs.GetInt ("isAdsRemoved") == 1) {
			noAds = true;
		}
		if (!noAds) {
			if (Application.isMobilePlatform) {
			/*	if (ApplovinAds.HasInterstitial()) {
					ApplovinAds.ShowInterstital ();
				} else {
					Admob.ShowInterstitial();
				} */
			}
		}
	}

	public static void showVideoAd () {
		bool noAds = false;
		if (!noAds) {
			if (Application.isMobilePlatform) {
			//	ApplovinAds.ShowRewardedVideo (giveCoins);
			}
		}
	}

	public static void changeScene(string sceneName) {
		if (sceneName == "gameOverScene") {
			globalScript.sceneBeforeDeath = globalScript.previousScene;
		}

		if (SceneManager.GetActiveScene ().name != "gameOverScene") {
			globalScript.previousScene = SceneManager.GetActiveScene ().name;
		} else {
			globalScript.previousScene = globalScript.sceneBeforeDeath;
		}

		startingOrderEnemies = 0;
		startingOrderNPCs = 0;

		if (fader != null) {
			fader.SetActive (true);
			fader.transform.position = new Vector2(Camera.main.transform.position.x, Camera.main.transform.position.y);
			AudioSource music = Camera.main.GetComponent<AudioSource> ();
			print (music);
			if (music != null) {
				LeanTween.value (1, 0, 0.25f).setOnUpdate ((float value) => {
					if (PlayerPrefs.GetInt ("volumeMusic") == 0) {
						if (music != null) {
							music.volume = value;
						}
					}
				});
			}
			LeanTween.alpha (fader, 1, 0.25f).setOnComplete (() => {
				SceneManager.LoadScene (sceneName);
				if (sceneName != "gameOverScene" && sceneName != "menuScene") {
					LeanTween.value(0, 1, 1.5f).setOnComplete(()=>{
						globalScript.saveGame();
					});
				}
				AudioSource music2 = Camera.main.GetComponent<AudioSource> ();
				if (music2 != null) {
					LeanTween.value (0, 1, 0.25f).setOnUpdate ((float value) => {
						if (PlayerPrefs.GetInt ("volumeMusic") == 0) {
							if (music2 != null) {
								music2.volume = value;
							}
						}
					});
				}
				LeanTween.alpha(fader, 0, 0.25f).setOnComplete (() => {
					fader.SetActive(false);
				});
			});
		} else {
			SceneManager.LoadScene (sceneName);
			print (sceneName);
			if (sceneName != "gameOverScene" && sceneName != "menuScene") {
				LeanTween.value(0, 1, 1.5f).setOnComplete(()=>{
					globalScript.saveGame();
				});
			}
		}
	}

	public static void loadGame (SampleState saveData) {
		if (!useBackend) {
			saveData = JsonUtility.FromJson<SampleState> (PlayerPrefs.GetString ("savedData"));
		}
		if (saveData != null) {
			globalScript.currentGold = saveData.currentGold;
			globalScript.currentQuest = saveData.currentQuest;
			globalScript.currentWeapon = saveData.currentWeapon;
			globalScript.questStep = saveData.questStep;
			globalScript.lastPlayedScene = saveData.lastScene;
			globalScript.equippedBow = saveData.equippedBow;
			globalScript.equippedHelm = saveData.equippedHelm;
			globalScript.equippedSword = saveData.equippedSword;
			globalScript.equippedMagic = saveData.equippedMagic;
			globalScript.previousScene = saveData.prevScene;
			globalScript.bow1Purchased = saveData.bow1Purchased;
			globalScript.bow2Purchased = saveData.bow2Purchased;
			globalScript.bow3Purchased = saveData.bow3Purchased;
			globalScript.bow4Purchased = saveData.bow4Purchased;
			globalScript.bow5Purchased = saveData.bow5Purchased;
			globalScript.sword1Purchased = saveData.sword1Purchased;
			globalScript.sword2Purchased = saveData.sword2Purchased;
			globalScript.sword3Purchased = saveData.sword3Purchased;
			globalScript.sword4Purchased = saveData.sword4Purchased;
			globalScript.sword5Purchased = saveData.sword5Purchased;
			globalScript.helm1Purchased = saveData.helm1Purchased;
			globalScript.helm2Purchased = saveData.helm2Purchased;
			globalScript.helm3Purchased = saveData.helm3Purchased;
			globalScript.helm4Purchased = saveData.helm4Purchased;
			globalScript.helm5Purchased = saveData.helm5Purchased;

			if (globalScript.lastPlayedScene != "") {
				globalScript.changeScene (globalScript.lastPlayedScene);
			}
		}
	}

	public static void saveGame () {
		SampleState saveData = new SampleState ();
		saveData.currentGold = globalScript.currentGold;
		saveData.currentQuest = globalScript.currentQuest;
		saveData.currentWeapon = globalScript.currentWeapon;
		saveData.questStep = globalScript.questStep;
		saveData.lastScene = SceneManager.GetActiveScene ().name;
		saveData.equippedBow = globalScript.equippedBow;
		saveData.equippedHelm = globalScript.equippedHelm;
		saveData.equippedSword = globalScript.equippedSword;
		saveData.equippedMagic = globalScript.equippedMagic;
		saveData.prevScene = globalScript.previousScene;
		saveData.bow1Purchased = globalScript.bow1Purchased;
		saveData.bow2Purchased = globalScript.bow2Purchased;
		saveData.bow3Purchased = globalScript.bow3Purchased;
		saveData.bow4Purchased = globalScript.bow4Purchased;
		saveData.bow5Purchased = globalScript.bow5Purchased;
		saveData.sword1Purchased = globalScript.sword1Purchased;
		saveData.sword2Purchased = globalScript.sword2Purchased;
		saveData.sword3Purchased = globalScript.sword3Purchased;
		saveData.sword4Purchased = globalScript.sword4Purchased;
		saveData.sword5Purchased = globalScript.sword5Purchased;
		saveData.helm1Purchased = globalScript.helm1Purchased;
		saveData.helm2Purchased = globalScript.helm2Purchased;
		saveData.helm3Purchased = globalScript.helm3Purchased;
		saveData.helm4Purchased = globalScript.helm4Purchased;
		saveData.helm5Purchased = globalScript.helm5Purchased;

		if (!useBackend) {
			string jsonData = JsonUtility.ToJson (saveData);
			if (jsonData != null) {
				PlayerPrefs.SetString ("savedData", jsonData);
			}
		} else {
			GameObject.Find ("backend").GetComponent<backendClass> ().SaveState (saveData);
		}
	}

	public static void fadeToBlack (float seconds) {
		if (fader != null) {
			fader.SetActive (true);
			LeanTween.alpha (fader, 1, 0.25f).setOnComplete (() => {
				globalScript.saveGame();
				LeanTween.alpha (fader, 0, 0.25f).setOnComplete (() => {
					fader.SetActive (false);
				}).setDelay(seconds-0.5f);
			});
		}
	}

	void Update () {
		if (shakeScreenTime > 0) {
			shakeScreenTime -= Time.deltaTime;
			groupToShake.transform.position = new Vector2 (Random.Range (basePositionGroupToShake.x-0.5f, basePositionGroupToShake.x+0.5f), Random.Range (basePositionGroupToShake.y-0.5f, basePositionGroupToShake.y+0.5f));
			if (shakeScreenTime <= 0) {
				groupToShake.transform.position = basePositionGroupToShake;
				shakeScreenTime = 0;
			}
		}

		if (!useBackend) {
			GameObject loginGroup = GameObject.Find ("loginGroup");
			if (loginGroup != null) {
				loginGroup = loginGroup.transform.GetChild (0).gameObject;
				if (loginGroup.activeSelf) {
					loginGroup.SetActive (false);
				}
			}
		} else {
			if (SceneManager.GetActiveScene ().name == "menuScene") {
				if (PlayerPrefs.GetString ("identityString") != "") {
					GameObject loginGroup = GameObject.Find ("loginGroup");
					if (loginGroup != null) {
						loginGroup = loginGroup.transform.GetChild (0).gameObject;
						if (loginGroup.activeSelf) {
							loginGroup.SetActive (false);
							//#if !UNITY_ANDROID
								GameObject.Find ("backend").GetComponent<backendClass> ().SignIn ();
							//#endif
							GameObject menuGroup = GameObject.Find ("menuGroup");
							if (menuGroup != null) {
								menuGroup = menuGroup.transform.GetChild (0).gameObject;
								if (!menuGroup.activeSelf) {
									menuGroup.SetActive (true);
								}
							}
						}
					}
				} else {
					GameObject menuGroup = GameObject.Find ("menuGroup");
					if (menuGroup != null) {
						menuGroup = menuGroup.transform.GetChild (0).gameObject;
						if (menuGroup.activeSelf) {
							menuGroup.SetActive (false);
							GameObject loginGroup = GameObject.Find ("loginGroup");
							if (loginGroup != null) {
								loginGroup = loginGroup.transform.GetChild (0).gameObject;
								if (!loginGroup.activeSelf) {
									loginGroup.SetActive (true);
								}
							}
						}
					}
				}
			}
		}
	}

	void Awake() {
		#if UNITY_EDITOR
			useBackend = false;
		#endif

		if (SceneManager.GetActiveScene ().name == "menuScene") {
			Update ();
		}

		if (hasAlreadyLoaded == true) {
			if (SceneManager.GetActiveScene ().name == "menuScene") {
				GameObject userInterfaceOld = GameObject.Find ("UserInterface");
				if (userInterfaceOld != null) {
					Destroy (userInterfaceOld);
				}
				GameObject talkGroupOld = GameObject.Find ("talkGroup");
				if (talkGroupOld != null) {
					Destroy (talkGroupOld);
				}
			}
			return;
		}

		shakeScreenTime = 0;
		currentGold = 0;
		gameState = "";
		hasAlreadyLoaded = true;
		Application.targetFrameRate = 60;

		bow1Purchased = true;
		sword1Purchased = true;

		currentQuest = 19;
		questStep = 0;

		magicTimer = 0;

		fader = GameObject.Find ("faderBase");

		if (fader != null) {
			fader.name = "fader";
			Object.DontDestroyOnLoad (fader);
			fader.SetActive (false);
		}

		GameObject userInterface = GameObject.Find ("UserInterface");

		if (userInterface != null) {
			userInterface.name = "UserInterfaceFound";
			Object.DontDestroyOnLoad (userInterface);
		}

		GameObject talkGroup = GameObject.Find ("talkGroup");

		if (talkGroup != null) {
			talkGroup.name = "talkGroupFound";
			Object.DontDestroyOnLoad (talkGroup);
		}

		unlockAlliAP = unlockAllIapAndroid;

        if (removeAds == true) {
			PlayerPrefs.SetInt ("isAdsRemoved", 1);
		}

		if (removeIAP == true) {
			PlayerPrefs.SetInt ("isAllUnlocked", 1);
		//	PlayerPrefs.SetInt ("isAdsRemoved", 1);
		}

		if (resetsSavedData == true) {
			PlayerPrefs.DeleteAll ();
		}

		if (PlayerPrefs.GetInt ("firstTime") == 0) {
			PlayerPrefs.SetInt ("firstTime", 1);
			PlayerPrefs.SetInt ("defenseTeamZombie", 1);
		}

		#if UNITY_IOS
			moreGamesMarketURL = iosMoreGamesURL;
			demoButtonURL = iOSDemoButtonURL;
			unlockAlliAP = unlockAllIapIOS;
		#endif
		#if UNITY_ANDROID
			moreGamesMarketURL = androidMoreGamesURL;
			demoButtonURL = androidDemoButtonURL;
			unlockAlliAP = unlockAllIapAndroid;
		#endif
	}
}