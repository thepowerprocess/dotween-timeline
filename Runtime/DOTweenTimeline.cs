using DG.Tweening;
using UnityEngine;

namespace Dott
{
    [AddComponentMenu("DOTween/DOTween Timeline")]
    public class DOTweenTimeline : MonoBehaviour
    {
        [SerializeField] private bool autoPlay = true;

        private void OnValidate()
        {
            foreach (var doTweenAnimation in GetComponents<DOTweenAnimation>())
            {
                doTweenAnimation.autoPlay = autoPlay;
            }
        }
    }
}