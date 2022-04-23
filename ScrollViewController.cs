using System;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using Cysharp.Threading.Tasks;

public class ScrollViewController : MonoBehaviour
{
    [SerializeField] private Transform contentTr;
    [SerializeField] private Scrollbar scrollbar;
    [SerializeField] private GameObject scrollNode;

    [SerializeField] private int maxNumber = 100; // Buttonの最大数
    public int MaxNumber => maxNumber;

    public int CreatedNumber { get; private set; }

    public bool NoNeedScrollbar => CreatedNumber == maxNumber;

    // 現時点のButton群の先頭の番号
    public IReadOnlyReactiveProperty<int> Current => current;
    private ReactiveProperty<int> current = new ReactiveProperty<int>();

    public void SetCurrent(int current)
    {
        this.current.Value = current;
    }

    // 初期化完了の通知
    public IObservable<Unit> OnInitializedAsync => initializedAsyncSubject;
    private AsyncSubject<Unit> initializedAsyncSubject = new AsyncSubject<Unit>();

    public int   Final  { get; private set; } // 最後方のScrollNode群の先頭の番号
    public float Height { get; private set; } // 全体の高さ

    private float GetRectHeight(RectTransform rectTr) => rectTr.offsetMax.y - rectTr.offsetMin.y;

    private Text[] texts;

    private void Start()
    {
        var scrollViewTr = transform as RectTransform;
        Height = GetRectHeight(scrollViewTr);

        var scrollNodeTr = scrollNode.transform as RectTransform;
        var nodeHeight = GetRectHeight(scrollNodeTr);

        CreatedNumber = (int)(Height / nodeHeight) + 1;
        if (maxNumber < CreatedNumber) CreatedNumber = maxNumber;

        texts = new Text[CreatedNumber];
        for (int i = 0; i < CreatedNumber; i++)
        {
            var createdTreeNode = Instantiate<GameObject>(scrollNode, contentTr);
            texts[i] = createdTreeNode.GetComponentInChildren<Text>();
            texts[i].text= i.ToString();
        }

        Final = maxNumber - CreatedNumber + 1;

        // 次に進む
        scrollbar.ObserveEveryValueChanged(x => x.value)
            .Where(_ => !NoNeedScrollbar)
            .Where(x => x < 0f)
            .Where(_ => current.Value != Final)
            .Subscribe(x =>
            {
                scrollbar.value = 1;
                current.Value++;
            })
            .AddTo(this);

        // 前に戻る
        scrollbar.ObserveEveryValueChanged(x => x.value)
            .Where(_ => !NoNeedScrollbar)
            .Where(x => x > 1f)
            .Where(_ => current.Value != 0)
            .Subscribe(x =>
            {
                scrollbar.value = 0;
                current.Value--;
            })
            .AddTo(this);

        current
            .Where(_ => !NoNeedScrollbar)
            .Subscribe(_ => SetNumber())
            .AddTo(this);

        initializedAsyncSubject.OnNext(Unit.Default);
        initializedAsyncSubject.OnCompleted();
    }

    private void SetNumber()
    {
        for (int i = current.Value; i < current.Value + CreatedNumber; i++)
        {
            var j = i - current.Value;
            texts[j].text = i.ToString();
        }
    }
}
