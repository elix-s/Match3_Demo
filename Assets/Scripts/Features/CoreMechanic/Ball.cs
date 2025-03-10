using UnityEngine;

public enum BallColor { Red, Green, Blue }

[RequireComponent(typeof(Rigidbody2D)), RequireComponent(typeof(SpriteRenderer))]
public class Ball : MonoBehaviour 
{
    public BallColor BallColor { get; set; } = BallColor.Red;
    public bool IsReleased { get; set; } = false;
    public int TargetColumn = -1;
    
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    
    void Start() 
    {
        _rb.isKinematic = true;
    }
    
    public void SetBallColor(BallColor color) 
    {
        BallColor = color;
        
        if (_spriteRenderer != null)
        {
            if (color == BallColor.Red)
                _spriteRenderer.color = Color.red;
            else if (color == BallColor.Green)
                _spriteRenderer.color = Color.green;
            else if (color == BallColor.Blue)
                _spriteRenderer.color = Color.blue;
        }
    }
    
    public void ReleaseBall(int column, float releaseForce) 
    {
        if (IsReleased) return;
        
        IsReleased = true;
        TargetColumn = column;
        transform.SetParent(null); 
        _rb.isKinematic = false;
        _rb.gravityScale = 1f;
        _rb.AddForce(Vector2.down * releaseForce, ForceMode2D.Impulse);
    }
}

