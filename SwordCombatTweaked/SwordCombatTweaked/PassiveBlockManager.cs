using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SwordCombatTweaked
{
    public static class PassiveBlockManager
    {
        public static Dictionary<int, PassiveBlocker> blockerRegister = 
            new Dictionary<int, PassiveBlocker>();

        public static PassiveBlocker GenerateBlockerFor(FirstPersonMover client)
        {
            int usage = 0;
            int spawnCost = client.SpawnCost;
            if (spawnCost > 500)
            {
                usage = 3;
            }
            else if (spawnCost >= 300)
            {
                usage = 1;
            }
            return new PassiveBlocker(usage);
        }

        public static bool CanRegisterNow(FirstPersonMover client)
        {

            return true;
        }

        public static void TryToRegister(FirstPersonMover client)
        {
            if (CanRegisterNow(client))
            {
                Register(client);
            }
        }

        public static void Register(FirstPersonMover client)
        {
            var id = client.gameObject.GetInstanceID();
            if (!blockerRegister.ContainsKey(id))
            {
                blockerRegister.Add(id, GenerateBlockerFor(client));

            }
        }
        public static void Register(int id, PassiveBlocker blocker)
        {
            if (!blockerRegister.ContainsKey(id))
            {
                blockerRegister.Add(id, blocker);
            }
            else
            {
                blockerRegister[id] = blocker;
            }
        }

        public static void RemoveFromTable(FirstPersonMover client)
        {
            if (!blockerRegister.ContainsKey(client.gameObject.GetInstanceID()))
            {
                blockerRegister.Remove(client.gameObject.GetInstanceID());

            }
        }

        public static bool TryToUse(FirstPersonMover client)
        {  
            var id = client.gameObject.GetInstanceID();
            if (!blockerRegister.ContainsKey(id) || blockerRegister[id] == null)
            {
                Register(id, GenerateBlockerFor(client));
            }
            return blockerRegister[id].TryToUseBlocker();
            
            //Debug.Log($"block register does not exist for {client.gameObject.name}");
            //return false;
        }
        public static bool TryToUse(FirstPersonMover client, out int remains)
        {
            var id = client.gameObject.GetInstanceID();
            if (!blockerRegister.ContainsKey(id) || blockerRegister[id] == null)
            {
                Register(id, GenerateBlockerFor(client));
            }
            remains = blockerRegister[id].Usage - 1;
            return blockerRegister[id].TryToUseBlocker();

        }

        public static void BounceOpponent(FirstPersonMover owner)
        {
            owner.PlayBlockProjectileAnimation();
        }

        public static void BounceSelf(ref SwordHitArea bladeCutArea, FirstPersonMover opponent)
        {

            if (!BoltNetwork.IsSinglePlayer)
            {
                return;
            }
            if (opponent == null || !opponent.IsAlive() || opponent.HasFallenDown())
            {
                return;
            }
            BlockSwordEvent blockSwordEvent = null;
            bladeCutArea.SetDamageActive(false);
            bladeCutArea.Owner.OnSwordHitSword(opponent, blockSwordEvent, localIsPlayer1: true);
        }
    }

}
