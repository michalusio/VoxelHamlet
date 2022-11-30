using Assets.Scripts.Entities.Objects;
using UnityEngine;

namespace Assets.Scripts.Entities.Templates
{
    [CreateAssetMenu(fileName = "CrateTemplate", menuName = "CrateTemplate", order = 4)]
    public class CrateTemplate : EntityTemplate<CrateTemplate, Crate>
    {
        public int CrateMaxItemCount = 20;
    }
}
