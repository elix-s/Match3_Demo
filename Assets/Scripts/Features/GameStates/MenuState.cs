using Cysharp.Threading.Tasks;
using Common.UIService;

public class MenuState : IGameState
{
    private Logger _logger;
    private UIService _uiService;
    
    public MenuState(Logger logger, UIService uiService)
    {
        _logger = logger;
        _uiService = uiService;
    }

    public void Enter(object obj)
    {
        _logger.Log("Entering MenuState");
        _uiService.ShowLoadingScreen(1000).Forget();
        _uiService.ShowUIPanel("MainMenu").Forget();
    }
    
    public void Update()
    {
        
    }

    public void Exit()
    {
        _uiService.HideUIPanel().Forget();
    }
}
