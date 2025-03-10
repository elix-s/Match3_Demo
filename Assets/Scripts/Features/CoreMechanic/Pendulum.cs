using UnityEngine;

public class Pendulum : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _swingAmplitude = 20f; 
    [SerializeField] private float _swingSpeed = 2f;
    private float _timer;

    void Update()
    {
        _timer += Time.deltaTime;
        float angle = Mathf.Sin(_timer * _swingSpeed) * _swingAmplitude;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}

