using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;

public class DamageTxt : MonoBehaviour
{
    private StringBuilder _sb;
    private TMP_Text _text;
    
    private void Awake()
    {
        TryGetComponent(out _text);
    }

    public void Init(StringBuilder sb)
    {
        _sb = sb;
    }
    
    public void SetText(int damage, bool isCrit)
    {
        _sb.Clear();
        _sb.Append(damage);
        _text.SetText(_sb);
        if (isCrit)
        {
            _text.fontSize = 40f;
            _text.color = Color.yellow;
        }
        else
        {
            _text.fontSize = 36f;
            _text.color = Color.white;
        }

        StartCoroutine(AnimateText());
    }

    private IEnumerator AnimateText()
    {
        float elapsedTime = 0f;
        float duration = 1f;
        Vector3 startPos = transform.position;
        Color startColor = _text.color;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            transform.position = startPos + Vector3.up * (t * 1.5f);
            
            _text.color = new Color(startColor.r, startColor.g, startColor.b, 1f - t);
            yield return null;
        }
        
        ObjectPoolingManager.Instance.ReleaseDamageTxt(this);
    }
}
