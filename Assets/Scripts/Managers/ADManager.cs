using GoogleMobileAds.Api;
using UnityEngine;

public class ADManager : MonoBehaviour
{
    public static ADManager Instance { get; private set; }

    private string bannerId;
    private BannerView banner;

    private InterstitialAd interAD;
    private System.Action OnCloseInterAD;

    private RewardedAd reward;
    private System.Action OnCloseReward;

#if UNITY_ANDROID // TODO 테스트용 ID
    private const string BANNER_ID = "ca-app-pub-3940256099942544/6300978111";
    private const string INTERSTITIAL_ID = "ca-app-pub-3940256099942544/1033173712";
    private const string REWARDED_ID = "ca-app-pub-3940256099942544/5224354917";
#elif UNITY_IPHONE
    private const string BANNER_ID = "ca-app-pub-3940256099942544/2934735716";
    private const string INTERSTITIAL_ID = "ca-app-pub-3940256099942544/4411468910";
    private const string REWARDED_ID = "ca-app-pub-3940256099942544/1712485313";
#endif

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        MobileAds.Initialize(_ => { });

        bannerId = BANNER_ID;
        LoadInterAD();
        LoadReward();
    }

#if UNITY_EDITOR
    private void Update()
    {
        var adObj = GameObject.Find("New Game Object");
        if (adObj != null)
        {
            adObj.name = "ADPlaceholder";
            adObj.transform.SetParent(transform);
        }
    }
#endif

    private void OnDestroy()
    {
        banner?.Destroy();
        interAD?.Destroy();
        reward?.Destroy();

        banner = null;
        interAD = null;
        reward = null;
    }

    #region 배너
    public void CreateBanner(bool _show)
    {
        if (_show)
        {
            if (banner == null)
            {
                var size = AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
                banner = new BannerView(bannerId, size, AdPosition.Bottom);
                banner.LoadAd(new AdRequest());
                RegisterBanner();
            }
            else banner.Show();
        }
        else banner?.Hide();
    }

    private void RegisterBanner()
    {
        if (banner == null) return;
        banner.OnBannerAdLoaded += () => { };
        banner.OnBannerAdLoadFailed += _ => { };
        banner.OnAdPaid += _ => { };
        banner.OnAdImpressionRecorded += () => { };
        banner.OnAdClicked += () => { };
        banner.OnAdFullScreenContentOpened += () => { };
        banner.OnAdFullScreenContentClosed += () => { };
    }
    #endregion

    #region 전면 광고
    public void LoadInterAD()
    {
        interAD?.Destroy();
        var request = new AdRequest();

        InterstitialAd.Load(INTERSTITIAL_ID, request, (_ad, _error) =>
        {
            if (_error != null || _ad == null) return;
            interAD = _ad;
            RegisterInterAD(interAD);
        });
    }

    public void ShowInterAD(System.Action _onClosed = null)
    {
        if (interAD != null && interAD.CanShowAd())
        {
            OnCloseInterAD = _onClosed;
            interAD.Show();
        }
        else
        {
            LoadInterAD();
            _onClosed?.Invoke();
        }
    }

    private void RegisterInterAD(InterstitialAd _ad)
    {
        _ad.OnAdPaid += _ => { };
        _ad.OnAdImpressionRecorded += () => { };
        _ad.OnAdClicked += () => { };
        _ad.OnAdFullScreenContentOpened += () => { };
        _ad.OnAdFullScreenContentClosed += () =>
        {
            OnCloseInterAD?.Invoke();
            OnCloseInterAD = null;
            LoadInterAD();
        };
        _ad.OnAdFullScreenContentFailed += _ =>
        {
            OnCloseInterAD?.Invoke();
            OnCloseInterAD = null;
            LoadInterAD();
        };
    }
    #endregion

    #region 리워드 광고
    public void LoadReward()
    {
        reward?.Destroy();
        var request = new AdRequest();
        RewardedAd.Load(REWARDED_ID, request, (_ad, _error) =>
        {
            if (_error != null || _ad == null) return;
            reward = _ad;
            RegisterReward(reward);
        });
    }

    public void ShowReward(System.Action _onClosed = null)
    {
        if (reward != null && reward.CanShowAd())
        {
            OnCloseReward = _onClosed;
            reward.Show(_reward => OnReward(_reward));
        }
        else
        {
            LoadReward();
            _onClosed?.Invoke();
        }
    }

    private void OnReward(Reward _reward)
    {
        // TODO 보상 처리 로직
    }

    private void RegisterReward(RewardedAd _ad)
    {
        _ad.OnAdPaid += _ => { };
        _ad.OnAdImpressionRecorded += () => { };
        _ad.OnAdClicked += () => { };
        _ad.OnAdFullScreenContentOpened += () => { };
        _ad.OnAdFullScreenContentClosed += () =>
        {
            OnCloseReward?.Invoke();
            OnCloseReward = null;
            LoadReward();
        };
        _ad.OnAdFullScreenContentFailed += _ =>
        {
            OnCloseReward?.Invoke();
            OnCloseReward = null;
            LoadReward();
        };
    }
    #endregion
}
