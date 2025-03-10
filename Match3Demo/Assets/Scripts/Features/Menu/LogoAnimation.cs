using UnityEngine;
using TMPro;
using DG.Tweening;

public class LogoAnimation : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private TextMeshProUGUI _leftText; 
    [SerializeField] private TextMeshProUGUI _rightText; 

    [Header("Animation parameters")]
    [SerializeField] private float _moveDuration = 1.0f; 
    [SerializeField] private float _offsetX = 500f;     

    [Header("Shake parameters")]
    [SerializeField] private float _shakeDuration = 0.5f;   
    [SerializeField] private float _shakeStrength = 50f;     
    [SerializeField] private int _shakeVibration = 10;         
    [SerializeField] private float _shakeRandomness = 90f;    
    
    private Vector2 _leftTargetPosition;
    private Vector2 _rightTargetPosition;

    public void Init()
    {
        _leftTargetPosition = _leftText.rectTransform.anchoredPosition;
        _rightTargetPosition = _rightText.rectTransform.anchoredPosition;
        
        _leftText.rectTransform.anchoredPosition = _leftTargetPosition + new Vector2(-_offsetX, 0);
        _rightText.rectTransform.anchoredPosition = _rightTargetPosition + new Vector2(_offsetX, 0);
        
        Sequence sequence = DOTween.Sequence();
        
        sequence.Append(_leftText.rectTransform.DOAnchorPos(_leftTargetPosition, _moveDuration).SetEase(Ease.OutCubic));
        sequence.Join(_rightText.rectTransform.DOAnchorPos(_rightTargetPosition, _moveDuration).SetEase(Ease.OutCubic));
        
        sequence.AppendCallback(() =>
        {
            _leftText.rectTransform.DOShakeAnchorPos(_shakeDuration, _shakeStrength, _shakeVibration, _shakeRandomness, false, true);
            _rightText.rectTransform.DOShakeAnchorPos(_shakeDuration, _shakeStrength, _shakeVibration, _shakeRandomness, false, true);
        });
        
        sequence.AppendInterval(_shakeDuration);
        
        sequence.AppendCallback(() =>
        {
            _leftText.rectTransform.anchoredPosition = _leftTargetPosition;
            _rightText.rectTransform.anchoredPosition = _rightTargetPosition;
        });
        
        sequence.Play();
    }
}
