using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamageFont : MonoBehaviour
{
    private TextMeshPro _damageText;

    public void SetInfo(Vector2 pos, float damage = 0, Transform parent = null, bool isCritical = false)
    {
        // 컴포넌트 가져와서 색상변경
        _damageText = GetComponent<TextMeshPro>();
        _damageText.sortingOrder = SortingLayers.PROJECTILE;

        transform.position = pos;

        if (damage < 0)
        {
            _damageText.color = Util.HexToColor("4EEE6F");
        }
        else if (isCritical)
        {
            _damageText.color = Util.HexToColor("EFAD00");
        }
        else
        {
            _damageText.color = Color.red;
        }

        _damageText.text = $"{Mathf.Abs(damage)}";
        _damageText.alpha = 1;

        if (parent != null)
            GetComponent<MeshRenderer>().sortingOrder = SortingLayers.DAMAGE_FONT;

        DoAnimation();
    }

    private void DoAnimation()
    {
        Sequence seq = DOTween.Sequence();

        transform.localScale = new Vector3(0, 0, 0);

        // join : 동시에 실행할 다른 애니메이션
        seq.Append(transform.DOScale(1.3f, 0.3f).SetEase(Ease.InOutBounce)). // 스케일 변화 (1.3만큼 0.3초) , InOutBounce 팅기는 애니메이션 이징효과
            Join(transform.DOMove(transform.position + Vector3.up, 0.3f).SetEase(Ease.Linear))  // 위쪽으로 선형이동 애니메이션
            .Append(transform.DOScale(1.0f, 0.3f).SetEase(Ease.InOutBounce)) // 이전 애니메이션이 끝난 후, 재생할 애니메이션 (다시 원래 크기로)
            .Join(transform.GetComponent<TMP_Text>().DOFade(0, 0.3f).SetEase(Ease.InQuint)) // 여기에 알파값을 0으로 만드는 페이드 애니메이션 추가
            .OnComplete(() => // 애니메이션 완료시 사용할 람다
            {
                Managers.Resource.Destroy(gameObject);  // 자기자신 파괴
            });
    }
}
