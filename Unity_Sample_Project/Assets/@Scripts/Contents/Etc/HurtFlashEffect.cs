using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtFlashEffect : InitBase
{
    private int _flashCount = 1;
    private Color _flashColor = new Color(0.5f, 0, 0);
    private float _interval = 1.0f / 15;
    private string _fillPhaseProperty = "_FillPhase";
    private string _fillColorProperty = "_FillColor";

    MaterialPropertyBlock _mpb; // 머테리얼 속성을 동적으로 설정할 수 있도록 + 별개의 머테리얼로 설정하여 각기 적용되도록
    MeshRenderer _meshRenderer;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        _mpb = new MaterialPropertyBlock();
        _meshRenderer = GetComponent<MeshRenderer>();

        return true;
    }
    public void Flash()
    {
        _meshRenderer.GetPropertyBlock(_mpb);
        StartCoroutine(FlashRoutine());
    }

    IEnumerator FlashRoutine()
    {
        int fillPhase = Shader.PropertyToID(_fillPhaseProperty); // 셰이더 속성들
        int fillColor = Shader.PropertyToID(_fillColorProperty); // 머테리얼 쪽 속성에서 Edit 누르면 속성을 볼 수 있다

        WaitForSeconds wait = new WaitForSeconds(_interval);

        for (int i = 0; i < _flashCount; i++)
        {
            _mpb.SetColor(fillColor, _flashColor);
            _mpb.SetFloat(fillPhase, 1.0f);
            _meshRenderer.SetPropertyBlock(_mpb);
            yield return wait;

            // 다음에 여기로 넘어온다
            _mpb.SetFloat(fillPhase, 0f);
            _meshRenderer.SetPropertyBlock(_mpb);
            yield return wait;
        }

        yield return null;
    }
}
