using System;
using System.Collections;
public sealed class EventsManager : Singleton<EventsManager>
{
    public event Action OnUpdateSelectedBlockUI;
    public void UpdateSelectedBlockUI()
    {
        OnUpdateSelectedBlockUI?.Invoke();
    }

    public event Action OnUpdatePerformanceDebugScreenUI;
    public event Action OnUpdatePlayerDebugScreenUI;
    public event Action OnUpdateSpecsDebugScreenUI;
    public void UpdateDebugScreenUI(bool performanceGUIArea = false, bool playerGUIArea = false, bool specsGUIArea = false)
    {
        if (performanceGUIArea)
        {
            OnUpdatePerformanceDebugScreenUI?.Invoke();
        }

        if (playerGUIArea)
        {
            OnUpdatePlayerDebugScreenUI?.Invoke();
        }

        if (specsGUIArea)
        {
            OnUpdateSpecsDebugScreenUI?.Invoke();
        }
    }
}
