using UnityEngine;

namespace Faithful
{
    internal class Technician : Survivor
    {
        public Technician()
        {
            // Initialise survivor
            Init("TECHNICIAN", "mdlTechnician", "texTechnicianIcon", _bodyColor: new Color(0.9414f, 0.7578f, 0.1953f), _sortPosition: 25, _maxHealth: 120.0f, _healthRegen: 1.0f, _armour: 0.0f,
                 _shield: 0.0f, _jumpCount: 1, _damage: 12, _attackSpeed: 1.0f, _crit: 1.0f, _moveSpeed: 7.0f, _acceleration: 80.0f, _jumpPower: 15.0f, _aiType: AIType.Commando);
        }
    }
}
