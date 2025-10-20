using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Core.Utils
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Image))]
    public class BackgroundImageFitter : MonoBehaviour
    {
        [SerializeField, Range(0.1f, 5f)]
        private float scaleMultiplier = 1f;

        private Image _backgroundImage;
        private RectTransform _parentRect;
        private Vector2 _parentSize;
        private Vector2 _imageSize;
        private float _parentAspect;
        private float _imageAspect;

        private void Start()
        {
            FitImageToParent();
        }
        
        private void FitImageToParent()
        {
            if (_backgroundImage == null)
                _backgroundImage = GetComponent<Image>();

            if (transform.parent == null)
            {
                return;
            }

            _parentRect = transform.parent.GetComponent<RectTransform>();
            if (_parentRect == null)
            {
                return;
            }

            _parentSize = _parentRect.rect.size;
            _parentAspect = _parentSize.x / _parentSize.y;

            _backgroundImage.SetNativeSize();
            _backgroundImage.preserveAspect = true;

            _imageSize = _backgroundImage.rectTransform.sizeDelta;
            _imageAspect = _imageSize.x / _imageSize.y;

            var scaleFactor = _parentAspect > _imageAspect
                ? _parentSize.x / _imageSize.x
                : _parentSize.y / _imageSize.y;

            _backgroundImage.rectTransform.sizeDelta = _imageSize * (scaleFactor * scaleMultiplier);
        }
    }
}