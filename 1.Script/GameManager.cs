using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using GoogleMobileAds.Api;

public class GameManager : MonoBehaviour
{
    public static string testText;
    public Text test;

    public StartGame StartGame;
    public Login Login;
    public Transform HelpGameObj;
    public GameObject CollectionGameObj;
    public Text checkLogin;

    public firebaseManager firebaseMgr;
    public void Awake()
    {
        firebaseMgr = new();
        StartGame.startObjScript += ChangeScene;

        firebaseMgr.StateUI += CheckLogin;

        SetResolution();
        ClickGoogle();
    }

    public void CheckLogin(bool check,string text)
    {
        checkLogin.text = text;
    }

    public void SetResolution()
    {
        int setWidth = 1080;
        int setHeight = 1920;

        int deviceWidth = Screen.width;
        int deviceHeight = Screen.height;

        Screen.SetResolution(setWidth, (int)(((float)deviceHeight / deviceWidth) * setWidth), true);

        if ((float)setWidth / setHeight < (float)deviceWidth / deviceHeight) // 기기의 해상도 비가 더 큰 경우
        {
            float newWidth = ((float)setWidth / setHeight) / ((float)deviceWidth / deviceHeight); // 새로운 너비
            Camera.main.rect = new Rect((1f - newWidth) / 2f, 0f, newWidth, 1f);
        }
        else // 게임의 해상도 비가 더 큰 경우
        {
            float newHeight = ((float)deviceWidth / deviceHeight) / ((float)setWidth / setHeight); // 새로운 높이
            Camera.main.rect = new Rect(0f, (1f - newHeight) / 2f, 1f, newHeight);
        }

    }

    public void ChangeScene(StartGame obj)
    {
        if (obj.gameObject.activeSelf) Login.gameObject.SetActive(false);
        else
        {
            if (obj.timer <= 0)
            {
                Debug.Log($"save : {obj.nowScore}");
                firebaseMgr.SaveScoreInGooglePlayFlatform(obj.nowScore);
            }
            Login.gameObject.SetActive(true);
        }
    }

   public void clickRank()
   {
       firebaseMgr.ShowAllScore();
   }

    public void ClickGoogle()
    {
        firebaseMgr.LogIn();
        Login.beforeLogin.SetActive(false);
        Login.afterLogin.SetActive(true);
    }


    public void Update()
    {
        test.text = testText;
    }

    public void ClickHelp()
    {
        HelpGameObj.gameObject.SetActive(true);
        for (int i = 0; i < HelpGameObj.childCount; i++)
        {
            if(i == 0) HelpGameObj.GetChild(i).gameObject.SetActive(true);
            else HelpGameObj.GetChild(i).gameObject.SetActive(false);
        }
    }

    public void ClickCollection()
    {
        CollectionGameObj.SetActive(true);
    }


    //add

    public void Createadd()
    {
        LoadInterstitialAd();
        ShowAd();
    }

    // These ad units are configured to always serve test ads.
#if UNITY_ANDROID
    private string _adUnitId = "ca-app-pub-3940256099942544/1033173712";
#elif UNITY_IPHONE1
  private string _adUnitId = "ca-app-pub-3940256099942544/4411468910";
#else
  private string _adUnitId = "unused";
#endif

    private InterstitialAd interstitialAd;

    /// <summary>
    /// Loads the interstitial ad.
    /// </summary>
    public void LoadInterstitialAd()
    {
        // Clean up the old ad before loading a new one.
        if (interstitialAd != null)
        {
            interstitialAd.Destroy();
            interstitialAd = null;
        }

        Debug.Log("Loading the interstitial ad.");
        // create our request used to load the ad.
        var adRequest = new AdRequest();
        adRequest.Keywords.Add("unity-admob-sample");

        // send the request to load the ad.
        InterstitialAd.Load(_adUnitId, adRequest,
            (InterstitialAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    Debug.LogError("interstitial ad failed to load an ad " +
                                   "with error : " + error);

                    return;
                }

                Debug.Log("Interstitial ad loaded with response : "
                          + ad.GetResponseInfo());
                interstitialAd = ad;

            });

    }

    /// <summary>
    /// Shows the interstitial ad.
    /// </summary>
    public void ShowAd()
    {
        StartCoroutine(showInterstitial());

        IEnumerator showInterstitial()
        {
            while(!(interstitialAd != null && interstitialAd.CanShowAd()))
            {
                yield return new WaitForSeconds(0.2f);
            }
            interstitialAd.Show();
        }
    }

    private void RegisterEventHandlers(InterstitialAd ad)
    {
        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("Interstitial ad paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Interstitial ad recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () =>
        {
            Debug.Log("Interstitial ad was clicked.");
        };
        // Raised when an ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Interstitial ad full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Interstitial ad full screen content closed.");
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Interstitial ad failed to open full screen content " +
                           "with error : " + error);
        };
    }

    private void RegisterReloadHandler(InterstitialAd ad)
    {
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += (null);
        {
            Debug.Log("Interstitial Ad full screen content closed.");

            // Reload the ad so that we can show another as soon as possible.
            LoadInterstitialAd();
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Interstitial ad failed to open full screen content " +
                           "with error : " + error);

            // Reload the ad so that we can show another as soon as possible.
            LoadInterstitialAd();
        };
    }


}
