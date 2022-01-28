using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;

public class ScrollbarConroller : MonoBehaviour
{
    private Button scrollbarButton;
    private RectTransform scrollbarTr;
    private RectTransform scrollViewTr;
    private ScrollViewConroller svc;

    private Vector3 MousePosPrev;
    private Vector3 ScBarPosPrev;

    // スクロールバーの全体
    private float bodyYMax;
    private float bodyYMin;
    private float bodyHeight;

    // スクロールバーのノブ部分
    private float knobHeightMin = 10;

    private bool dragging;

    // TODO:sizeに応じてheightを変える
    private void Start()
    {
        scrollViewTr = transform.parent.transform as RectTransform;
        svc = scrollViewTr.GetComponent<ScrollViewConroller>();
        svc.OnInitializedAsync.Subscribe(_ => Initialize());
    }

    private void Initialize()
    {
        bodyHeight = scrollViewTr.offsetMax.y - scrollViewTr.offsetMin.y;
        var knobHeight = (float)svc.Length / (float)svc.Size * bodyHeight;
        knobHeight = ValidateKnobHeight(knobHeight);

        bodyYMax = scrollViewTr.position.y + scrollViewTr.offsetMax.y;
        bodyYMin = scrollViewTr.position.y + scrollViewTr.offsetMin.y + knobHeight;
        
        scrollbarButton = GetComponent<Button>();
        scrollbarTr     = transform as RectTransform;
        scrollbarTr.sizeDelta = new Vector2(scrollbarTr.sizeDelta.x, knobHeight);

        scrollbarButton
            .OnPointerDownAsObservable()
            .Subscribe(_ =>
            {
                MousePosPrev = Input.mousePosition;
                ScBarPosPrev = scrollbarTr.position;
                dragging = true;
            })
            .AddTo(this);

        scrollbarButton
            .OnPointerUpAsObservable()
            .Subscribe(_ =>
            {
                dragging = false;
            })
            .AddTo(this);

        // バーをドラッグした時にボタンを更新
        scrollbarButton
            .OnDragAsObservable()
            .Subscribe(_ =>
            {
                var bodyYTarget = ScBarPosPrev.y + (Input.mousePosition.y - MousePosPrev.y);
                SetScrollbarPosition(ref bodyYTarget);

                float ratio = (bodyYMax - bodyYTarget) / (bodyHeight - knobHeight);
                svc.SetCurrent((int)(ratio * (float)svc.Final));
            })
            .AddTo(this);

        // マウスでスクロールした時にバーを更新
        svc.Current
            .Where(_ => !dragging)
            .Subscribe(current =>
            {
                ScBarPosPrev = scrollbarTr.position;
                float ratio = (float)current / (float)svc.Final;
                var bodyYTarget = bodyYMax - ratio * (bodyHeight - knobHeight);
                SetScrollbarPosition(ref bodyYTarget);
            });
    }

    private void SetScrollbarPosition(ref float bodyYTarget)
    {
        bodyYTarget = ValidateScrollbarPosition(bodyYTarget);
        scrollbarTr.position = new Vector3(ScBarPosPrev.x, bodyYTarget, 0f);
    }

    private float ValidateScrollbarPosition(float bodyYTarget)
    {
        if (bodyYTarget > bodyYMax)
            return bodyYMax;
        else if (bodyYTarget < bodyYMin)
            return bodyYMin;
        else
            return bodyYTarget;
    }

    private float ValidateKnobHeight(float knobHeight)
    {
        if (knobHeight < knobHeightMin)
            return knobHeightMin;
        else
            return knobHeight;
    }
}
