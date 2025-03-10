using Common.GameStateService;
using Common.SavingSystem;

public class StartLoadingState : IGameState
{
    private GameStateService _gameState;
    private readonly Logger _logger;
    
    public StartLoadingState(GameStateService gameStateService, Logger logger)
    {
        _gameState = gameStateService;
        _logger = logger;
    }
    
    public void Enter(object obj)
    {
        _logger.Log("StartLoadingState");
        _gameState.ChangeState<MenuState>();
    }

    public void Update(){}
    
    public void Exit() {}
}
