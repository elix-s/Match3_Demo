using Common.GameStateService;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class FinishGameView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private Button _replayButton;
    [SerializeField] private Button _toMenuButton;
    
    private GameStateService _gameStateService;

    [Inject]
    private void Construct(GameStateService gameStateService)
    {
        _gameStateService = gameStateService;
    }

    private void Awake()
    {
        _replayButton.onClick.AddListener(()=> _gameStateService.ChangeState<StartGameState>());
        _toMenuButton.onClick.AddListener(()=> _gameStateService.ChangeState<MenuState>());
    }

    public void Init(string score)
    {
        _scoreText.text = "Your result: "+score;
    }
}
