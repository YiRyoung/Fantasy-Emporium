using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public static class TweenUtils
{
    public static void Run(this MonoBehaviour _host, IEnumerator _routine)
    {
        if (_host != null && _host.isActiveAndEnabled) 
        {
            _host.StartCoroutine(_routine);
        }
    }

    public static IEnumerator Tween(Action<float> _onUpdate, float _duration = 1.0f, Action _onComplete = null)
    {
        if (_duration <= 0.0f)
        {
            _onUpdate?.Invoke(1.0f);
            _onComplete?.Invoke();
            yield break;
        }

        float elapsed = 0.0f;
        while (elapsed < _duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _duration);
            _onUpdate?.Invoke(t);
            yield return null;
        }

        _onComplete?.Invoke();
    }

    public static IEnumerator MoveTo(Transform _target, Vector3 _start, Vector3 _end, float _duration = 1.0f, Action _onComplete = null)
    {
        return Tween(t =>
        {
            if (_target == null) { return; }

            _target.position = Vector3.Lerp(_start, _end, t);
        },
        _duration, _onComplete);
    }
    public static IEnumerator ScaleTo(Transform _target, Vector3 _start, Vector3 _end, float _duration = 1.0f, Action _onComplete = null)
    {
        return Tween(t =>
        {
            if (_target == null) { return; }

            _target.localScale = Vector3.Lerp(_start, _end, t);
        },
        _duration, _onComplete);
    }

    public static IEnumerator RotateTo(Transform _target, Vector3 _start, Vector3 _end, float _duration = 1.0f, Action _onComplete = null)
    {
        return Tween(t =>
        {
            if (_target == null) { return; }

            _target.rotation = Quaternion.Slerp(Quaternion.Euler(_start), Quaternion.Euler(_end), t);
        },
        _duration, _onComplete);
    }

    public static IEnumerator FadeTo(Image _target, float _start, float _end, float _duration = 1.0f, Action _onComplete = null)
    {
        return Tween(t =>
        {
            if (_target == null) { return; }

            var color = _target.color;
            color.a = Mathf.Lerp(_start, _end, t);
            _target.color = color;
        },
        _duration, _onComplete);
    }

    public static IEnumerator FadeTo(CanvasGroup _target, float _start, float _end, float _duration = 1.0f, Action _onComplete = null)
    {
        return Tween(t =>
        {
            if (_target == null) { return; }

            var color = _target.alpha;
            color = Mathf.Lerp(_start, _end, t);
            _target.alpha = color;
        },
        _duration, _onComplete);
    }

    public static IEnumerator ColorTo(SpriteRenderer _target, Color _end, float _duration = 1.0f, Action _onComplete = null)
    {
        return Tween(t =>
        {
            if (_target == null) { return; }

            _target.color = Color.Lerp(_target.color, _end, t);
        },
        _duration, _onComplete);
    }
}