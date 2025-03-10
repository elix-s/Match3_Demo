using Common.AssetsSystem;
using Common.UIService;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class StartGameState : IGameState
{
    private readonly IAssetProvider _assetProvider;
    private IAssetUnloader _assetUnloader;
    private IObjectResolver _container;
    private UIService _uiService;
    private IAssetUnloader _loadingWindowUnloader;

    public StartGameState(IAssetProvider assetProvider, IAssetUnloader assetUnloader, IObjectResolver container,
        IAssetUnloader loadingWindowUnloader, UIService uiService)
    {
        _assetProvider = assetProvider;
        _assetUnloader = assetUnloader;
        _container = container;
        _loadingWindowUnloader = loadingWindowUnloader;
        _uiService = uiService;
    }
    
    public async void Enter(object obj)
    {
        Debug.Log("StartGameState Enter");
        _uiService.ShowLoadingScreen(1000).Forget();
        var panel = await _assetProvider.GetAssetAsync<GameObject>("GameState");
        _assetUnloader.AddResource(panel);

        var prefab = _container.Instantiate(panel);
        _assetUnloader.AttachInstance(prefab.gameObject);
    }
    public void Update() {}

    public void Exit()
    {
        _assetUnloader.Dispose();
    }
}