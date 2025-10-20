using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Core.Utils
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class NonDrawingGraphic : Graphic
    {
        public override void SetMaterialDirty()
        {
            return;
        }

        public override void SetVerticesDirty()
        {
            return;
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            return;
        }
    }
}