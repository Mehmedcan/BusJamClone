using _Project.Scripts.Core.Utils;
using _Project.Scripts.Gameplay.Human;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Stickman
{
    public class Stickman : MonoBehaviour
    {
        [SerializeField] private SkinnedMeshRenderer bodySkinnedMeshRenderer;
        [SerializeField] private Animator animator;

        private HumanType _type;

        public void Initialize(HumanType type, Material typeMaterial)
        {
            _type = type;
            
            var meshMaterials = bodySkinnedMeshRenderer.materials;
            meshMaterials[0] = typeMaterial; // the first material is the stickman body
            bodySkinnedMeshRenderer.materials = meshMaterials;
        }

        public UniTask MoveStickmanToPosition( Transform targetTransform, float duration)
        {
            return transform.DOMove(targetTransform.position, duration).ToUniTask();
        }
    }
}
