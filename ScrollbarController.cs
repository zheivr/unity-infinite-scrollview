using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;

public class ScrollbarController : MonoBehaviour
{
    [SerializeField] private ScrollViewController svController;

    private Button scrollbarButton;
    private RectTransform scrollbarTr;
    private RectTransform scrollViewTr;
    
    private Vector3 MousePosPrev;
    private Vector3 ScBarPosPrev;

    // スクロールバーの全体
    private float bodyYMax;
    private float bodyYMin;
    private float bodyHeight;
    private float bodyYTarget;
    private float BodyYTarget
    {
        get => bodyYTarget;

        set
        {
            bodyYTarget = value;
            if      (bodyYTarget > bodyYMax)
                bodyYTarget = bodyYMax;
            else if (bodyYTarget < bodyYMin)
                bodyYTarget = bodyYMin;
        }
    }

    // スクロールバーのノブ部分
    private float knobHeightMin = 10;
    private float knobHeight;
    private float KnobHeight
    {
        get => knobHeight;

        set
        {
            knobHeight = value;
            if (knobHeight < knobHeightMin)
                knobHeight = knobHeightMin;
        }
    }

    private bool dragging;

    private void Start()
    {
        scrollViewTr = svController.transform as RectTransform;
        svController.OnInitializedAsync.Subscribe(_ => Initialize());
    }

    private void Initialize()
    {
        if (svController.NoNeedScrollbar) gameObject.SetActive(false);

        bodyHeight = svController.Height;
        KnobHeight = 
            (float)svController.CreatedNumber / (float)svController.MaxNumber * bodyHeight;

        bodyYMax = scrollViewTr.position.y + bodyHeight / 2;
        bodyYMin = scrollViewTr.position.y - bodyHeight / 2 + KnobHeight;

        scrollbarButton = GetComponent<Button>();
        scrollbarTr     = transform as RectTransform;
        scrollbarTr.sizeDelta = new Vector2(scrollbarTr.sizeDelta.x, KnobHeight);

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
                BodyYTarget = ScBarPosPrev.y + (Input.mousePosition.y - MousePosPrev.y);
                SetScrollbarPosition(BodyYTarget);

                float ratio = (bodyYMax - BodyYTarget) / (bodyHeight - KnobHeight);
                svController.SetCurrent((int)(ratio * (float)svController.Final));
            })
            .AddTo(this);

        // マウスでスクロールした時にバーを更新
        svController.Current
            .Where(_ => !dragging)
            .Subscribe(current =>
            {
                ScBarPosPrev = scrollbarTr.position;
                float ratio = (float)current / (float)svController.Final;
                BodyYTarget = bodyYMax - ratio * (bodyHeight - KnobHeight);
                SetScrollbarPosition(BodyYTarget);
            });
    }

    private void SetScrollbarPosition(float bodyYTarget)
    {
        scrollbarTr.position = new Vector3(ScBarPosPrev.x, bodyYTarget, 0f);
    }
}
