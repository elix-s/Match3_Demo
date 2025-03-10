using Common.UIService;
using Cysharp.Threading.Tasks;

public class FinishGameState : IGameState
{
    private UIService _uiService;
    private Logger _logger;

    public FinishGameState(UIService uiService, Logger logger)
    {
        _uiService = uiService;
        _logger = logger;
    }
    
    public async void Enter(object obj)
    {
        _logger.Log("StartGameState Enter");
        _uiService.ShowLoadingScreen(1000).Forget();
        var window = await _uiService.ShowUIPanelWithComponent<FinishGameView>("FinishGameScreen");
        
        if(obj != null) window.Init(obj.ToString());
      
    }
    public void Update() {}
    
    public void Exit()
    {
        _uiService.HideUIPanel().Forget();
    }
}
