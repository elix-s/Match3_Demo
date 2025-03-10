using UnityEngine;
using System.Collections.Generic;
using Common.GameStateService;
using Cysharp.Threading.Tasks;
using VContainer; 

public class GameController : MonoBehaviour
{
    [Header("Prefabs and components")]
    [SerializeField] private GameObject _ballPrefab;
    [SerializeField] private Transform _pendulumTransform; 
    [SerializeField] private GameObject _particleEffectPrefab;
    
    [Header("Balls area settings")]
    [SerializeField] private float _dropZoneY = -4f;
    [SerializeField] private float _ballSpacing = 1.1f;
    [SerializeField] private float[] _columnXPositions = new float[3] { -1.2f, 0f, 1.2f };

    [Header("Force of releasing the ball")]
    [SerializeField] private float _releaseForce = 2f;
   
    private List<BallColor> _ballSequence;
    private int _currentMoveIndex = 0;
    private Ball _currentBall = null;
    
    private Ball[,] _grid = new Ball[3, 3];
    private int[] _columnHeights = new int[3] { 0, 0, 0 };
    private int _score = 0;

    private Dictionary<BallColor, int> _colorScoreMapping = new Dictionary<BallColor, int>(){
        { BallColor.Red, 100 },
        { BallColor.Green, 200 },
        { BallColor.Blue, 300 }
    };
    
    private bool _isAligning = false;
    private bool _isProcessingDrop = false;
    
    private GameStateService _gameStateService;
    private Logger _logger;
    
    private List<GameObject> _instantiatedBalls = new List<GameObject>();

    [Inject]
    private void Construct(GameStateService gameStateService, Logger logger)
    {
        _gameStateService = gameStateService;
        _logger = logger;
    }
    
    private void Start() 
    {
        GenerateBallSequence();
        SpawnNextBall();
    }

    private void Update() 
    {
        if (_currentBall != null && !_currentBall.IsReleased)
        {
            if (Input.GetMouseButtonDown(0) || Input.touchCount > 0)
            {
                float ballX = _currentBall.transform.position.x;
                int nearestColumn = GetNearestColumn(ballX);
                
                if (_columnHeights[nearestColumn] >= 3)
                {
                    _logger.Log("Column " + nearestColumn + " is already filled");
                    return;
                }
                
                _currentBall.ReleaseBall(nearestColumn, _releaseForce);
            }
        }
        
        if (_currentBall != null && _currentBall.IsReleased && !_isProcessingDrop)
        {
            _isProcessingDrop = true;
            ProcessBallDrop(_currentBall).Forget();
        }
    }

    // determine the nearest column
    private int GetNearestColumn(float ballX) 
    {
        int nearestColumn = 0;
        float minDist = Mathf.Abs(ballX - _columnXPositions[0]);
        
        for (int i = 1; i < 3; i++)
        {
            float dist = Mathf.Abs(ballX - _columnXPositions[i]);
            if (dist < minDist)
            {
                minDist = dist;
                nearestColumn = i;
            }
        }
        
        return nearestColumn;
    }

    // create ball color sequence
    private void GenerateBallSequence() 
    {
        _ballSequence = new List<BallColor>();
        
        for (int i = 0; i < 3; i++)
        {
            _ballSequence.Add(BallColor.Red);
            _ballSequence.Add(BallColor.Green);
            _ballSequence.Add(BallColor.Blue);
        }
        
        // Fisher-Yates shuffling
        for (int i = 0; i < _ballSequence.Count; i++)
        {
            BallColor temp = _ballSequence[i];
            int randomIndex = Random.Range(i, _ballSequence.Count);
            _ballSequence[i] = _ballSequence[randomIndex];
            _ballSequence[randomIndex] = temp;
        }
    }
    
    private bool HasGameEnded()
    {
        return IsGameOver() || (_currentMoveIndex >= _ballSequence.Count);
    }
    
    private async void SpawnNextBall() 
    {
        if (HasGameEnded())
        {
            await UniTask.Delay(1000);
            _logger.Log("Game over! Final score: " + _score);
            
            foreach (var i in _instantiatedBalls)
            {
                Destroy(i);
            }
            
            _instantiatedBalls.Clear();
            
            _gameStateService.ChangeState<FinishGameState>(_score);
            return;
        }
        
        BallColor nextColor = _ballSequence[_currentMoveIndex];
        _currentMoveIndex++;

        var ballObj = Instantiate(_ballPrefab, _pendulumTransform);
        ballObj.transform.localPosition = new Vector3(0, -1f, 0);
        _instantiatedBalls.Add(ballObj);

        var ballScript = ballObj.GetComponent<Ball>();
        
        if (ballScript != null)
        {
            ballScript.SetBallColor(nextColor);
        }
        
        _currentBall = ballScript;
    }
    
    private bool IsGameOver() 
    {
        for (int i = 0; i < 3; i++)
        {
            if (_columnHeights[i] < 3)
                return false;
        }
        
        return true;
    }
    
    private async UniTaskVoid ProcessBallDrop(Ball ball)
    {
        //Horizontal alignment
        float targetX = _columnXPositions[ball.TargetColumn];
        _isAligning = true;
        float horizDuration = 0.5f;
        float horizElapsed = 0f;
        float startX = ball.transform.position.x;
        
        Rigidbody2D rb = ball.GetComponent<Rigidbody2D>();
        
        if (rb != null)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
        
        while (horizElapsed < horizDuration)
        {
            horizElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(horizElapsed / horizDuration);
            ball.transform.position = new Vector2(Mathf.Lerp(startX, targetX, t), ball.transform.position.y);
            await UniTask.Yield();
        }
        
        ball.transform.position = new Vector2(targetX, ball.transform.position.y);
        _isAligning = false;

        // Drop into column
        int col = ball.TargetColumn;
        int targetRow = _columnHeights[col]; 
        float targetY = _dropZoneY + targetRow * _ballSpacing;
        
        Vector3 verticalTarget = new Vector3(ball.transform.position.x, targetY, ball.transform.position.z);

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }
        
        await InterpolateBallPosition(ball, verticalTarget);
        
        _grid[col, targetRow] = ball;
        _columnHeights[col]++;
        _currentBall = null;
        
        CheckMatches();
        SpawnNextBall();

        _isProcessingDrop = false;
    }
    
    // interpolate the ball to the target position
    private async UniTask InterpolateBallPosition(Ball ball, Vector3 targetPos)
    {
        Vector3 startPos = ball.transform.position;
        
        float distance = Vector3.Distance(startPos, targetPos);
        float baseSpeed = 6f;
        float duration = distance / baseSpeed;
        
        if (duration < 0.1f) duration = 0.1f;

        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            ball.transform.position = Vector3.Lerp(startPos, targetPos, t);
            await UniTask.Yield();
        }
        
        ball.transform.position = targetPos;
    }

    // Match checking algorithm
    private void CheckMatches() 
    {
        List<Vector2Int> cellsToClear = new List<Vector2Int>();

        // horizontal
        for (int row = 0; row < 3; row++)
        {
            if (_grid[0, row] != null && _grid[1, row] != null && _grid[2, row] != null)
            {
                if (_grid[0, row].BallColor == _grid[1, row].BallColor &&
                    _grid[1, row].BallColor == _grid[2, row].BallColor)
                {
                    cellsToClear.Add(new Vector2Int(0, row));
                    cellsToClear.Add(new Vector2Int(1, row));
                    cellsToClear.Add(new Vector2Int(2, row));
                }
            }
        }
        
        // vertical
        for (int col = 0; col < 3; col++)
        {
            if (_grid[col, 0] != null && _grid[col, 1] != null && _grid[col, 2] != null)
            {
                if (_grid[col, 0].BallColor == _grid[col, 1].BallColor &&
                    _grid[col, 1].BallColor == _grid[col, 2].BallColor)
                {
                    cellsToClear.Add(new Vector2Int(col, 0));
                    cellsToClear.Add(new Vector2Int(col, 1));
                    cellsToClear.Add(new Vector2Int(col, 2));
                }
            }
        }
        
        // diagonal
        if (_grid[0, 0] != null && _grid[1, 1] != null && _grid[2, 2] != null)
        {
            if (_grid[0, 0].BallColor == _grid[1, 1].BallColor &&
                _grid[1, 1].BallColor == _grid[2, 2].BallColor)
            {
                cellsToClear.Add(new Vector2Int(0, 0));
                cellsToClear.Add(new Vector2Int(1, 1));
                cellsToClear.Add(new Vector2Int(2, 2));
            }
        }
        
        if (_grid[2, 0] != null && _grid[1, 1] != null && _grid[0, 2] != null)
        {
            if (_grid[2, 0].BallColor == _grid[1, 1].BallColor &&
                _grid[1, 1].BallColor == _grid[0, 2].BallColor)
            {
                cellsToClear.Add(new Vector2Int(2, 0));
                cellsToClear.Add(new Vector2Int(1, 1));
                cellsToClear.Add(new Vector2Int(0, 2));
            }
        }
        
        List<Vector2Int> uniqueCells = new List<Vector2Int>();
        
        foreach (var cell in cellsToClear)
        {
            if (!uniqueCells.Contains(cell))
                uniqueCells.Add(cell);
        }

        if (uniqueCells.Count > 0)
        {
            var matchColor = _grid[uniqueCells[0].x, uniqueCells[0].y].BallColor;
            int points = _colorScoreMapping[matchColor] * (uniqueCells.Count / 3);
            _score += points;
            _logger.Log("Current score:" + _score);
            
            var effectPosition = Vector3.zero;
            
            foreach (var cell in uniqueCells)
            {
                Ball ballRef = _grid[cell.x, cell.y];
                
                if (ballRef != null)
                {
                    effectPosition += ballRef.transform.position;
                }
            }
            
            effectPosition /= uniqueCells.Count;
            effectPosition.z = -1.0f;
            
            if (_particleEffectPrefab != null)
            {
                InstantiateEffect(_particleEffectPrefab, effectPosition).Forget();
            }
            
            foreach (var cell in uniqueCells)
            {
                var ballToRemove = _grid[cell.x, cell.y];
                
                if (ballToRemove != null)
                {
                    Destroy(ballToRemove.gameObject);
                    _grid[cell.x, cell.y] = null;
                }
            }
            
            for (int col = 0; col < 3; col++)
            {
                var ballsInColumn = new List<Ball>();
                
                for (int row = 0; row < 3; row++)
                {
                    if (_grid[col, row] != null)
                        ballsInColumn.Add(_grid[col, row]);
                }
                
                for (int row = 0; row < 3; row++)
                {
                    _grid[col, row] = null;
                }
                
                for (int i = 0; i < ballsInColumn.Count; i++)
                {
                    _grid[col, i] = ballsInColumn[i];
                    float newX = _columnXPositions[col];
                    float newY = _dropZoneY + i * _ballSpacing;
                    ballsInColumn[i].transform.position = new Vector2(newX, newY);
                }
                
                _columnHeights[col] = ballsInColumn.Count;
            }
        }
    }
    
    private async UniTask InstantiateEffect(GameObject effectPrefab, Vector3 position)
    {
        var effect = InstantiateAsync(effectPrefab, position, Quaternion.identity);
        await UniTask.Delay(1000);
        UniTask.WaitUntil(()=>effect.isDone);
        Destroy(effect.Result[0]);
    }
}
