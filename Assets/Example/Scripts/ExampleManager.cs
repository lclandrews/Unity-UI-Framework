using UnityEngine;
using UIFramework;

public class ExampleManager : MonoBehaviour
{
    [SerializeField] private ExampleController exampleController = null;

    private void Awake()
    {
        exampleController.Init();
    }

    private void Start()
    {
        exampleController.Open<UGUIExampleTransitionScreen>(null);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            WindowAnimation animation = new WindowAnimation(WindowAnimation.Type.Fade, 0.5F, EasingMode.EaseInOut);
            exampleController.Open<UGUIExampleTransitionScreen>(in animation, null);
        }
    }
}
