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

    // �����_��Button�Q�̐擪�̔ԍ�
    public IReadOnlyReactiveProperty<int> Current => current;

    private ReactiveProperty<int> current = new ReactiveProperty<int>(); 

    public void SetCurrent(int current)
    {
        this.current.Value = current;
    }

    // �Ō����Button�Q�̐擪�̔ԍ�
    public int Final { get; private set; }  
    
    public int Length { get; private set; } // Button�̐�

    public int Size => size;
    [SerializeField] private int size  = 100; // Button�̍ő吔

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

        // ���ɐi��
        scrollbar.ObserveEveryValueChanged(x => x.value)
            .Where(x => x < 0f)
            .Where(_ => current.Value != Final)
            .Subscribe(x =>
            {
                scrollbar.value = 1;
                current.Value++;
            })
            .AddTo(this);

        // �O�ɖ߂�
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
