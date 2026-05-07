using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class AutoRotate : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        DOTween.Sequence()
            .Append(transform.DOLocalRotate(new Vector3(0, -180, 0), 10).SetEase(Ease.Linear))
            .Append(transform.DOLocalRotate(new Vector3(0, 0, 0), 10).SetEase(Ease.Linear))
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);
    }

    // Update is called once per frame
    void Update()
    {
    }
}