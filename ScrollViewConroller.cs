using System;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using Cysharp.Threading.Tasks;

public class ScrollViewConroller : MonoBehaviour
{
    [SerializeField] private Transform contentTr;
    [SerializeField] private Scrollbar scrollbar;

    private Text[] texts;

    // Œ»“_‚ÌButtonŒQ‚Ìæ“ª‚Ì”Ô†
    public IReadOnlyReactiveProperty<int> Current => current;

    private ReactiveProperty<int> current = new ReactiveProperty<int>(); 

    public void SetCurrent(int current)
    {
        this.current.Value = current;
    }

    // ÅŒã•û‚ÌButtonŒQ‚Ìæ“ª‚Ì”Ô†
    public int Final { get; private set; }  
    
    public int Length { get; private set; } // Button‚Ì”

    public int Size => size;
    [SerializeField] private int size  = 100; // Button‚ÌÅ‘å”

    public IObservable<Unit> OnInitializedAsync => initializedAsyncSubject;
    private AsyncSubject<Unit> initializedAsyncSubject = new AsyncSubject<Unit>();

    private void Start()
    {
        texts = contentTr.GetComponentsInChildren<Text>();
        Length = texts.Length;
        for (int i = 0; i < Length; i++)
        {
            texts[i].text = i.ToString();
        }
        Final = size - Length + 1;

        initializedAsyncSubject.OnNext(Unit.Default);
        initializedAsyncSubject.OnCompleted();

        // Ÿ‚Éi‚Ş
        scrollbar.ObserveEveryValueChanged(x => x.value)
            .Where(x => x < 0f)
            .Where(_ => current.Value != Final)
            .Subscribe(x =>
            {
                scrollbar.value = 1;
                current.Value++;
            })
            .AddTo(this);

        // ‘O‚É–ß‚é
        scrollbar.ObserveEveryValueChanged(x => x.value)
            .Where(x => x > 1f)
            .Where(_ => current.Value != 0)
            .Subscribe(x =>
            {
                scrollbar.value = 0;
                current.Value--;
            })
            .AddTo(this);

        current
            .Subscribe(_ =>
            {
                SetNumber();
            })
            .AddTo(this);
    }

    private void SetNumber()
    {
        for (int i = current.Value; i < current.Value + Length; i++)
        {
            var j = i - current.Value;
            texts[j].text = i.ToString();
        }
    }
}
