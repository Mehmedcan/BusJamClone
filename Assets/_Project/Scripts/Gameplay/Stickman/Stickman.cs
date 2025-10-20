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
        
        private static readonly int IsWalk = Animator.StringToHash("isWalk");
        private static readonly int IsIdle = Animator.StringToHash("isIdle");

        public void Initialize(HumanType type, Material typeMaterial)
        {
            _type = type;
            
            var meshMaterials = bodySkinnedMeshRenderer.materials;
            meshMaterials[0] = typeMaterial; // the first material is the stickman body
            bodySkinnedMeshRenderer.materials = meshMaterials;
        }

        public UniTask MoveStickmanToPosition(Transform targetTransform, float duration)
        {
            SetWalkAnimation(true);
            
            var movePosition = new Vector3( targetTransform.position.x, transform.position.y, targetTransform.position.z);
            return transform.DOMove(movePosition, duration).OnComplete(() =>
            {
                SetWalkAnimation(false);
            }).ToUniTask();
        }
        
        private void SetWalkAnimation(bool isWalking)
        {
            animator.SetBool(IsWalk, isWalking);
            animator.SetBool(IsIdle, !isWalking);
        }
    }
}
