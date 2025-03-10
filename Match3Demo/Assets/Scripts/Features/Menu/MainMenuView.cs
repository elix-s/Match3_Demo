using System;
using Common.GameStateService;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class MainMenuView : MonoBehaviour
{
    [SerializeField] private Button _startGameButton;
    [SerializeField] private LogoAnimation _logoAnimation;
    private GameStateService _gameStateService;

    [Inject]
    private void Construct(GameStateService gameStateService)
    {
        _gameStateService = gameStateService;
    }

    private void Awake()
    {
        _startGameButton.onClick.AddListener(()=> _gameStateService.ChangeState<StartGameState>());
    }

    private void Start()
    {
        _logoAnimation.Init();
    }
}
