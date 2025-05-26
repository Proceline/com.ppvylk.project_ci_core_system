using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.AilmentSystem;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.AbilityParams;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.GUI
{
    public class SlotDataUIElementBase : MonoBehaviour
    {
        protected AbilityListUIElementBase Owner { get; private set; }
        public virtual string DisplayName { get; set; }

        public void SetOwner(AbilityListUIElementBase InListUIElem)
        {
            Owner = InListUIElem;
        }

        public virtual void SetAbility(UnitAbilityCore InAbility, int InIndex)
        {
            // Do nothing
        }

        public virtual void ClearAbility()
        {
            // Do nothing
        }

        public virtual void OnClicked()
        {
            // Do nothing
        }

        public virtual void OnHover()
        {
            // Do nothing
        }
    }
}
