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
        exampleController.OpenScreen<UGUIExampleTransitionScreen>();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            if(!exampleController.isOpen)
            {
                exampleController.OpenScreen<UGUIExampleTransitionScreen>(new WindowAccessPlayable(GenericWindowAnimationType.Fade, 0.5F, EasingMode.EaseInOut));
            }
        }
    }
}
