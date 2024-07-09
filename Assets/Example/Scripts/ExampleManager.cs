using UnityEngine;
using UIFramework;

using UnityEngine.Extension;

public class ExampleManager : MonoBehaviour, IUpdatable
{
    [SerializeField] private ExampleController exampleController = null;

    private void Awake()
    {
        exampleController.Init();
        UpdateManager.AddUpdatable(this);
    }

    private void Start()
    {
        exampleController.OpenScreen<UGUIExampleTransitionScreen>();
    }

    public void ManagedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!exampleController.IsOpen)
            {
                exampleController.OpenScreen<UGUIExampleTransitionScreen>(new WindowAccessPlayable(GenericWindowAnimationType.Fade, 0.5F, EasingMode.EaseInOut));
            }
        }
    }

    private void OnDestroy()
    {
        UpdateManager.RemoveUpdatable(this);
    }        
}
