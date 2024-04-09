using UIFramework;

using UnityEngine;
using UnityEngine.UI;

public class ExampleController : Controller<ExampleController>
{
    public RectTransform rectTransform
    {
        get
        {
            if (_rectTransform == null)
            {
                _rectTransform = GetComponent<RectTransform>();
            }
            return _rectTransform;
        }
    }
    private RectTransform _rectTransform = null;

    public CanvasGroup canvasGroup
    {
        get
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }
            return _canvasGroup;
        }
    }
    private CanvasGroup _canvasGroup = null;

    [SerializeField] private UIFramework.UGUI.ScreenCollector<ExampleController> _uguiScreenCollector = new UIFramework.UGUI.ScreenCollector<ExampleController> ();
    [SerializeField] private UIFramework.UIToolkit.ScreenCollector<ExampleController> _uiToolkitScreenCollector = new UIFramework.UIToolkit.ScreenCollector<ExampleController>();

    [SerializeField] private Button backButton = null;

    protected override UpdateTimeMode updateTimeMode { get; set; } = UpdateTimeMode.Scaled;
    protected override StateAnimationMode stateAnimationMode { get { return StateAnimationMode.Screen | StateAnimationMode.Self; } }

    protected override IScreenCollector<ExampleController>[] screenCollectors
    {
        get
        {
            if (_screenCollectors == null)
            {
                _screenCollectors = new IScreenCollector<ExampleController>[] { _uguiScreenCollector, _uiToolkitScreenCollector };
            }
            return _screenCollectors;
        }
    }
    private IScreenCollector<ExampleController>[] _screenCollectors = null;

    protected override void OnInit()
    {
        base.OnInit();
        gameObject.SetActive(false);
        backButton.onClick.AddListener(navigation.Back);
    }

    public override IWindowAnimator CreateAnimator()
    {
        return new UIFramework.UGUI.WindowAnimator(rectTransform.root as RectTransform, rectTransform, canvasGroup);
    }

    protected override void UpdateUI(float deltaTime)
    {
        base.UpdateUI(deltaTime);

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            NavigationEvent navigationEvent = navigation.Back(true);
            if(navigationEvent.exit)
            {
                WindowAnimation animation = new WindowAnimation(WindowAnimation.Type.Fade, 0.5F, EasingMode.EaseInOut, 0.5F, UIFramework.PlayMode.Reverse);
                Close(in animation);
            }
        }
    }

    public override void SetWaiting(bool waiting)
    {
        throw new System.NotImplementedException();
    }

    protected override void SetBackButtonActive(bool active)
    {
        if(navigation.activeScreen != null && navigation.activeScreen.SetBackButtonActive(active))
        {
            backButton?.gameObject.SetActive(false);
        }        
        else
        {
            backButton?.gameObject.SetActive(active);
        }
    }

    protected override void OnOpen()
    {
        base.OnOpen();
        gameObject.SetActive(true);
    }

    protected override void OnClosed()
    {
        base.OnClosed();
        gameObject.SetActive(false);
    }
}
