using Assets.Scripts.Entities.Objects;
using UnityEngine;

namespace Assets.Scripts.Entities.Templates
{
    [CreateAssetMenu(fileName = "FenceTemplate", menuName = "FenceTemplate", order = 5)]
    public class FenceTemplate : EntityTemplate<FenceTemplate, Fence>
    {
    }
}
