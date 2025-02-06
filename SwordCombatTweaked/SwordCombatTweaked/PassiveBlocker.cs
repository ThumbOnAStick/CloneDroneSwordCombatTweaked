using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SwordCombatTweaked
{
    public class PassiveBlocker
    {
        private int usage;

        public PassiveBlocker()
        {
            usage = 0;
        }

        public PassiveBlocker(int usage)
        {
            this.usage = usage;
        }

        public int Usage => usage;

        public bool TryToUseBlocker(string name = "")
        {
            Debug.Log($"Blocker used for {name}");
            if(this.usage > 0)
            {
                usage--;
                return true;
            }
            return false;
        }
    }
}
